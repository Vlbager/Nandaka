using Nandaka.Core.Device;

namespace Example
{
    public class ConcreteSlaveManager : SlaveDeviceManager
    {
        public ConcreteDevice ConcreteDevice { get; }
        
        protected override NandakaDevice Device => ConcreteDevice;
        
        private ConcreteSlaveManager()
        {
            ConcreteDevice = ConcreteDevice.Create();
        }

        public static ConcreteSlaveManager Create()
        {
            return new ConcreteSlaveManager();
        }
    }
}