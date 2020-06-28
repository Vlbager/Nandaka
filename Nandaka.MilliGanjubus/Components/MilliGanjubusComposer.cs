using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

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

                case IErrorMessage errorMessage:
                    composedGroups = Array.Empty<IRegisterGroup>();
                    return Compose(errorMessage);

                default:
                    throw new NandakaBaseException("Unexpected type of message");
            }
        }

        private byte[] Compose(IErrorMessage message)
        {
            byte[] packet = PreparePacketWithHeader(_info.MinPacketLength + 1, message.SlaveDeviceAddress);

            if (message is ProtocolSpecifiedErrorMessage mgError)
                packet[_info.DataOffset] = (byte) mgError.ErrorCode;
            else
                packet[_info.DataOffset] = ConvertCommonErrorMessageCode(message);
            
            
            packet[packet.Length - 1] =
                CheckSum.Crc8(packet.Take(packet.Length - _info.PacketCheckSumSize).ToArray());

            return packet;
        }

        private byte ConvertCommonErrorMessageCode(IErrorMessage message)
        {
            throw new NotImplementedException();
        }

        private byte[] Compose(IRegisterMessage message, out IReadOnlyCollection<IRegisterGroup> composedGroups)
        {
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
            switch (message.Type)
            {
                case MessageType.Request:
                    gByte = MilliGanjubusInfo.GRequest;
                    break;

                case MessageType.Response:
                    gByte = (byte)(MilliGanjubusInfo.GReply << 4);
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
