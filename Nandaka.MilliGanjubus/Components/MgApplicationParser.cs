using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.MilliGanjubus.Models;

namespace Nandaka.MilliGanjubus.Components
{
    internal class MgApplicationParser : ApplicationParserBase<byte[]>
    {
        private readonly MgInfo _info;

        public MgApplicationParser() 
            : base(new MgDataLinkParser())
        {
            _info = MgInfo.Instance;
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
                case MgInfo.GRequest:
                    messageType = MessageType.Request;
                    break;

                case MgInfo.GReply:
                    messageType = MessageType.Response;
                    withValues = true;
                    break;

                case MgInfo.GError:
                    return ParseErrorMessage(data, deviceAddress);

                default:
                    throw new InvalidMetaDataReceivedException("Wrong gByte");
            }

            bool isRange = false;
            OperationType operationType;

            int fCode = gByte & 0xF;
            switch (fCode)
            {
                case MgInfo.FReadSingle:
                case MgInfo.FReadSeries:
                    operationType = OperationType.Read;
                    break;
                case MgInfo.FReadRange:
                    operationType = OperationType.Read;
                    isRange = true;
                    break;

                case MgInfo.FWriteSingle:
                case MgInfo.FWriteSeries:
                    operationType = OperationType.Write;
                    // By default assume that this is read operation. Otherwise invert variable.
                    withValues = !withValues;
                    break;
                case MgInfo.FWriteRange:
                    operationType = OperationType.Write;
                    withValues = !withValues;
                    isRange = true;
                    break;

                default:
                    throw new InvalidMetaDataReceivedException("Wrong gByte");
            }

            IReadOnlyList<IRegister<byte>> registers = isRange ? ParseAsRange(data, withValues)
                : ParseAsSeries(data, withValues);

            return new MgRegisterMessage(deviceAddress, messageType, operationType, registers);
        }

        private ErrorMessage ParseErrorMessage(IReadOnlyList<byte> data, byte deviceAddress)
        {
            var mgErrorType = (MgErrorType) data[_info.DataOffset + 1];
            return MgErrorMessage.Create(deviceAddress, MessageType.Response, mgErrorType);
        }

        private IReadOnlyList<IRegister<byte>> ParseAsSeries(IReadOnlyList<byte> data, bool withValues)
        {
            // Look through all data bytes except CRC.
            byte packetSize = data[_info.SizeOffset];
            int byteIndex = _info.MinPacketLength;

            var registers = new List<Register<byte>>();

            if (withValues)
            {
                while (byteIndex < packetSize - 1)
                    registers.Add(RawRegistersFactory.Create(data[byteIndex++], data[byteIndex++]));
            }
            else
            {
                while (byteIndex < packetSize - 1)
                    registers.Add(RawRegistersFactory.Create<byte>(data[byteIndex++]));
            }

            // Case of crc is register value.
            if (byteIndex != packetSize - 1)
                throw new Exception("Wrong data amount");

            return registers;
        }

        private IReadOnlyList<IRegister<byte>> ParseAsRange(IReadOnlyList<byte> data, bool withValues)
        {
            // Ignore gByte.
            int currentByteIndex = _info.DataOffset + 1;

            // Bytes after gByte are a range of addresses.
            byte startAddress = data[currentByteIndex++];
            byte endAddress = data[currentByteIndex++];
            int registersCount = endAddress - startAddress + 1;

            // Check addresses validity.
            if (startAddress > endAddress)
                throw new InvalidRegistersReceivedException("Wrong register Address");

            // Check registers count is valid number (less than registerGroup values bytes count).
            if (withValues && registersCount > data[_info.SizeOffset] - _info.MinPacketLength - 2)
                throw new TooMuchDataRequestedException("Wrong data amount");


            return Enumerable.Range(startAddress, registersCount)
                             .Select(address => withValues
                                         ? RawRegistersFactory.Create(address, data[currentByteIndex++])
                                         : RawRegistersFactory.Create<byte>(address))
                             .ToArray();
        }
    }
}
