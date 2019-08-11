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
            // Local function for assert byte value.
            void checkByteValue(bool condition)
            {
                if (condition)
                {
                    _parserCounter++;
                }
                else
                {
                    OnParserError();
                }
            }

            foreach (var value in data)
            {
                AddByteToBuffer(value);
                switch ((ParsingStage)_parserCounter)
                {
                    case ParsingStage.WaitingStartByte:
                        checkByteValue(value == StartByte);
                        break;
                    case ParsingStage.WaitingAddress:
                        checkByteValue(value == AwaitingReplyAddress || value == DirectCastAddress);
                        break;
                    case ParsingStage.WaitingSize:
                        checkByteValue(value <= MaxPacketLength);
                        break;
                    case ParsingStage.WaitingHeaderCrc:
                        checkByteValue(value == CheckSum.CRC8(_buffer.ToArray()));
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

                // Reparse buffered bytes.
                Parse(bufferArray);
            }
            // What else is needed here? Report at log?
        }

        private ITransferData GetTransferData(byte[] checkedMessage)
        {
            var ackNibble = checkedMessage[DataOffset] >> 4;
            switch (ackNibble)
            {
                case GRequest:
                    // aaand what difference with GAcknowledge???
                    // RequestTransferData and ReplyTransferData classes?
                    // Or request-reply extra field in ITransferData interface?
                    throw new NotImplementedException();
                case GReply:
                    throw new NotImplementedException();
                case GError:
                    // todo: create and return Error class, that implement ITransferData.
                    throw new NotImplementedException();
            }

            var fNibble = checkedMessage[DataOffset] & 0xF;
            switch (fNibble)
            {
                case FWriteSingle:
                case FWriteSeries:
                    // todo: create SingleRegister class
                    throw new NotImplementedException();
                case FWriteRange:
                    // todo: create RangeRegister class.
                    throw new NotImplementedException();
                case FReadSingle:
                case FReadSeries:
                    throw new NotImplementedException();
                case FReadRange:
                    throw new NotImplementedException();
            }
            throw new NotImplementedException();
        }

    }
}
