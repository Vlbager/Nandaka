using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Session;
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

        public static void Update(this IEnumerable<IRegisterGroup> targetGroups, IReadOnlyCollection<IRegister> sourceRegister,
            OperationType operationType)
        {
            IReadOnlyDictionary<IRegisterGroup, IRegister[]> registerMap = MapRegisters(targetGroups, sourceRegister);

            if (operationType == OperationType.Write)
                return;
            if (operationType != OperationType.Read)
                // todo: create a custom exception
                throw new Exception("Wrong Operation type");

            foreach (IRegisterGroup registerGroup in registerMap.Keys)
                registerGroup.Update(registerMap[registerGroup]);
        }
        
        private static IReadOnlyDictionary<IRegisterGroup, IRegister[]> MapRegisters(IEnumerable<IRegisterGroup> mapAtGroups, 
            IReadOnlyCollection<IRegister> mapRegisters)
        {
            var result = new Dictionary<IRegisterGroup, IRegister[]>();

            try
            {
                foreach (IRegisterGroup group in mapAtGroups)
                {
                    IEnumerable<IRegister> registers = Enumerable.Range(group.Address, group.Count)
                        .Select(address => mapRegisters.Single(register => register.Address == address));

                    result.Add(group, registers.ToArray());
                }
            }
            catch (InvalidOperationException exception)
            {
                // todo: create a custom exception
                throw new Exception("Wrong registers received", exception);
            }

            return result;
        }
    }
}