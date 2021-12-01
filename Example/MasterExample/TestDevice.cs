using Nandaka.Core.Device;
using Nandaka.Model.Attributes;

namespace Example.MasterExample
{
    [GenerateDevice(typeof(TestDeviceTable))]
    public partial class TestDevice : ForeignDevice
    {

    }
}