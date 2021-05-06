using System.Collections.Generic;

namespace Nandaka.Core.Device
{
    public interface IDeviceUpdatePolicyFactory
    {
        DeviceUpdatePolicy FactoryMethod(IReadOnlyCollection<ForeignDevice> devices);
    }
}