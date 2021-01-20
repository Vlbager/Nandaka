using System.Collections.Generic;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Util;

namespace Nandaka.MilliGanjubus.Components
{
    internal class MgDataLinkParser : DataLinkParserBase<byte[]>
    {
        private enum ParsingStage
        {
            WaitingStartByte = 0,
            WaitingAddress = 1,
            WaitingSize = 2,
            WaitingHeaderCrc = 3,
            WaitingData = 4
        }

        private readonly MgInfo _info;

        private int _parserCounter;

        private readonly List<byte> _buffer;

        public MgDataLinkParser()
        {
            _info = MgInfo.Instance;
            _parserCounter = (int)ParsingStage.WaitingStartByte;
            _buffer = new List<byte>(_info.MaxPacketLength);
        }

        public override void Parse(byte[] data)
        {
            // Local function for assert byte value.
            void CheckByteValue(bool condition)
            {
                if (condition)
                    _parserCounter++;
                // If buffer contain 1 byte we don't have to reparse (it guaranted isn't StartByte).
                else if (_buffer.Count > 1)
                    OnParserError();
                // But always need to clear buffer.
                else
                    _buffer.Clear();
            }

            foreach (byte byteValue in data)
            {
                _buffer.Add(byteValue);
                switch ((ParsingStage)_parserCounter)
                {
                    case ParsingStage.WaitingStartByte:
                        CheckByteValue(byteValue == _info.StartByte);
                        break;

                    case ParsingStage.WaitingAddress:
                        _parserCounter++;
                        break;

                    case ParsingStage.WaitingSize:
                        CheckByteValue(byteValue <= _info.MaxPacketLength);
                        break;

                    case ParsingStage.WaitingHeaderCrc:
                        CheckByteValue(byteValue == CheckSum.Crc8(_buffer.GetRange(0, _parserCounter).ToArray()));
                        break;
                    
                    default:
                        if (_parserCounter < _buffer[_info.SizeOffset] - 1)
                        {
                            _parserCounter++;
                        }
                        else if (_buffer[_parserCounter] == CheckSum.Crc8(_buffer.GetRange(0, _parserCounter).ToArray()))
                        {
                            OnMessageParsed(_buffer.ToArray());
                            _parserCounter = (int)ParsingStage.WaitingStartByte;
                            _buffer.Clear();
                        }
                        else
                        {
                            OnParserError();
                        }
                        break;
                }
            }
        }

        private void OnParserError()
        {
            _parserCounter = (int)ParsingStage.WaitingStartByte;

            // Reparse all buffer without first element (because it's previous StartByte).
            var bufferArray = _buffer.GetRange(1, _buffer.Count - 1).ToArray();

            // Clear buffer for new packet.
            _buffer.Clear();

            Parse(bufferArray);
        }
    }
}
