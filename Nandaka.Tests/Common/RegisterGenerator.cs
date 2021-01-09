using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Nandaka.Core.Registers;

namespace Nandaka.Tests.Common
{
    internal abstract class RegisterGenerator
    {
        protected static readonly IEnumerable<RegisterType> Types = new[] { RegisterType.ReadRequest, RegisterType.WriteRequest };
    }
    
    internal sealed class RegisterGenerator<T> : RegisterGenerator 
        where T: struct
    {
        public static int RegisterValueSize { get; } = Marshal.SizeOf<T>();

        public IEnumerable<IRegister<T>> Generate(IEnumerable<int> addresses)
        {
            return from type in Types 
                   from address in addresses
                   select new Register<T>(address, type);
        }

        public IEnumerable<IRegister<T>> Generate(IEnumerable<int> addresses, RegisterType type)
        {
            return addresses.Select(address => new Register<T>(address, type));
        }

        public IEnumerable<IRegister<T>[]> GenerateBatches(IEnumerable<int> batchSizes, IReadOnlyCollection<int> addressPool)
        {
            return from batch in batchSizes.Select(addressPool.GetCircular)
                   from registerType in Types
                   select Generate(batch, registerType).ToArray();
        }
    }
}