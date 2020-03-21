using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;

namespace Nandaka.Core.Helpers
{
    public static class RegisterConverter
    {
        public static byte[] ComposeDataAsRange(IRegisterMessage message, IProtocolInfo info, IEnumerable<byte> dataHeader,
            bool withValues)
        {
            IRegisterGroup[] registerGroups = message.RegisterGroups.ToArray();

            var result = new List<byte>(dataHeader);

            result.AddRange(GetRegisterAddress(registerGroups.First().Address, info));

            if (!withValues)
            {
                result.AddRange(GetRegisterAddress(registerGroups.Last().Address, info));
                return result.ToArray();
            }

            int lastRegisterAddressResultIndex = result.Count;

            var lastAddedRegisterAddress = 0;
            foreach (IRegisterGroup registerGroup in registerGroups)
            {
                byte[] groupData = registerGroup.ToBytes();

                if (result.Count + groupData.Length + info.AddressSize > info.MaxDataLength)
                    break;

                result.AddRange(groupData);

                message.RegisterGroups.Remove(registerGroup);
                lastAddedRegisterAddress = registerGroup.Address;
            }

            byte[] endRangeAddress = GetRegisterAddress(lastAddedRegisterAddress, info);
            result.InsertRange(lastRegisterAddressResultIndex, endRangeAddress);

            return result.ToArray();
        }

        public static byte[] ComposeDataAsSeries(IRegisterMessage message, IProtocolInfo info, IEnumerable<byte> dataHeader,
            bool withValues)
        {
            ICollection<IRegisterGroup> registerGroups = message.RegisterGroups;

            var result = new List<byte>(dataHeader);

            foreach (IRegisterGroup registerGroup in registerGroups)
            {
                int groupSize = GetGroupSize(registerGroup, info);

                if (result.Count + groupSize > info.MaxDataLength)
                    break;

                foreach (IRegister register in registerGroup.GetRawRegisters())
                {
                    result.AddRange(GetRegisterAddress(register.Address, info));

                    if (withValues)
                        result.AddRange(register.ToBytes());
                }

                message.RegisterGroups.Remove(registerGroup);
            }

            return result.ToArray();
        }

        /// <summary>
        /// Check for addresses ordering.
        /// </summary>
        public static bool IsRange(ICollection<IRegisterGroup> registerGroups, IProtocolInfo info)
        {
            var registerInRangeCount = 0;
            var dataSize = 0;

            int nextAddress = registerGroups.First().Address;
            foreach (IRegisterGroup registerGroup in registerGroups)
            {
                if (registerGroup.Address != nextAddress)
                    return false;

                dataSize += GetGroupSize(registerGroup, info);
                if (dataSize > info.MaxDataLength)
                    break;

                nextAddress += registerGroup.Count;

                registerInRangeCount++;
            }

            if (registerInRangeCount < info.MinimumRangeRegisterCount)
                return false;

            return true;
        }

        private static int GetGroupSize(IRegisterGroup registerGroup, IProtocolInfo info)
        {
            return registerGroup.Count * (info.AddressSize + registerGroup.DataSize);
        }

        private static byte[] GetRegisterAddress(int address, IProtocolInfo info)
        {
            return LittleEndianConverter.GetBytes(address, info.AddressSize);
        }
    }
}
