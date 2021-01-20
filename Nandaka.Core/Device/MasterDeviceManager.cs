using System;
using System.Collections.Generic;
using Nandaka.Core.Protocol;
using Nandaka.Core.Threading;

namespace Nandaka.Core.Device
{
    public sealed class MasterDeviceManager : IDisposable
    {
        private const string DefaultMasterName = "Default";
        
        private readonly MasterThread _thread;

        public IReadOnlyCollection<ForeignDeviceCtx> SlaveDevices { get; }

        private MasterDeviceManager(IProtocol protocol, IDeviceUpdatePolicy updatePolicy, IReadOnlyCollection<ForeignDeviceCtx> slaveDevices, string masterName)
        {
            SlaveDevices = slaveDevices;
            _thread = MasterThread.Create(SlaveDevices, protocol, updatePolicy, masterName);
            _thread.StartRoutine();
        }

        public static MasterDeviceManager Start(IProtocol protocol, IDeviceUpdatePolicy updatePolicy, IReadOnlyCollection<ForeignDeviceCtx> slaveDevices,
                                                string masterName = DefaultMasterName)
        {
            return new MasterDeviceManager(protocol, updatePolicy, slaveDevices, masterName);
        }

        public void Dispose()
        {
            _thread.Dispose();
        }
    }
}