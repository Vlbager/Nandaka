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
        
        public int RegisterSize { get; }

        private static readonly IEnumerable<RegisterType> Types = new[] { RegisterType.Read, RegisterType.ReadWrite };

        public RegisterGenerator(Func<int, RegisterType, IRegisterGroup> registerFactory, int registerSize)
        {
            _registerFactory = registerFactory;
            RegisterSize = registerSize;
        }

        public IEnumerable<IRegisterGroup> Generate(int address)
        {
            return Generate(address.ToEnumerable());
        }

        public IEnumerable<IRegisterGroup> Generate(IEnumerable<int> addresses)
        {
            return from address in addresses
                   from type in Types
                   select _registerFactory(address, type);
        }

        public IEnumerable<IRegisterGroup> GenerateRange(int startAddress, int count)
        {
            return Generate(Enumerable.Range(startAddress, count));
        }

        public IEnumerable<IRegisterGroup[]> GenerateBatches(IEnumerable<int> batchSizes, IReadOnlyCollection<int> addressList)
        {
            return batchSizes.Select(addressList.GetCircular)
                             .Select(addressesBatch => Generate(addressesBatch).ToArray());
        }
    }
}