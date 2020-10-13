using System;
using System.Collections.Generic;
using Nandaka.Core.Protocol;
using Nandaka.Core.Threading;

namespace Nandaka.Core.Device
{
    public sealed class MasterDeviceManager : IDisposable
    {
        private readonly ILog _log;

        private MasterThread _thread;
        private readonly List<NandakaDevice> _slaveDevices;

        public IReadOnlyCollection<NandakaDevice> SlaveDevices => _slaveDevices;

        public MasterDeviceManager()
        {
            _log = Log.Instance;
            _slaveDevices = new List<NandakaDevice>();
        }

        public void AddSlaveDevice(NandakaDevice slaveDevice)
        {
            slaveDevice.Reflect(isManagedByMaster: true);
            _slaveDevices.Add(slaveDevice);
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