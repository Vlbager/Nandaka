using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Protocol;
using Nandaka.Core.Threading;

namespace Nandaka.Core.Device
{
    public abstract class MasterDeviceManager
    {
        private readonly ILog _log;

        private MasterThread _thread;

        public IReadOnlyCollection<NandakaDevice> SlaveDevices { get; }

        protected MasterDeviceManager(IEnumerable<NandakaDevice> slaveDevices, ILog log)
        {
            _log = log;
            SlaveDevices = slaveDevices.ToArray();
        }

        public void Start(IProtocol protocol, IDeviceUpdatePolicy updatePolicy)
        {
            _log.AppendMessage(LogMessageType.Info, "Starting thread");
            _thread = MasterThread.Create(this, protocol, updatePolicy, _log);
            _thread.StartRoutine();
        }
    }
}
