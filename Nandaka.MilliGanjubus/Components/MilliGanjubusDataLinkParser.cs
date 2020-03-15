using System.Collections.Generic;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;

namespace Nandaka.MilliGanjubus.Components
{
    public class MilliGanjubusDataLinkParser : DataLinkParserBase<byte[]>
    {
        private enum ParsingStage
        {
            WaitingStartByte = 0,
            WaitingAddress = 1,
            WaitingSize = 2,
            WaitingHeaderCrc = 3,
            WaitingData = 4
        }

        private int _parserCounter = (int)ParsingStage.WaitingStartByte;

        private readonly List<byte> _buffer = new List<byte>(MilliGanjubusBase.MaxPacketLength);

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
                        CheckByteValue(byteValue == MilliGanjubusBase.StartByte);
                        break;

                    case ParsingStage.WaitingAddress:
                        break;

                    case ParsingStage.WaitingSize:
                        CheckByteValue(byteValue <= MilliGanjubusBase.MaxPacketLength);
                        break;

                    case ParsingStage.WaitingHeaderCrc:
                        CheckByteValue(byteValue == CheckSum.Crc8(_buffer.GetRange(0, _parserCounter).ToArray()));
                        break;

                    default:
                        if (_parserCounter < _buffer[MilliGanjubusBase.SizeOffset] - 1)
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

            // Reparse buffered bytes.
            Parse(bufferArray);
        }
    }
}
