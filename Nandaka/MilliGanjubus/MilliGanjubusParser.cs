using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka.MilliGanjubus
{
    public class MilliGanjubusParser : MilliGanjubusProtocolInfo, IParser<byte[]>
    {
        public event EventHandler<IProtocolMessage> MessageParsed;

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
        private readonly List<byte> _buffer = new List<byte>(MaxPacketLength);

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
                _buffer.Add(value);
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

            if (_buffer.Count > 1)
            {
                // Reparse all buffer without first element (because it's previous StartByte).
                var bufferArray = _buffer.GetRange(1, _buffer.Count - 1).ToArray();
                
                // Clear buffer for new packet.
                _buffer.Clear();

                // Reparse buffered bytes.
                Parse(bufferArray);
            }
            // What else is needed here? Report at log?
        }

        private IProtocolMessage GetTransferData(byte[] checkedMessage)
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
