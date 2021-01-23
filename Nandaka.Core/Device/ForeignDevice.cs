using System.Collections.Generic;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;

namespace Nandaka.Core.Device
{
    public abstract class ForeignDevice : NandakaDevice
    {
        internal IRegistersUpdatePolicy UpdatePolicy { get; }
        public DeviceState State { get; set; }
        public Dictionary<DeviceError, int> ErrorCounter { get; }

        protected ForeignDevice(int address, RegisterTable table, DeviceState state, IRegistersUpdatePolicy updatePolicy, ISpecificMessageHandler specificMessageHandler)
            :base(address, table, specificMessageHandler)
        {
            UpdatePolicy = updatePolicy;
            ErrorCounter = new Dictionary<DeviceError, int>();
            State = state;
        }

        protected ForeignDevice(int address, RegisterTable table, IRegistersUpdatePolicy updatePolicy, DeviceState state)
            : this(address, table, state, updatePolicy, new NullSpecificMessageHandler()) { }
    }
}
