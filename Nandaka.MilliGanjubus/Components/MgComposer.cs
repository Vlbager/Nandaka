using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.Core.Util;
using Nandaka.MilliGanjubus.Models;
using Nandaka.MilliGanjubus.Utils;
using Nandaka.Model.Registers;

namespace Nandaka.MilliGanjubus.Components
{
    internal class MgComposer : IComposer<IMessage, byte[]>
    {
        private static readonly int[] MgErrorCodes = Enum.GetValues<MgErrorType>()
                                                         .Cast<int>()
                                                         .ToArray();
        
        private readonly MgInfo _info;

        public MgComposer()
        {
            _info = MgInfo.Instance;
        }

        public byte[] Compose(IMessage message)
        {
            switch (message)
            {
                case IRegisterMessage registerMessage:
                    return Compose(registerMessage);

                case ErrorMessage errorMessage:
                    return Compose(errorMessage);

                default:
                    throw new NandakaBaseException("Unexpected type of message");
            }
        }

        private byte[] Compose(ErrorMessage message)
        {
            byte[] packet = PreparePacketWithHeader(_info.MinPacketLength + 2, message.SlaveDeviceAddress);

            packet[_info.DataOffset] = MgInfo.GError.ToFirstNibble();
            packet[_info.DataOffset + 1] = GetReturnCode(message);

            packet[^1] =
                CheckSum.Crc8(packet.Take(packet.Length - _info.PacketCheckSumSize).ToArray());

            return packet;
        }

        private byte GetReturnCode(ErrorMessage message)
        {
            MgErrorType? mgErrorType = message.ErrorType.Convert();
            if (mgErrorType.HasValue)
                return (byte) mgErrorType.Value;

            if (message.ErrorType == ErrorType.InternalProtocolError && 
                message.ProtocolSpecifiedErrorMessage != null &&
                MgErrorCodes.Contains(message.ProtocolSpecifiedErrorMessage.ErrorCode))
            {
                return (byte) message.ProtocolSpecifiedErrorMessage.ErrorCode;
            }

            throw new NandakaBaseException("Specified error message does not contains any compatible error type");
        }

        private byte[] Compose(IRegisterMessage message)
        {
            if (message.Registers.IsEmpty())
                throw new NandakaBaseException("Specified message does not contains any registers");
            
            byte[] data = GetDataBytes(message);

            byte[] packet = PreparePacketWithHeader(_info.MinPacketLength + data.Length, message.SlaveDeviceAddress);

            Array.Copy(data, 0, packet, _info.DataOffset, data.Length);
            packet[^1] =
                CheckSum.Crc8(packet.Take(packet.Length - _info.PacketCheckSumSize).ToArray());

            return packet;
        }

        private byte[] PreparePacketWithHeader(int packetLength, int deviceAddress)
        {
            var packetBlank = new byte[packetLength];

            packetBlank[_info.StartByteOffset] = _info.StartByte;
            packetBlank[_info.AddressOffset] = (byte)deviceAddress;
            packetBlank[_info.SizeOffset] = (byte)packetBlank.Length;
            packetBlank[_info.HeaderCheckSumOffset] =
                CheckSum.Crc8(packetBlank.Take(_info.HeaderCheckSumOffset).ToArray());

            return packetBlank;
        }

        private byte[] GetDataBytes(IRegisterMessage message)
        {
            if (message.Registers is not IReadOnlyList<IRegister<byte>>)
                throw new NandakaBaseException("MG composer supports only byte registers");
            
            byte gByte;
            bool withValues = false;
            switch (message.MessageType)
            {
                case MessageType.Request:
                    gByte = MgInfo.GRequest;
                    break;

                case MessageType.Response:
                    gByte = MgInfo.GReply.ToFirstNibble();
                    withValues = true;
                    break;

                default:
                    throw new NandakaBaseException("Undefined message type");
            }

            bool isRange = RegisterConverter.IsRange(message.Registers, _info);

            switch (message.OperationType)
            {
                case OperationType.Read:
                    if (isRange)
                        gByte |= MgInfo.FReadRange;
                    else
                        gByte |= MgInfo.FReadSeries;

                    break;

                case OperationType.Write:
                    if (isRange)
                        gByte |= MgInfo.FWriteRange;
                    else
                        gByte |= MgInfo.FWriteSeries;

                    // By default assume that this is read operation. Otherwise invert variable.
                    withValues = !withValues;
                    break;

                default:
                    throw new NandakaBaseException("Undefined operation type");
            }

            byte[] dataHeader = {gByte};

            return GetDataBytes(message, isRange, dataHeader, withValues);
        }

        private byte[] GetDataBytes(IRegisterMessage message, bool isRange, byte[] dataHeader, bool withValues)
        {
            IReadOnlyList<int> composedRegisterAddresses;
            byte[] composedBytes = isRange 
                ? RegisterConverter.ComposeDataAsRange(message.Registers, _info, dataHeader, withValues, out composedRegisterAddresses) 
                : RegisterConverter.ComposeDataAsSeries(message.Registers, _info, dataHeader, withValues, out composedRegisterAddresses);

            if (composedRegisterAddresses.Count != message.Registers.Count)
                throw new TooMuchDataRequestedException("Too much registers required to compose");

            return composedBytes;
        }
    }
}
