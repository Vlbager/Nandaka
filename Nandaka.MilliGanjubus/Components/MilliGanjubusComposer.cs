using System;
using System.Linq;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;

namespace Nandaka.MilliGanjubus.Components
{
    internal class MilliGanjubusComposer : IComposer<IFrameworkMessage, byte[]>
    {
        private readonly MilliGanjubusInfo _info;

        public MilliGanjubusComposer(MilliGanjubusInfo ganjubusInfo)
        {
            _info = ganjubusInfo;
        }

        public byte[] Compose(IFrameworkMessage message)
        {
            switch (message)
            {
                case IRegisterMessage registerMessage:
                    return Compose(registerMessage);

                case IErrorMessage errorMessage:
                    return Compose(errorMessage);

                default:
                    // todo: create a custom exception
                    throw new Exception("Unexpected type of message");
            }
        }

        private byte[] Compose(IErrorMessage message)
        {
            throw new NotImplementedException();
        }

        private byte[] Compose(IRegisterMessage message)
        {
            byte[] data = GetDataBytes(message);

            var packet = new byte[_info.MinPacketLength + data.Length];

            packet[_info.StartByteOffset] = _info.StartByte;
            packet[_info.AddressOffset] = (byte)message.SlaveDeviceAddress;
            packet[_info.SizeOffset] = (byte)packet.Length;
            packet[_info.HeaderCheckSumOffset] =
                CheckSum.Crc8(packet.Take(_info.HeaderCheckSumOffset).ToArray());

            Array.Copy(data, 0, packet, _info.DataOffset, data.Length);
            packet[packet.Length - 1] =
                CheckSum.Crc8(packet.Take(packet.Length - _info.PacketCheckSumSize).ToArray());

            return packet;
        }

        private byte[] GetDataBytes(IRegisterMessage message)
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
                    //todo: create a custom exception
                    throw new Exception("Undefined message type");
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
                    // todo: create a custom exception;
                    throw new Exception("Undefined operation type");
            }

            byte[] dataHeader = new[] {gByte};

            if (isRange)
                return RegisterConverter.ComposeDataAsRange(message, _info, dataHeader, withValues);

            return RegisterConverter.ComposeDataAsSeries(message, _info, dataHeader, withValues);
        }




    }
}
