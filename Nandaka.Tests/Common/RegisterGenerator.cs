using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;
using Nandaka.Core.Table;

namespace Nandaka.Tests.Common
{
    public class RegisterGenerator
    {
        private readonly Func<int, RegisterType, IRegisterGroup> _registerFactory;
        
        public static readonly IEnumerable<RegisterType> Types = new[] { RegisterType.Read, RegisterType.ReadWrite };
        
        public int RegisterValueSize { get; }

        public RegisterGenerator(Func<int, RegisterType, IRegisterGroup> registerFactory, int registerValueSize)
        {
            _registerFactory = registerFactory;
            RegisterValueSize = registerValueSize;
        }

        public IEnumerable<IRegisterGroup> Generate(IEnumerable<int> addresses)
        {
            return from type in Types 
                   from address in addresses
                   select _registerFactory(address, type);
        }

        public IEnumerable<IRegisterGroup> Generate(IEnumerable<int> addresses, RegisterType type)
        {
            return addresses.Select(address => _registerFactory(address, type));
        }

        public IEnumerable<IRegisterGroup[]> GenerateBatches(IEnumerable<int> batchSizes, IReadOnlyCollection<int> addressList)
        {
            return from batch in batchSizes.Select(addressList.GetCircular)
                   from registerType in Types
                   select Generate(batch, registerType).ToArray();
        }
    }
}