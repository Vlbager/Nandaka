using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;

namespace Nandaka.Core.Table
{
    public class RegisterTableValidator
    {
        private readonly IList<IRegisterGroup> _registerGroups;
        private readonly HashSet<int> _addressSet;

        public RegisterTableValidator()
        {
            _registerGroups = new List<IRegisterGroup>();
            _addressSet = new HashSet<int>();
        }

        public T AddGroup<T>(T registerGroup) where T : IRegisterGroup
        {
            int[] registerAddresses = registerGroup.GetRawRegisters()
                .Select(register => register.Address)
                .ToArray();
            
            if (registerAddresses.Any(address => _addressSet.Contains(address)))
                throw new NandakaBaseException("Can't add new register to table: register with specified address already exists");

            foreach (int address in registerAddresses)
                _addressSet.Add(address);

            _registerGroups.Add(registerGroup);

            return registerGroup;
        }

        public IRegisterGroup[] GetGroups() => _registerGroups.ToArray();
    }
}
