using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Nandaka.MilliGanjubus
{
    public class MilliGanjubusApplicationParser : ApplicationParserBase<byte[]>
    {
        public MilliGanjubusApplicationParser() : base(new MilliGanjubusDataLinkParser())
        {
        }

        protected override IProtocolMessage ApplicationParse(byte[] data)
        {
            // Find out the type of message from gByte.
            var gByte = data[MilliGanjubusBase.DataOffset];

            // Left nibble is ack code. Right - F code.
            switch (gByte >> 4)
            {
                case MilliGanjubusBase.GRequest:
                    switch (gByte & 0xF)
                    {
                        case MilliGanjubusBase.FReadSeries:
                        case MilliGanjubusBase.FReadSingle:
                            return FromSeries(MessageType.ReadDataRequest, false);
                        case MilliGanjubusBase.FReadRange:
                            return FromRange(MessageType.ReadDataRequest, false);
                        case MilliGanjubusBase.FWriteSingle:
                        case MilliGanjubusBase.FWriteSeries:
                            return FromSeries(MessageType.WriteDataRequest, true);
                        case MilliGanjubusBase.FWriteRange:
                            return FromRange(MessageType.WriteDataRequest, true);
                    }
                    throw new ArgumentException();
                case MilliGanjubusBase.GReply:
                    switch (gByte & 0xF)
                    {
                        case MilliGanjubusBase.FReadSingle:
                        case MilliGanjubusBase.FReadSeries:
                            return FromSeries(MessageType.ReadDataResponse, true);
                        case MilliGanjubusBase.FReadRange:
                            return FromRange(MessageType.ReadDataResponse, true);
                        case MilliGanjubusBase.FWriteSingle:
                        case MilliGanjubusBase.FWriteSeries:
                            return FromSeries(MessageType.WriteDataResponse, false);
                        case MilliGanjubusBase.FWriteRange:
                            return FromRange(MessageType.WriteDataResponse, false);
                    }
                    throw new ArgumentException();
                default:
                    var errorType = (MilliGanjubusErrorType)data[MilliGanjubusBase.DataOffset + 1];
                    return new MilliGanjubusErrorMessage(errorType);
            }

            // Local functions for different types of packet.
            IProtocolMessage FromSeries(MessageType messageType, bool withValues)
            {
                var message = new CommonMessage(messageType, data[MilliGanjubusBase.AddressOffset]);

                // Look through all data bytes except CRC.
                var packetSize = data[MilliGanjubusBase.SizeOffset];
                var byteIndex = MilliGanjubusBase.MinPacketLength;
                while (byteIndex < packetSize - 1)
                {
                    // todo: add register class and rework this.
                    var register = withValues ?
                        new TestByteRegister(data[byteIndex++], data[byteIndex++]) :
                        new TestByteRegister(data[byteIndex++]);

                    message.AddRegister(register);
                }
                return message;
            }

            IProtocolMessage FromRange(MessageType messageType, bool withValues)
            {
                var message = new CommonMessage(messageType, data[MilliGanjubusBase.AddressOffset]);

                // Ignore gByte.
                var currentByteIndex = MilliGanjubusBase.DataOffset + 1;

                // Bytes after gByte are a range of addresses.
                var startAddress = data[currentByteIndex++];
                var endAddress = data[currentByteIndex++];
                var registersCount = endAddress - startAddress;

                foreach (var address in Enumerable.Range(startAddress, registersCount))
                {
                    // todo: add register class and rework this.
                    var register = withValues ?
                        new TestByteRegister(address, data[currentByteIndex++]) :
                        new TestByteRegister(address);

                    message.AddRegister(register);
                }
                return message;
            }
        }
    }
}
