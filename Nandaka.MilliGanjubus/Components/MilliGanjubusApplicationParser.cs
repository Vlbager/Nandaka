using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;
using Nandaka.MilliGanjubus.Models;

namespace Nandaka.MilliGanjubus.Components
{
    internal class MilliGanjubusApplicationParser : ApplicationParserBase<byte[]>
    {
        private readonly MilliGanjubusInfo _info;

        public MilliGanjubusApplicationParser(MilliGanjubusInfo ganjubusInfo) 
            : base(new MilliGanjubusDataLinkParser(ganjubusInfo))
        {
            _info = ganjubusInfo;
        }

        protected override MessageReceivedEventArgs ApplicationParse(byte[] data)
        {
            try
            {
                IMessage message = ParseMessage(data);
                return new MessageReceivedEventArgs(message);
            }
            catch (Exception exception)
            {
                return new MessageReceivedEventArgs(exception);
            }
        }

        private IMessage ParseMessage(byte[] data)
        {
            byte deviceAddress = data[_info.AddressOffset];

            // Find out the type of message from gByte.
            var gByte = data[_info.DataOffset];

            MessageType messageType;
            bool withValues = false;

            int ackCode = gByte >> 4;
            switch (ackCode)
            {
                case MilliGanjubusInfo.GRequest:
                    messageType = MessageType.Request;
                    break;

                case MilliGanjubusInfo.GReply:
                    messageType = MessageType.Response;
                    withValues = true;
                    break;

                case MilliGanjubusInfo.GError:
                    byte errorCode = data[_info.DataOffset + 1];
                    return new MilliGanjubusErrorMessage(deviceAddress, MessageType.Response, (MilliGanjubusErrorType)errorCode);

                default:
                    // todo: create a custom exception 
                    throw new Exception("Wrong gByte");
            }

            bool isRange = false;
            OperationType operationType;

            int fCode = gByte & 0xF;
            switch (fCode)
            {
                case MilliGanjubusInfo.FReadSingle:
                case MilliGanjubusInfo.FReadSeries:
                    operationType = OperationType.Read;
                    break;
                case MilliGanjubusInfo.FReadRange:
                    operationType = OperationType.Read;
                    isRange = true;
                    break;

                case MilliGanjubusInfo.FWriteSingle:
                case MilliGanjubusInfo.FWriteSeries:
                    operationType = OperationType.Write;
                    // By default assume that this is read operation. Otherwise invert variable.
                    withValues = !withValues;
                    break;
                case MilliGanjubusInfo.FWriteRange:
                    operationType = OperationType.Write;
                    withValues = !withValues;
                    isRange = true;
                    break;

                default:
                    // todo: create a custom exception
                    throw new Exception("Wrong gByte");
            }

            IReadOnlyList<IRegister> registers = isRange ? ParseAsRange(data, _info, withValues)
                : ParseAsSeries(data, _info, withValues);

            return new ReceivedRegisterMessage(deviceAddress, messageType, operationType, registers);
        }

        private static IReadOnlyList<IRegister> ParseAsSeries(IReadOnlyList<byte> data, IProtocolInfo info, bool withValues)
        {
            // Look through all data bytes except CRC.
            byte packetSize = data[info.SizeOffset];
            int byteIndex = info.MinPacketLength;

            var registers = new List<Register<byte>>();

            if (withValues)
            {
                while (byteIndex < packetSize - 1)
                    registers.Add(Register<byte>.CreateByte(data[byteIndex++], RegisterType.Raw, data[byteIndex++]));
            }
            else
            {
                while (byteIndex < packetSize - 1)
                    registers.Add(Register<byte>.CreateByte(data[byteIndex++], RegisterType.Raw));
            }

            // Case of crc is register value.
            if (byteIndex != packetSize - 1)
                throw new Exception("Wrong data amount");

            return registers;
        }

        private static IReadOnlyList<IRegister> ParseAsRange(IReadOnlyList<byte> data, IProtocolInfo info, bool withValues)
        {
            // Ignore gByte.
            int currentByteIndex = info.DataOffset + 1;

            // Bytes after gByte are a range of addresses.
            byte startAddress = data[currentByteIndex++];
            byte endAddress = data[currentByteIndex++];
            int registersCount = endAddress - startAddress + 1;

            // Check addresses validity.
            if (startAddress > endAddress)
                //todo: create a custom exception
                throw new Exception("Wrong register Address");

            // Check registers count is valid number (less than registerGroup values bytes count).
            if (withValues && registersCount > data[info.SizeOffset] - info.MinPacketLength - 2)
                //todo: create a custom exception
                throw new Exception("Wrong data amount");

            var registers = new List<Register<byte>>(registersCount);

            foreach (int address in Enumerable.Range(startAddress, registersCount))
            {
                Register<byte> register = withValues
                    ? Register<byte>.CreateByte(address, RegisterType.Raw, data[currentByteIndex++])
                    : Register<byte>.CreateByte(address, RegisterType.Raw);

                registers.Add(register);
            }

            return registers;
        }
    }
}
