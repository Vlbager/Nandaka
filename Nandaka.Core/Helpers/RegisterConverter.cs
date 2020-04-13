using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Helpers
{
    public static class RegisterConverter
    {
        /// <summary>
        /// Range represent sequence of registers ordered by address in row with structure:
        /// first register address, last register address, register 1 value, register 2 value, ..., register n value. 
        /// </summary>
        public static byte[] ComposeDataAsRange(IReadOnlyCollection<IRegisterGroup> registerGroups, IProtocolInfo info, IEnumerable<byte> dataHeader,
            bool withValues, out IReadOnlyCollection<IRegisterGroup> composedGroups)
        {
            var result = new List<byte>(dataHeader);
            var composedGroupList = new List<IRegisterGroup>();

            result.AddRange(GetRegisterAddress(registerGroups.First().Address, info));
            
            int lastRegisterAddressIndex = result.Count;
            // Temp address should be specified after the packet is formed.
            result.AddRange(Enumerable.Repeat<byte>(0, info.AddressSize));

            // Current packet size with last address.
            int currentDataPacketSize = result.Count;
            foreach (IRegisterGroup registerGroup in registerGroups)
            {
                if (currentDataPacketSize + registerGroup.DataSize > info.MaxDataLength)
                    break;

                currentDataPacketSize += registerGroup.DataSize;

                if (withValues)
                    result.AddRange(registerGroup.ToBytes());

                composedGroupList.Add(registerGroup);
            }

            byte[] endRangeAddress = GetRegisterAddress(composedGroupList.Last().GetLastRegisterAddress(), info);
            foreach (int indexOffset in Enumerable.Range(0, info.AddressSize))
                result[lastRegisterAddressIndex + indexOffset] = endRangeAddress[indexOffset];

            composedGroups = composedGroupList;
            return result.ToArray();
        }

        public static byte[] ComposeDataAsSeries(IReadOnlyCollection<IRegisterGroup> registerGroups, IProtocolInfo info, IEnumerable<byte> dataHeader,
            bool withValues, out IReadOnlyCollection<IRegisterGroup> composedGroups)
        {
            var result = new List<byte>(dataHeader);
            var composedGroupList = new List<IRegisterGroup>();

            int currentDataPacketSize = result.Count;
            foreach (IRegisterGroup registerGroup in registerGroups)
            {
                int groupSize = GetGroupSizeAsSeries(registerGroup, info);

                if (currentDataPacketSize + groupSize > info.MaxDataLength)
                    break;

                currentDataPacketSize += groupSize;

                foreach (IRegister register in registerGroup.GetRawRegisters())
                {
                    result.AddRange(GetRegisterAddress(register.Address, info));

                    if (withValues)
                        result.AddRange(register.ToBytes());
                }

                composedGroupList.Add(registerGroup);
            }

            composedGroups = composedGroupList;
            return result.ToArray();
        }

        /// <summary>
        /// Check for addresses ordering.
        /// </summary>
        public static bool IsRange(IReadOnlyCollection<IRegisterGroup> registerGroups, IProtocolInfo info)
        {
            var registerInRangeCount = 0;
            // Already includes first and last registers addresses.
            int dataSize = 2 * info.AddressSize;

            int nextAddress = registerGroups.First().Address;
            foreach (IRegisterGroup registerGroup in registerGroups)
            {
                foreach (IRegister register in registerGroup.GetRawRegisters())
                {
                    if (register.Address != nextAddress)
                        return false;

                    nextAddress++;
                    registerInRangeCount++;
                }
                
                dataSize += registerGroup.DataSize;
                if (dataSize > info.MaxDataLength)
                    break;
            }

            if (registerInRangeCount < info.MinimumRangeRegisterCount)
                return false;

            return true;
        }

        private static int GetGroupSizeAsSeries(IRegisterGroup registerGroup, IProtocolInfo info)
        {
            return info.AddressSize + registerGroup.DataSize;
        }

        private static byte[] GetRegisterAddress(int address, IProtocolInfo info)
        {
            return LittleEndianConverter.GetBytes(address, info.AddressSize);
        }
    }
}