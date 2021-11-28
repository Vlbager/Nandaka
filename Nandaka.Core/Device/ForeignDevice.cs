using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.Model.Device;

namespace Nandaka.Core.Device
{
    public abstract class ForeignDevice : NandakaDevice, IForeignDevice
    {
        internal readonly IErrorMessageHandler? ErrorMessageHandlerField;
        internal readonly IRegistersUpdatePolicy UpdatePolicyField = new WriteFirstUpdatePolicy();
        
        internal IRegistersUpdatePolicy UpdatePolicy
        {
            init => UpdatePolicyField = value;
        }
        public IErrorMessageHandler ErrorMessageHandler
        {
            init => ErrorMessageHandlerField = value;
        }
        public DeviceState State { get; set; }

        protected ForeignDevice(int address, RegisterTable table, DeviceState state) 
            : base(address, table)
        {
            State = state;
        }
    }
}
