using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;
using Nandaka.MilliGanjubus.Models;
using Nandaka.MilliGanjubus.Utils;

namespace Nandaka.MilliGanjubus.Components
{
    internal class MilliGanjubusComposer : IComposer<IMessage, byte[]>
    {
        private readonly MilliGanjubusInfo _info;

        public MilliGanjubusComposer(MilliGanjubusInfo ganjubusInfo)
        {
            _info = ganjubusInfo;
        }

        public byte[] Compose(IMessage message, out IReadOnlyCollection<IRegisterGroup> composedGroups)
        {
            switch (message)
            {
                case IRegisterMessage registerMessage:
                    return Compose(registerMessage, out composedGroups);

                case ErrorMessage errorMessage:
                    composedGroups = Array.Empty<IRegisterGroup>();
                    return Compose(errorMessage);

                default:
                    throw new NandakaBaseException("Unexpected type of message");
            }
        }

        private byte[] Compose(ErrorMessage message)
        {
            byte[] packet = PreparePacketWithHeader(_info.MinPacketLength + 2, message.SlaveDeviceAddress);

            packet[_info.DataOffset] = MilliGanjubusInfo.GError.ToFirstNibble();
            packet[_info.DataOffset + 1] = GetReturnCode(message);

            packet[packet.Length - 1] =
                CheckSum.Crc8(packet.Take(packet.Length - _info.PacketCheckSumSize).ToArray());

            return packet;
        }

        private byte GetReturnCode(ErrorMessage message)
        {
            MilliGanjubusErrorType? mgErrorType = message.ErrorType.Convert();
            if (mgErrorType.HasValue)
                return (byte) mgErrorType.Value;

            if (message.ErrorType == ErrorType.InternalProtocolError &&
                message.ProtocolSpecifiedErrorMessage is MilliGanjubusErrorMessage mgErrorMessage)
            {
                return (byte) mgErrorMessage.ErrorCode;
            }

            throw new InvalidMessageToComposeException("Specified error message does not contains any compatible error type");
        }

        private byte[] Compose(IRegisterMessage message, out IReadOnlyCollection<IRegisterGroup> composedGroups)
        {
            if (message.RegisterGroups.IsEmpty())
                throw new InvalidMessageToComposeException("Specified message does not contains any registers");
            
            byte[] data = GetDataBytes(message, out composedGroups);

            byte[] packet = PreparePacketWithHeader(_info.MinPacketLength + data.Length, message.SlaveDeviceAddress);

            Array.Copy(data, 0, packet, _info.DataOffset, data.Length);
            packet[packet.Length - 1] =
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

        private byte[] GetDataBytes(IRegisterMessage message, out IReadOnlyCollection<IRegisterGroup> composedGroups)
        {
            byte gByte;
            bool withValues = false;
            switch (message.MessageType)
            {
                case MessageType.Request:
                    gByte = MilliGanjubusInfo.GRequest;
                    break;

                case MessageType.Response:
                    gByte = MilliGanjubusInfo.GReply.ToFirstNibble();
                    withValues = true;
                    break;

                default:
                    throw new NandakaBaseException("Undefined message type");
            }

            bool isRange = RegisterConverter.IsRange(message.RegisterGroups, _info);

            switch (message.OperationType)
            {
                case OperationType.Read:
                    if (isRange)
                        gByte |= MilliGanjubusInfo.FReadRange;
                    else
                        gByte |= MilliGanjubusInfo.FReadSeries;

                    break;

                case OperationType.Write:
                    if (isRange)
                        gByte |= MilliGanjubusInfo.FWriteRange;
                    else
                        gByte |= MilliGanjubusInfo.FWriteSeries;

                    // By default assume that this is read operation. Otherwise invert variable.
                    withValues = !withValues;
                    break;

                default:
                    throw new NandakaBaseException("Undefined operation type");
            }

            byte[] dataHeader = {gByte};

            if (isRange)
                return RegisterConverter.ComposeDataAsRange(message.RegisterGroups, _info, dataHeader, withValues, out composedGroups);

            return RegisterConverter.ComposeDataAsSeries(message.RegisterGroups, _info, dataHeader, withValues, out composedGroups);
        }
    }
}
