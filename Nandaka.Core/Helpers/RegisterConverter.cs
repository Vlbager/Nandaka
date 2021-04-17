using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Helpers
{
    public static class RegisterConverter
    {
        /// <summary>
        /// Range represent sequence of registers ordered by address in row with structure:
        /// first register address, last register address, register 1 value, register 2 value, ..., register n value. 
        /// </summary>
        public static byte[] ComposeDataAsRange(IReadOnlyCollection<IRegister> registers, IProtocolInfo info, IEnumerable<byte> dataHeader,
            bool withValues, out IReadOnlyList<int> composedRegisterAddresses)
        {
            var result = new List<byte>(dataHeader);
            var composedRegisterAddressList = new List<int>();

            result.AddRange(GetRegisterAddress(registers.First().Address, info));
            
            int lastRegisterAddressIndex = result.Count;
            // Temp address should be specified after the packet is formed.
            result.AddRange(Enumerable.Repeat<byte>(0, info.AddressSize));

            // Current packet size with last address.
            int currentDataPacketSize = 2 * info.AddressSize;
            foreach (IRegister register in registers)
            {
                if (currentDataPacketSize + register.DataSize > info.MaxDataLength)
                    break;

                currentDataPacketSize += register.DataSize;

                if (withValues)
                    result.AddRange(register.ToBytes());

                composedRegisterAddressList.Add(register.Address);
            }

            byte[] endRangeAddress = GetRegisterAddress(composedRegisterAddressList[^1], info);
            foreach (int indexOffset in Enumerable.Range(0, info.AddressSize))
                result[lastRegisterAddressIndex + indexOffset] = endRangeAddress[indexOffset];

            composedRegisterAddresses = composedRegisterAddressList;
            return result.ToArray();
        }

        public static byte[] ComposeDataAsSeries(IReadOnlyCollection<IRegister> registers, IProtocolInfo info, IEnumerable<byte> dataHeader,
            bool withValues, out IReadOnlyList<int> composedRegisterAddresses)
        {
            var result = new List<byte>(dataHeader);
            var composedRegisterAddressList = new List<int>();

            int currentDataPacketSize = 0;
            foreach (IRegister register in registers)
            {
                int registerSize = GetRegisterSizeAsSeries(register, info);

                if (currentDataPacketSize + registerSize > info.MaxDataLength)
                    break;

                currentDataPacketSize += registerSize;
                
                result.AddRange(GetRegisterAddress(register.Address, info));
                
                if (withValues)
                    result.AddRange(register.ToBytes());
                
                composedRegisterAddressList.Add(register.Address);
            }

            composedRegisterAddresses = composedRegisterAddressList;
            return result.ToArray();
        }

        /// <summary>
        /// Check for addresses ordering.
        /// </summary>
        public static bool IsRange(IReadOnlyCollection<IRegister> registers, IProtocolInfo info)
        {
            var registerInRangeCount = 0;
            // Already includes first and last registers addresses.
            int dataSize = 2 * info.AddressSize;

            int nextAddress = registers.First().Address;
            foreach (IRegister register in registers)
            {
                if (dataSize + register.DataSize > info.MaxDataLength)
                    break;

                if (register.Address != nextAddress)
                    return false;

                nextAddress++;
                registerInRangeCount++;

                dataSize += register.DataSize;
            }

            if (registerInRangeCount < info.MinimumRangeRegisterCount)
                return false;

            return true;
        }

        private static int GetRegisterSizeAsSeries(IRegister register, IProtocolInfo info)
        {
            return info.AddressSize + register.DataSize;
        }

        //todo: NAN-34
        private static byte[] GetRegisterAddress(int address, IProtocolInfo info)
        {
            return LittleEndianConverter.GetBytes(address, info.AddressSize);
        }
    }
}