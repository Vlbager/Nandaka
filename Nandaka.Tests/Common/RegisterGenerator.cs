using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Registers;

namespace Nandaka.Tests.Common
{
    public class RegisterGenerator
    {
        private readonly Func<int, RegisterType, IRegister> _registerFactory;
        
        public static readonly IEnumerable<RegisterType> Types = new[] { RegisterType.ReadRequest, RegisterType.WriteRequest };
        
        public int RegisterValueSize { get; }

        public RegisterGenerator(Func<int, RegisterType, IRegister> registerFactory, int registerValueSize)
        {
            _registerFactory = registerFactory;
            RegisterValueSize = registerValueSize;
        }

        public IEnumerable<IRegister> Generate(IEnumerable<int> addresses)
        {
            return from type in Types 
                   from address in addresses
                   select _registerFactory(address, type);
        }

        public IEnumerable<IRegister> Generate(IEnumerable<int> addresses, RegisterType type)
        {
            return addresses.Select(address => _registerFactory(address, type));
        }

        public IEnumerable<IRegister[]> GenerateBatches(IEnumerable<int> batchSizes, IReadOnlyCollection<int> addressPool)
        {
            return from batch in batchSizes.Select(addressPool.GetCircular)
                   from registerType in Types
                   select Generate(batch, registerType).ToArray();
        }
    }
}