using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.MilliGanjubus.Components
{
    public class MilliGanjubusComposer : IComposer<IFrameworkMessage, byte[]>
    {
        // todo: how to make protocolInfo fields changeable
        // Interface with get-properties?

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
            var data = GetDataBytes(message);

            var packet = new byte[MilliGanjubusBase.MinPacketLength + data.Length];

            packet[MilliGanjubusBase.StartByteOffset] = MilliGanjubusBase.StartByte;
            packet[MilliGanjubusBase.AddressOffset] = (byte)message.SlaveDeviceAddress;
            packet[MilliGanjubusBase.SizeOffset] = (byte)packet.Length;
            packet[MilliGanjubusBase.HeaderCheckSumOffset] =
                CheckSum.Crc8(packet.Take(MilliGanjubusBase.HeaderCheckSumOffset).ToArray());

            Array.Copy(data, 0, packet, MilliGanjubusBase.DataOffset, data.Length);
            packet[packet.Length - 1] =
                CheckSum.Crc8(packet.Take(packet.Length - MilliGanjubusBase.PacketCheckSumSize).ToArray());

            return packet;
        }

        private byte[] GetDataBytes(IRegisterMessage message)
        {
            byte gByte;
            bool withValues = false;
            switch (message.Type)
            {
                case MessageType.Request:
                    gByte = MilliGanjubusBase.GRequest;
                    break;

                case MessageType.Response:
                    gByte = MilliGanjubusBase.GReply << 4;
                    withValues = true;
                    break;

                default:
                    //todo: create a custom exception
                    throw new Exception("Undefined message type");
            }

            switch (message.OperationType)
            {
                case OperationType.Read:
                    break;

                case OperationType.Write:
                    // By default assume that this is read operation. Otherwise invert variable.
                    withValues = !withValues;
                    break;

                default:
                    // todo: create a custom exception;
                    throw new Exception("Undefined operation type");
            }

            
            if (IsRange(message.RegisterGroups as IList<IRegisterGroup>))
                return ComposeDataAsRange(message, (byte)(gByte | MilliGanjubusBase.FReadRange), withValues);
            
            return ComposeDataAsSeries(message, (byte)(gByte | MilliGanjubusBase.FReadSeries), withValues);
        }

        private byte[] ComposeDataAsRange(IRegisterMessage message, byte gByte, bool withValues)
        {
            IRegisterGroup[] registerGroups = message.RegisterGroups.ToArray();

            var result = new List<byte> {gByte};

            result.AddRange(GetRegisterAddress(registerGroups.First().Address));

            if (!withValues)
            {
                result.AddRange(GetRegisterAddress(registerGroups.Last().Address));
                return result.ToArray();
            }

            int lastRegisterAddressResultIndex = result.Count;

            var lastAddedRegisterAddress = 0;
            foreach (IRegisterGroup registerGroup in registerGroups)
            {
                byte[] groupData = registerGroup.ToBytes();

                if (result.Count + groupData.Length + MilliGanjubusBase.AddressSize > MilliGanjubusBase.MaxDataLength)
                    break;

                result.AddRange(groupData);

                message.RegisterGroups.Remove(registerGroup);
                lastAddedRegisterAddress = registerGroup.Address;
            }

            byte[] endRangeAddress = GetRegisterAddress(lastAddedRegisterAddress);
            result.InsertRange(lastRegisterAddressResultIndex, endRangeAddress);

            return result.ToArray();
        }

        private byte[] ComposeDataAsSeries(IRegisterMessage message, byte gByte, bool withValues)
        {
            ICollection<IRegisterGroup> registerGroups = message.RegisterGroups;

            var result = new List<byte> {gByte};

            foreach (IRegisterGroup registerGroup in registerGroups)
            {
                int groupSize = GetGroupSize(registerGroup);

                if (result.Count + groupSize > MilliGanjubusBase.MaxDataLength)
                    break;

                foreach (IRegister register in registerGroup.GetRawRegisters())
                {
                    result.AddRange(GetRegisterAddress(register.Address));

                    if (withValues)
                        result.AddRange(register.ToBytes());
                }

                message.RegisterGroups.Remove(registerGroup);
            }

            return result.ToArray();
        }

        private int GetGroupSize(IRegisterGroup registerGroup)
        {
            return registerGroup.Count * (MilliGanjubusBase.AddressSize + registerGroup.DataSize);
        }

        private byte[] GetRegisterAddress(int address)
        {
            return LittleEndianConverter.GetBytes(address, MilliGanjubusBase.AddressSize);
        }

        /// <summary>
        /// Check for addresses ordering.
        /// </summary>
        private bool IsRange(IList<IRegisterGroup> registerGroups)
        {
            var registerInRangeCount = 0;
            var dataSize = 0;

            int nextAddress = registerGroups.First().Address;
            foreach (IRegisterGroup registerGroup in registerGroups)
            {
                if (registerGroup.Address != nextAddress)
                    return false;

                dataSize += GetGroupSize(registerGroup);
                if (dataSize > MilliGanjubusBase.MaxDataLength)
                    break;

                nextAddress += registerGroup.Count;

                registerInRangeCount++;
            }

            if (registerInRangeCount < MilliGanjubusBase.MinimumRangeRegisterCount)
                return false;

            return true;
        }


    }
}
