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

        private MasterDeviceDealer(IProtocol protocol, DeviceUpdatePolicy updatePolicy, string masterName)
        {
            SlaveDevices = updatePolicy.SlaveDevices;
            _sessionsHolder = MasterSessionHolderFactory.Create(protocol, updatePolicy, masterName);
            _sessionsHolder.StartRoutine();
        }

        public static MasterDeviceDealer Start(IProtocol protocol, IDeviceUpdatePolicyFactory updatePolicyFactory, IReadOnlyCollection<ForeignDevice> slaveDevices,
                                                string masterName = DefaultMasterName)
        {
            DeviceUpdatePolicy updatePolicy = updatePolicyFactory.FactoryMethod(slaveDevices);
            return new MasterDeviceDealer(protocol, updatePolicy, masterName);
        }

        public void Dispose()
        {
            _sessionsHolder.Dispose();
        }
    }
}