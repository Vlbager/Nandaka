using System;
using System.Collections.Generic;
using Nandaka.Core.Protocol;
using Nandaka.Core.Threading;

namespace Nandaka.Core.Device
{
    public abstract class MasterDeviceManager : IDisposable
    {
        private readonly ILog _log;

        private MasterThread _thread;

        public abstract IReadOnlyCollection<NandakaDevice> SlaveDevices { get; }

        protected MasterDeviceManager()
        {
            _log = Log.Instance;
        }

        public void Start(IProtocol protocol, IDeviceUpdatePolicy updatePolicy)
        {
            _log.AppendMessage(LogMessageType.Info, "Starting Master thread");
            _thread = MasterThread.Create(SlaveDevices, protocol, updatePolicy, _log);
            _thread.StartRoutine();
        }

        public void Dispose()
        {
            _thread?.Dispose();
        }
    }
}
