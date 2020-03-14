using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.MilliGanjubus.Components
{
    public class MilliGanjubusComposer : IComposer<IRegisterMessage, byte[]>
    {
        // todo: how to make protocolInfo fields changeable
        // Interface with get-properties?

        public byte[] Compose(IRegisterMessage message)
        {
            var data = GetDataBytes(message);
            var packet = new byte[MilliGanjubusBase.MinPacketLength + data.Length];
            if (packet.Length > MilliGanjubusBase.MaxPacketLength)
            {
                // todo: create a custom exception.
                throw new ArgumentOutOfRangeException();
            }
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
            // Length of result array is unknown, so list is used.
            var dataList = new List<byte>();

            var registersArray = message.Registers.ToArray();
            switch (message.Type)
            {
                case MessageType.ReadDataRequest:
                    if (IsRange(registersArray))
                    {
                        FillRange(MilliGanjubusBase.FReadRange, false);
                        break;
                    }
                    FillSeries(MilliGanjubusBase.FReadSeries, false);
                    break;

                case MessageType.ReadDataResponse:
                    if (IsRange(registersArray))
                    {
                        FillRange(MilliGanjubusBase.GReply << 4 | MilliGanjubusBase.FReadRange, true);
                        break;
                    }
                    FillSeries(MilliGanjubusBase.GReply << 4 | MilliGanjubusBase.FReadSeries, true);
                    break;

                case MessageType.WriteDataRequest:
                    if (IsRange(registersArray))
                    {
                        FillRange(MilliGanjubusBase.FWriteRange, true);
                        break;
                    }
                    FillSeries(MilliGanjubusBase.FWriteSeries, true);
                    break;

                case MessageType.WriteDataResponse:
                    if (IsRange(registersArray))
                    {
                        FillRange(MilliGanjubusBase.GReply << 4 | MilliGanjubusBase.FWriteRange, false);
                        break;
                    }
                    FillSeries(MilliGanjubusBase.GReply << 4 | MilliGanjubusBase.FWriteSeries, false);
                    break;

                case MessageType.ErrorMessage:
                    dataList.Add(MilliGanjubusBase.GError << 4);
                    dataList.Add((byte)message.ErrorCode);
                    break;

                default:
                    // todo: Create a custom exception
                    throw new ArgumentException("Wrong message type");
            }

            return dataList.ToArray();

            // Local functions for application data filling.
            void FillRange(byte gByte, bool withValues)
            {
                dataList.Add(gByte);
                AddAddressToPacket(dataList, registersArray[0].Address);
                // End range registerGroup address will be added after fill the collection.
                var endRangeIndex = dataList.Count;
                if (!withValues)
                {
                    return;
                }

                int lastAddedRegisterAddress = 0;
                foreach (var register in registersArray)
                {
                    var registerValue = register.GetBytes();
                    if (dataList.Count + registerValue.Length + MilliGanjubusBase.AddressSize > MilliGanjubusBase.MaxDataLength)
                    {
                        break;
                    }

                    foreach (var b in registerValue)
                    {
                        dataList.Add(b);
                    }

                    message.RemoveRegister(register);
                    lastAddedRegisterAddress = register.Address;
                }

                // Add real last registerGroup address.
                var endRangeAddress = new List<byte>(MilliGanjubusBase.AddressSize);
                AddAddressToPacket(endRangeAddress, lastAddedRegisterAddress);
                dataList.InsertRange(endRangeIndex, endRangeAddress);
            }

            void FillSeries(byte gByte, bool withValues)
            {
                dataList.Add(gByte);
                if (withValues)
                {
                    foreach (var register in registersArray)
                    {
                        if (dataList.Count + MilliGanjubusBase.AddressSize > MilliGanjubusBase.MaxDataLength)
                        {
                            break;
                        }
                        AddAddressToPacket(dataList, register.Address);
                        message.RemoveRegister(register);
                    } 
                    return;
                }

                foreach (var register in registersArray)
                {
                    var registerValue = register.GetBytes();
                    if (dataList.Count + registerValue.Length + MilliGanjubusBase.AddressSize > MilliGanjubusBase.MaxDataLength)
                    {
                        break;
                    }

                    AddAddressToPacket(dataList, register.Address);
                    foreach (var b in registerValue)
                    {
                        dataList.Add(b);
                    }

                    message.RemoveRegister(register);
                }
            }

        }

        // In little endian.
        void AddAddressToPacket(List<byte> packet, int address)
        {
            int index = 0;
            while (index < MilliGanjubusBase.AddressSize)
            {
                packet.Add((byte)(address >> (8 * index++)));
            }
        }

        // todo: maybe I can mark message as range at forming stage?
        private bool IsRange(IRegisterGroup[] registers)
        {
            if (registers.Length < MilliGanjubusBase.MinimumRangeRegisterCount)
            {
                return false;
            }

            var index = 0;
            var previousAddress = registers[index++].Address;
            while (index < registers.Length)
            {
                var currentAddress = registers[index++].Address;
                // All addresses must be in order.
                if (currentAddress - previousAddress != 1)
                {
                    return false;
                }
            }

            return true;
        }


    }
}
