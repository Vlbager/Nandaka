using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Helpers
{
    internal static class RegisterExtensions
    {
        public static string GetAllAddressesAsString(this IEnumerable<IRegister> groups)
        {
            IEnumerable<int> addresses = groups.Select(group => group.Address);
            return String.Join(", ", addresses);
        }

        public static void Update(this IReadOnlyDictionary<IRegister, IRegister> registerMap)
        {
            foreach (IRegister updateRegister in registerMap.Keys)
                updateRegister.Update(registerMap[updateRegister]);
        }

        public static void UpdateWithoutValues(this IReadOnlyDictionary<IRegister, IRegister> registerMap)
        {
            foreach (IRegister registerGroup in registerMap.Keys)
                registerGroup.UpdateWithoutValues();
        }
        
        public static IReadOnlyDictionary<IRegister, IRegister> MapRegistersToAllGroups(this IReadOnlyCollection<IRegister> mapAtRegisters, 
            IReadOnlyCollection<IRegister> registersToMap)
        {
            Dictionary<IRegister, IRegister> result = mapAtRegisters.Join(registersToMap,
                                                                          keyRegister => keyRegister.Address,
                                                                          valueRegister => valueRegister.Address,
                                                                          (keyRegister, valueRegister) => (keyRegister, valueRegister))
                                                                    .ToDictionary(pair => pair.keyRegister,
                                                                                  pair => pair.valueRegister);

            if (result.Count != mapAtRegisters.Count)
                throw new InvalidRegistersReceivedException($"Received {result.Count} registers instead of {mapAtRegisters.Count} register");

            return result;
        }
        
        public static IReadOnlyDictionary<IRegister, IRegister> MapRegistersAsPossible(
            this IReadOnlyCollection<IRegister> mapAtRegisters,
            IEnumerable<IRegister> registersToMap)
        {
            var result = new Dictionary<IRegister, IRegister>();
            
            foreach (IRegister sourceRegister in registersToMap)
            {
                IRegister? deviceRegister = mapAtRegisters.FirstOrDefault(register => register.Address == sourceRegister.Address);
                if (deviceRegister == null)
                    throw new InvalidRegistersReceivedException($"Register with address {sourceRegister.Address} was not found");
                
                result.Add(deviceRegister, sourceRegister);
            }
            
            return result;
        }
    }
}