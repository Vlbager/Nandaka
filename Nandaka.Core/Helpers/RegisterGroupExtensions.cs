using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Table;

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
                // todo: create a custom exception
                throw new Exception("Failed to map registers strictly", exception);
            }

            return result;
        }
        
        public static IReadOnlyDictionary<IRegisterGroup, IRegister[]> MapRegistersToPossibleGroups(
            this IReadOnlyCollection<IRegisterGroup> matAtGroups,
            IReadOnlyList<IRegister> registersToMap)
        {
            var result = new Dictionary<IRegisterGroup, IRegister[]>();

            try
            {
                var registerIndex = 0;
                while (registerIndex < registersToMap.Count)
                {
                    IRegister headRegister = registersToMap[registerIndex];

                    IRegisterGroup deviceGroup = matAtGroups.FirstOrDefault(register => register.Address == headRegister.Address);
                    if (deviceGroup == null)
                        // todo: create a custom excepion
                        throw new Exception($"Register group with address {headRegister.Address} was not found");

                    IRegister[] requestRegistersInGroup = Enumerable.Range(deviceGroup.Address, deviceGroup.Count)
                        .Select(address => registersToMap[registerIndex++].WithAddressAssert(address))
                        .ToArray();

                    result.Add(deviceGroup, requestRegistersInGroup);
                }
            }
            catch (ApplicationException exception)
            {
                // todo: handle custom "register was not found" exception
                throw;
            }
            catch (Exception exception)
            {
                // todo: create a custom exception
                throw new Exception("Failed to map registers", exception);
            }

            return result;
        }

        public static int GetLastRegisterAddress(this IRegisterGroup registerGroup)
            => registerGroup.GetRawRegisters().Last().Address;
    }
}