using System.Collections.Generic;
using System.Linq;
using Nandaka.Core;
using Nandaka.Core.Device;

namespace Example
{
    public sealed class ConcreteMasterManager : MasterDeviceManager
    {
        public ConcreteDevice ConcreteDevice { get; }
        
        public override IReadOnlyCollection<NandakaDevice> SlaveDevices { get; }

        private ConcreteMasterManager()
        {
            ConcreteDevice = ConcreteDevice.Create();
            SlaveDevices = new[] {ConcreteDevice};
        }

        public static ConcreteMasterManager Create()
        {
            return new ConcreteMasterManager();
        }
    }
}