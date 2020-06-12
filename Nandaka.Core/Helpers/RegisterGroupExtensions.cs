using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Table;
using InvalidOperationException = System.InvalidOperationException;

namespace Nandaka.Core.Helpers
{
    internal static class RegisterGroupExtensions
    {
        public static string GetAllAddressesAsString(this IEnumerable<IRegisterGroup> groups)
        {
            IEnumerable<int> addresses = groups.Select(group => group.Address);
            return String.Join(", ", addresses);
        }

        public static void Update(this IReadOnlyDictionary<IRegisterGroup, IRegister[]> registerMap)
        {
            foreach (IRegisterGroup registerGroup in registerMap.Keys)
                registerGroup.Update(registerMap[registerGroup]);
        }

        public static void UpdateWithoutValues(this IReadOnlyDictionary<IRegisterGroup, IRegister[]> registerMap)
        {
            foreach (IRegisterGroup registerGroup in registerMap.Keys)
                registerGroup.UpdateWithoutValues();
        }
        
        public static IReadOnlyDictionary<IRegisterGroup, IRegister[]> MapRegistersToAllGroups(this IEnumerable<IRegisterGroup> mapAtGroups, 
            IReadOnlyCollection<IRegister> registersToMap)
        {
            var result = new Dictionary<IRegisterGroup, IRegister[]>();

            try
            {
                foreach (IRegisterGroup group in mapAtGroups)
                {
                    IEnumerable<IRegister> registers = Enumerable.Range(group.Address, group.Count)
                        .Select(address => registersToMap.Single(register => register.Address == address));

                    result.Add(group, registers.ToArray());
                }
            }
            catch (InvalidOperationException exception)
            {
                throw new InvalidRegistersException("Failed to map registers strictly", exception);
            }

            return result;
        }
        
        public static IReadOnlyDictionary<IRegisterGroup, IRegister[]> MapRegistersToPossibleGroups(
            this IReadOnlyCollection<IRegisterGroup> matAtGroups,
            IReadOnlyList<IRegister> registersToMap)
        {
            var result = new Dictionary<IRegisterGroup, IRegister[]>();
            
            var registerIndex = 0;
            while (registerIndex < registersToMap.Count)
            {
                IRegister headRegister = registersToMap[registerIndex];

                IRegisterGroup deviceGroup = matAtGroups.FirstOrDefault(register => register.Address == headRegister.Address);
                if (deviceGroup == null)
                    throw new InvalidRegistersException($"Register group with address {headRegister.Address} was not found");

                IRegister[] requestRegistersInGroup = Enumerable.Range(deviceGroup.Address, deviceGroup.Count)
                    .Select(address => registersToMap[registerIndex++].WithAddressAssert(address))
                    .ToArray();

                result.Add(deviceGroup, requestRegistersInGroup);
            }
            
            return result;
        }

        public static int GetLastRegisterAddress(this IRegisterGroup registerGroup)
            => registerGroup.GetRawRegisters().Last().Address;
    }
}