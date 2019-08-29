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

        protected override IMessage ApplicationParse(byte[] data)
        {
            var deviceAddress = data[MilliGanjubusBase.AddressOffset];

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
                case MilliGanjubusBase.GError:
                    var errorCode = data[MilliGanjubusBase.DataOffset + 1];
                    return new MilliGanjubusMessage(MessageType.ErrorMessage, deviceAddress, errorCode);
                default:
                    return new MilliGanjubusMessage(MessageType.ApplicationDataError, 
                        deviceAddress, (int)MilliGanjubusErrorType.WrongGByte);
            }

            // Local functions for different types of packet.
            IMessage FromSeries(MessageType messageType, bool withValues)
            {
                // Look through all data bytes except CRC.
                var packetSize = data[MilliGanjubusBase.SizeOffset];
                var byteIndex = MilliGanjubusBase.MinPacketLength;

                // If message with values, then packet size should be an odd number.
                if (withValues && packetSize % 2 != 0)
                {
                    return new MilliGanjubusMessage(MessageType.ApplicationDataError,
                            deviceAddress, (int)MilliGanjubusErrorType.WrongDataAmount);
                }

                var message = new MilliGanjubusMessage(messageType, data[MilliGanjubusBase.AddressOffset]);

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

            IMessage FromRange(MessageType messageType, bool withValues)
            {
                // Ignore gByte.
                var currentByteIndex = MilliGanjubusBase.DataOffset + 1;

                // Bytes after gByte are a range of addresses.
                var startAddress = data[currentByteIndex++];
                var endAddress = data[currentByteIndex++];
                var registersCount = endAddress - startAddress + 1;

                // Check addresses validity.
                if (startAddress > endAddress)
                {
                    return new MilliGanjubusMessage(MessageType.ApplicationDataError, 
                        deviceAddress, (int)MilliGanjubusErrorType.WrongRegisterAddress);
                }

                // Check registers count is valid number (less than max data length minus addess bytes).
                if (registersCount > MilliGanjubusBase.MaxDataLength - 2)
                {
                    return new MilliGanjubusMessage(MessageType.ApplicationDataError,
                        deviceAddress, (int)MilliGanjubusErrorType.WrongDataAmount);
                }

                var message = new MilliGanjubusMessage(messageType, data[MilliGanjubusBase.AddressOffset]);

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
