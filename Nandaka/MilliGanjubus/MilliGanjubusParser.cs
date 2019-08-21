using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nandaka.MilliGanjubus
{
    public class MilliGanjubusParser : MilliGanjubusBase, IParser<byte[], IProtocolMessage>
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
            void CheckByteValue(bool condition)
            {
                if (condition)
                {
                    _parserCounter++;
                }
                // If buffer contain 1 byte we don't have to reparse (it guaranted isn't StartByte).
                else if (_buffer.Count > 1)
                {
                    OnParserError();
                }
                // But always need to clear buffer.
                else
                {
                    _buffer.Clear();
                }
            }

            foreach (var byteValue in data)
            {
                _buffer.Add(byteValue);
                switch ((ParsingStage)_parserCounter)
                {
                    case ParsingStage.WaitingStartByte:
                        CheckByteValue(byteValue == StartByte);
                        break;
                    case ParsingStage.WaitingAddress:
                        CheckByteValue(byteValue == AwaitingReplyAddress || byteValue == DirectCastAddress);
                        break;
                    case ParsingStage.WaitingSize:
                        CheckByteValue(byteValue <= MaxPacketLength);
                        break;
                    case ParsingStage.WaitingHeaderCrc:
                        CheckByteValue(byteValue == CheckSum.CRC8(_buffer.GetRange(0, _parserCounter).ToArray()));
                        break;
                    default:
                        if (_parserCounter < _buffer[SizeOffset] - 1)
                        {
                            _parserCounter++;
                        }
                        else if (_buffer[_parserCounter] == CheckSum.CRC8(_buffer.GetRange(0, _parserCounter).ToArray()))
                        {
                            MessageParsed(this, GetProtocolMessage(_buffer.ToArray()));
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

            // What else is needed here? Report at log?
        }

        private IProtocolMessage GetProtocolMessage(byte[] checkedMessage)
        {
            // Find out the type of message from gByte.
            var gByte = checkedMessage[DataOffset];

            switch (gByte)
            {
                case GRequest << 4 | FReadSeries:
                case GRequest << 4 | FReadSingle:
                    return FromSeries(MessageType.ReadDataRequest, false);
                case GRequest << 4 | FReadRange:
                    return FromRange(MessageType.ReadDataRequest, false);
                case GRequest << 4 | FWriteSingle:
                case GRequest << 4 | FWriteSeries:
                    return FromSeries(MessageType.WriteDataRequest, true);
                case GRequest << 4 | FWriteRange:
                    return FromRange(MessageType.WriteDataRequest, true);
                case GReply << 4 | FReadSingle:
                case GReply << 4 | FReadSeries:
                    return FromSeries(MessageType.ReadDataResponse, true);
                case GReply << 4 | FReadRange:
                    return FromRange(MessageType.ReadDataResponse, true);
                case GReply << 4 | FWriteSingle:
                case GReply << 4 | FWriteSeries:
                    return FromSeries(MessageType.WriteDataResponse, false);
                case GReply << 4 | FWriteRange:
                    return FromRange(MessageType.WriteDataResponse, false);
                default:
                    var errorType = (MilliGanjubusErrorType)checkedMessage[DataOffset + 1];
                    return new MilliGanjubusErrorMessage(errorType);
            }

            // Local functions for different types of packet.
            IProtocolMessage FromSeries(MessageType messageType, bool withValues)
            {
                var message = new CommonMessage(messageType, checkedMessage[AddressOffset]);

                // Look through all data bytes except CRC.
                var packetSize = checkedMessage[SizeOffset];
                var byteIndex = MinPacketLength;
                while (byteIndex < packetSize - 1)
                {
                    // todo: add register class and rework this.
                    var register = withValues ?
                        new TestByteRegister(checkedMessage[byteIndex++], checkedMessage[byteIndex++]) :
                        new TestByteRegister(checkedMessage[byteIndex++]);

                    message.AddRegister(register);
                }
                return message;
            }

            IProtocolMessage FromRange(MessageType messageType, bool withValues)
            {
                var message = new CommonMessage(messageType, checkedMessage[AddressOffset]);

                // Ignore gByte.
                var currentByteIndex = DataOffset + 1;

                // Bytes after gByte are a range of addresses.
                var startAddress = checkedMessage[currentByteIndex++];
                var endAddress = checkedMessage[currentByteIndex++];
                var registersCount = endAddress - startAddress;

                foreach (var address in Enumerable.Range(startAddress, registersCount))
                {
                    // todo: add register class and rework this.
                    var register = withValues ?
                        new TestByteRegister(address, checkedMessage[currentByteIndex++]) :
                        new TestByteRegister(address);

                    message.AddRegister(register);
                }
                return message;
            }
        }
    }
}
