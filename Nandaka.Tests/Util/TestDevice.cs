using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Registers;

namespace Nandaka.Tests.Util
{
    internal sealed class TestDevice : ForeignDevice
    {
        public new const int Address = 1;
        
        public override string Name => nameof(TestDevice);
            
        private TestDevice(RegisterTable table, DeviceState state) 
            : base(Address, table, state) { }

        public static TestDevice Create(IEnumerable<IRegister> registers)
        {
            var table = RegisterTable.CreateWithValidation(registers);
            return new TestDevice(table, DeviceState.Connected);
        }
    }
}