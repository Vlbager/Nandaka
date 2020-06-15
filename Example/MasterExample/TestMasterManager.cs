using System.Collections.Generic;
using Nandaka.Core.Device;

namespace Example.MasterExample
{
    public class TestMasterManager : MasterDeviceManager
    {
        public TestDevice TestDevice { get; }
        public override IReadOnlyCollection<NandakaDevice> SlaveDevices { get; }

        public TestMasterManager()
        {
            TestDevice = TestDevice.Create();
            SlaveDevices = new[] {TestDevice};
        }
    }
}