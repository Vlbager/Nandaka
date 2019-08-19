using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Nandaka.MilliGanjubus
{
    public class MilliGanjubusComposer : MilliGanjubusBase, IComposer<byte[]>
    {
        // todo: how to make protocolInfo fields changeable for MilliGanjubus or GeneralGanjubus 
        // or DontKnowGanjubus? (Composer and parser can be the same for all of them)
        // Interface with getter-methods?

        public byte[] Compose(IProtocolMessage message)
        {
            var data = GetDataBytes(message);
            var packet = new byte[MinPacketLength + data.Length];
            if (packet.Length > MaxPacketLength)
            {
                // todo: create a custom exception.
                throw new ArgumentOutOfRangeException();
            }
            packet[StartByteOffset] = StartByte;
            packet[AddressOffset] = (byte)message.DeviceAddress;
            packet[SizeOffset] = (byte)packet.Length;
            packet[HeaderCheckSumOffset] =
                CheckSum.CRC8(packet.Take(HeaderCheckSumOffset).ToArray());
            Array.Copy(data, 0, packet, DataOffset, data.Length);
            packet[packet.Length - 1] =
                CheckSum.CRC8(packet.Take(packet.Length - PacketCheckSumSize).ToArray());
            return packet;
        }

        private byte[] GetDataBytes(IProtocolMessage message)
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
