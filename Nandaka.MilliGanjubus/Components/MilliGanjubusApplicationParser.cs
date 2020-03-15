using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;
using Nandaka.MilliGanjubus.Models;

namespace Nandaka.MilliGanjubus.Components
{
    public class MilliGanjubusApplicationParser : ApplicationParserBase<byte[]>
    {
        public MilliGanjubusApplicationParser() : base(new MilliGanjubusDataLinkParser())
        {
        }

        protected override IFrameworkMessage ApplicationParse(byte[] data)
        {
            byte deviceAddress = data[MilliGanjubusBase.AddressOffset];

            // Find out the type of message from gByte.
            var gByte = data[MilliGanjubusBase.DataOffset];

            MessageType messageType;
            bool withValues = false;

            // Left nibble is ack code. Right - F code.
            switch (gByte >> 4)
            {
                case MilliGanjubusBase.GRequest:
                    messageType = MessageType.Request;
                    break;

                case MilliGanjubusBase.GReply:
                    messageType = MessageType.Response;
                    withValues = true;
                    break;

                case MilliGanjubusBase.GError:
                    byte errorCode = data[MilliGanjubusBase.DataOffset + 1];
                    return new MilliGanjubusErrorMessage(deviceAddress, MessageType.Response, (MilliGanjubusErrorType)errorCode);

                default:
                    // todo: create a custom exception 
                    throw new Exception("Wrong gByte");
            }

            bool isRange = false;
            OperationType operationType;

            switch (gByte & 0xF)
            {
                case MilliGanjubusBase.FReadSingle:
                case MilliGanjubusBase.FReadSeries:
                    operationType = OperationType.Read;
                    break;
                case MilliGanjubusBase.FReadRange:
                    operationType = OperationType.Read;
                    isRange = true;
                    break;

                case MilliGanjubusBase.FWriteSingle:
                case MilliGanjubusBase.FWriteSeries:
                    operationType = OperationType.Write;
                    // By default assume that this is read operation. Otherwise invert variable.
                    withValues = !withValues;
                    break;
                case MilliGanjubusBase.FWriteRange:
                    operationType = OperationType.Write;
                    withValues = !withValues;
                    isRange = true;
                    break;

                default:
                    // todo: create a custom exception
                    throw new Exception("Wrong gByte");
            }

            IReadOnlyCollection<IRegisterGroup> registers = isRange ? ParseAsRange(data, withValues)
                : ParseAsSeries(data, withValues);

            return new CommonMessage(deviceAddress, messageType, operationType, registers);
        }

        private static IReadOnlyCollection<IRegisterGroup> ParseAsSeries(IReadOnlyList<byte> data, bool withValues)
        {
            // Look through all data bytes except CRC.
            byte packetSize = data[MilliGanjubusBase.SizeOffset];
            int byteIndex = MilliGanjubusBase.MinPacketLength;

            var registers = new List<SingleRegisterGroup<byte>>();

            if (withValues)
            {
                while (byteIndex < packetSize - 1)
                    registers.Add(new SingleRegisterGroup<byte>(Register<byte>.CreateByte(data[byteIndex++], data[byteIndex++])));
            }
            else
            {
                while (byteIndex < packetSize - 1)
                    registers.Add(new SingleRegisterGroup<byte>(Register<byte>.CreateByte(data[byteIndex++])));
            }

            // Case of crc is register value.
            if (byteIndex != packetSize - 1)
                throw new Exception("Wrong data amount");

            return registers;
        }

        private static IReadOnlyCollection<IRegisterGroup> ParseAsRange(IReadOnlyList<byte> data, bool withValues)
        {
            // Ignore gByte.
            int currentByteIndex = MilliGanjubusBase.DataOffset + 1;

            // Bytes after gByte are a range of addresses.
            byte startAddress = data[currentByteIndex++];
            byte endAddress = data[currentByteIndex++];
            int registersCount = endAddress - startAddress + 1;

            // Check addresses validity.
            if (startAddress > endAddress)
                //todo: create a custom exception
                throw new Exception("Wrong register Address");

            // Check registers count is valid number (less than registerGroup values bytes count).
            if (withValues && registersCount > data[MilliGanjubusBase.SizeOffset] - MilliGanjubusBase.MinPacketLength - 2)
                //todo: create a custom exception
                throw new Exception("Wrong data amount");

            var registers = new List<SingleRegisterGroup<byte>>(registersCount);

            foreach (int address in Enumerable.Range(startAddress, registersCount))
            {
                SingleRegisterGroup<byte> register = withValues
                    ? new SingleRegisterGroup<byte>(Register<byte>.CreateByte(address, data[currentByteIndex++]))
                    : new SingleRegisterGroup<byte>(Register<byte>.CreateByte(address));

                registers.Add(register);
            }

            return registers;
        }
    }
}
