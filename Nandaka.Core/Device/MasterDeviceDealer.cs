using System;
using System.Collections.Generic;
using Nandaka.Core.Protocol;
using Nandaka.Core.Threading;

namespace Nandaka.Core.Device
{
    public sealed class MasterDeviceDealer : IDisposable
    {
        private const string DefaultMasterName = "Master";
        
        private readonly IMasterSessionsHolder _sessionsHolder;

        public IReadOnlyCollection<ForeignDevice> SlaveDevices { get; }

        private MasterDeviceDealer(IProtocol protocol, IDeviceUpdatePolicy updatePolicy, IReadOnlyCollection<ForeignDevice> slaveDevices, string masterName)
        {
            SlaveDevices = slaveDevices;
            _sessionsHolder = MasterSessionHolderFactory.Create(protocol, updatePolicy, slaveDevices, masterName);
            _sessionsHolder.StartRoutine();
        }

        public static MasterDeviceDealer Start(IProtocol protocol, IDeviceUpdatePolicy updatePolicy, IReadOnlyCollection<ForeignDevice> slaveDevices,
                                                string masterName = DefaultMasterName)
        {
            return new MasterDeviceDealer(protocol, updatePolicy, slaveDevices, masterName);
        }

        public void Dispose()
        {
            _sessionsHolder.Dispose();
        }
    }
}