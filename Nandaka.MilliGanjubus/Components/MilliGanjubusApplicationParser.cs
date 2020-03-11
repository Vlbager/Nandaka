using System.Linq;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;
using Nandaka.MilliGanjubus.Models;

namespace Nandaka.MilliGanjubus.Components
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
                    break;
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
                    break;
                case MilliGanjubusBase.GError:
                    var errorCode = data[MilliGanjubusBase.DataOffset + 1];
                    return new MilliGanjubusMessage(MessageType.ErrorMessage, deviceAddress, errorCode);

            }
            // If message not returned yet, then gByte is wrong.
            // ReSharper disable once RedundantArgumentDefaultValue
            return new MilliGanjubusMessage(MessageType.ApplicationDataError,
                deviceAddress, (int)MilliGanjubusErrorType.WrongGByte);

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

                // todo: add registerGroup class and rework this.
                if (withValues)
                {
                    while (byteIndex < packetSize - 1)
                    {
                        message.AddRegister(new RawRegister(data[byteIndex++], data[byteIndex++]));
                    }
                }
                else
                {
                    while (byteIndex < packetSize - 1)
                    {
                        message.AddRegister(new RawRegister(data[byteIndex++]));
                    }
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

                // Check registers count is valid number (less than registerGroup values bytes count).
                if (withValues && registersCount > data[MilliGanjubusBase.SizeOffset] - MilliGanjubusBase.MinPacketLength - 2)
                {
                    return new MilliGanjubusMessage(MessageType.ApplicationDataError,
                        deviceAddress, (int)MilliGanjubusErrorType.WrongDataAmount);
                }

                var message = new MilliGanjubusMessage(messageType, data[MilliGanjubusBase.AddressOffset]);

                foreach (var address in Enumerable.Range(startAddress, registersCount))
                {
                    // todo: add registerGroup class and rework this.
                    var register = withValues ?
                        new RawRegister(address, data[currentByteIndex++]) :
                        new RawRegister(address);

                    message.AddRegister(register);
                }
                return message;
            }
        }
    }
}
