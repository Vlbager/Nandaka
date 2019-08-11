using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    public class MilliGanjubusParser : MilliGanjubusProtocolInfo, IParser<byte[]>
    {
        public event EventHandler<ITransferData> MessageParsed;

        public int AwaitingReplyAddress { get; set; }

        // Maybe move this enum in separate .cs file, and GeneralGanjubus can use it too?
        private enum ParsingStage
        {
            WaitingStartByte = 0,
            WaitingAddress = 1,
            WaitingSize = 2,
            WaitingHeaderCrc = 3,
            WaitingData = 4
        }

        private int _parserCounter = (int)ParsingStage.WaitingStartByte;

        // Visual Studio recommended make it readonly...
        private readonly List<byte> _buffer = new List<byte>();
        private bool _isStartByteFinded = false;

        public void Parse(byte[] data)
        {
            foreach (var value in data)
            {
                AddByteToBuffer(value);
                switch ((ParsingStage)_parserCounter)
                {
                    case ParsingStage.WaitingStartByte:
                        if (value == StartByte)
                        {
                            _parserCounter++;
                        }
                        else
                        {
                            OnParserError();
                        }
                        break;
                    case ParsingStage.WaitingAddress:
                        if (value == AwaitingReplyAddress || value == DirectCastAddress)
                        {
                            _parserCounter++;
                        }
                        else
                        {
                            OnParserError();
                        }
                        break;
                    case ParsingStage.WaitingSize:
                        if (value <= MaxPacketLength)
                        {
                            _parserCounter++;
                        }
                        else
                        {
                            OnParserError();
                        }
                        break;
                    case ParsingStage.WaitingHeaderCrc:
                        if (value == CheckSum.CRC8(_buffer.ToArray()))
                        {
                            _parserCounter++;
                        }
                        else
                        {
                            OnParserError();
                        }
                        break;
                    default:
                        if (_parserCounter < _buffer[SizeOffset] - 1)
                        {
                            _parserCounter++;
                        }
                        else if (_buffer[_parserCounter] == CheckSum.CRC8(_buffer.GetRange(0, _parserCounter).ToArray()))
                        {
                            MessageParsed(this, GetTransferData(_buffer.ToArray()));
                            _parserCounter = (int)ParsingStage.WaitingStartByte;
                            _isStartByteFinded = false;
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

        // Buffer must be stored all bytes from the last unchecked StartByte-value.
        private void AddByteToBuffer(byte value)
        {
            if (!_isStartByteFinded && value == StartByte)
            {
                _isStartByteFinded = true;
            }
            else if (!_isStartByteFinded)
            {
                return;
            }
            _buffer.Add(value);
        }

        private void OnParserError()
        {
            _parserCounter = (int)ParsingStage.WaitingStartByte;

            // If buffer isn't empty then there is missed start byte.
            if (_buffer.Count > 0)
            {
                // Clear buffer before start reparse.
                var bufferArray = _buffer.ToArray();
                _isStartByteFinded = false;
                _buffer.Clear();

                Parse(bufferArray);
            }
            // What else is needed here? Report at log?
        }

        private ITransferData GetTransferData(byte[] checkedMessage)
        {
            // todo: What kind of result I should return if in Ack GError?

            throw new NotImplementedException();
        }

    }
}
