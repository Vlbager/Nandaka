using System.Collections;
using System.Collections.Generic;
using Nandaka.Core.Exceptions;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Registers
{
    // Do not change class name (source generator)
    public sealed class RegisterTable : IReadOnlyCollection<IRegister>
    {
        private readonly Dictionary<int, IRegister> _dictionary;

        private RegisterTable(Dictionary<int, IRegister> dictionary)
        {
            _dictionary = dictionary;
        }

        // Do not change method name (source generator)
        public static RegisterTable CreateWithValidation(IEnumerable<IRegister> registers)
        {
            var dictionary = new Dictionary<int, IRegister>();

            foreach (IRegister register in registers)
            {
                if (dictionary.ContainsKey(register.Address))
                    throw new ConfigurationException("Two or more registers with same addresses was defined");

                if (register.Address < 0)
                    throw new ConfigurationException("Register address should not be negative");
                
                if (register.RegisterType != RegisterType.ReadRequest && register.RegisterType != RegisterType.WriteRequest)
                    throw new ConfigurationException("Registers must have WriteRequest or ReadRequest type");
                
                dictionary.Add(register.Address, register);
            }

            return new RegisterTable(dictionary);
        }
        
        public IRegister this[int key] => _dictionary[key];

        public bool TryGetRegister(int address, out IRegister? register)
        {
            return _dictionary.TryGetValue(address, out register);
        }

        #region IReadOnlyCollection

        public int Count => _dictionary.Count;

        public IEnumerator<IRegister> GetEnumerator() => _dictionary.Values.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion
    }
}