using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nandaka.MilliGanjubus
{
    public class MilliGanjubusComposer : IComposer<IMessage, byte[]>
    {
        // todo: how to make protocolInfo fields changeable
        // Interface with getter-methods?

        public byte[] Compose(IMessage message)
        {
            var data = GetDataBytes(message);
            var packet = new byte[MilliGanjubusBase.MinPacketLength + data.Length];
            if (packet.Length > MilliGanjubusBase.MaxPacketLength)
            {
                // todo: create a custom exception.
                throw new ArgumentOutOfRangeException();
            }
            packet[MilliGanjubusBase.StartByteOffset] = MilliGanjubusBase.StartByte;
            packet[MilliGanjubusBase.AddressOffset] = (byte)message.DeviceAddress;
            packet[MilliGanjubusBase.SizeOffset] = (byte)packet.Length;
            packet[MilliGanjubusBase.HeaderCheckSumOffset] =
                CheckSum.Crc8(packet.Take(MilliGanjubusBase.HeaderCheckSumOffset).ToArray());
            Array.Copy(data, 0, packet, MilliGanjubusBase.DataOffset, data.Length);
            packet[packet.Length - 1] =
                CheckSum.Crc8(packet.Take(packet.Length - MilliGanjubusBase.PacketCheckSumSize).ToArray());
            return packet;
        }

        private byte[] GetDataBytes(IMessage message)
        {
            // Length of result array is unknown, so list is used.
            var dataList = new List<byte[]>();

            // todo: Add GByte!!!

            foreach (var register in message.Registers)
            {
                dataList.Add(register.GetBytes());
            }

            // Calculate resultArray size.
            var resultArrayLength = 0;
            dataList.ForEach(byteArray => resultArrayLength += byteArray.Length);

            // Form result array from dataList.
            var resultArray = new byte[resultArrayLength];
            var currentIndex = 0;
            foreach (var byteArray in dataList)
            {
                Array.Copy(byteArray, 0, resultArray, currentIndex, byteArray.Length);
                currentIndex += byteArray.Length;
            }

            return resultArray;
        }
    }
}
