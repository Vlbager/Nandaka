using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Protocol;
using Nandaka.Core.Table;

namespace Nandaka.Core.Helpers
{
    public static class RegisterConverter
    {
        public static byte[] ComposeDataAsRange(IReadOnlyCollection<IRegisterGroup> registerGroups, IProtocolInfo info, IEnumerable<byte> dataHeader,
            bool withValues, out IReadOnlyCollection<IRegisterGroup> composedGroups)
        {
            var result = new List<byte>(dataHeader);

            result.AddRange(GetRegisterAddress(registerGroups.First().Address, info));

            if (!withValues)
            {
                result.AddRange(GetRegisterAddress(registerGroups.Last().Address, info));

                composedGroups = Array.Empty<IRegisterGroup>();
                return result.ToArray();
            }

            var composedGroupList = new List<IRegisterGroup>();

            int lastRegisterAddressResultIndex = result.Count;

            var lastAddedRegisterAddress = 0;
            foreach (IRegisterGroup registerGroup in registerGroups)
            {
                byte[] groupData = registerGroup.ToBytes();

                if (result.Count + groupData.Length + info.AddressSize > info.MaxDataLength)
                    break;

                result.AddRange(groupData);

                composedGroupList.Add(registerGroup);
                lastAddedRegisterAddress = registerGroup.Address;
            }

            byte[] endRangeAddress = GetRegisterAddress(lastAddedRegisterAddress, info);
            result.InsertRange(lastRegisterAddressResultIndex, endRangeAddress);

            composedGroups = composedGroupList;
            return result.ToArray();
        }

        public static byte[] ComposeDataAsSeries(IReadOnlyCollection<IRegisterGroup> registerGroups, IProtocolInfo info, IEnumerable<byte> dataHeader,
            bool withValues, out IReadOnlyCollection<IRegisterGroup> composedGroups)
        {
            var result = new List<byte>(dataHeader);
            var composedGroupList = new List<IRegisterGroup>();

            foreach (IRegisterGroup registerGroup in registerGroups)
            {
                int groupSize = GetGroupSizeAsSeries(registerGroup, info);

                if (result.Count + groupSize > info.MaxDataLength)
                    break;

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