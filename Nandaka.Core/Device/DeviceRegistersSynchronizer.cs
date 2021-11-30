using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Registers;
using Nandaka.Model.Registers;

namespace Nandaka.Core.Device
{
    internal sealed class DeviceRegistersSynchronizer
    {
        private readonly NandakaDevice _device;

        public DeviceRegistersSynchronizer(NandakaDevice device)
        {
            _device = device;
        }

        public IReadOnlyList<IRegister> UpdateAllRequested(IReadOnlyCollection<IRegister> requestedRegisters, IEnumerable<IRegister> receivedRegisters)
        {
            IReadOnlyDictionary<IRegister, IRegister> updatePatch = GetAllRequestedUpdatePatch(requestedRegisters, receivedRegisters);

            UpdateRegisters(updatePatch);

            return updatePatch.Keys.ToArray();
        }

        public IReadOnlyList<IRegister> UpdateAllReceived(IEnumerable<IRegister> receivedRegisters)
        {
            IReadOnlyDictionary<IRegister, IRegister> updatePatch = GetAllReceivedUpdatePatch(receivedRegisters);

            UpdateRegisters(updatePatch);

            return updatePatch.Keys.ToArray();
        }
        
        public IReadOnlyList<IRegister> GetDeviceRegisters(IReadOnlyCollection<IRegister> requestedRegisters)
        {
            return requestedRegisters.Select(GetDeviceRegister)
                                     .ToArray();
        }

        public void MarkAsUpdatedAllRequested(IReadOnlyCollection<IRegister> requestedRegisters)
        {
            IReadOnlyList<IRegister> deviceRegisters = GetAllDeviceRegisters(requestedRegisters);
            
            MarkRegistersAsUpdated(deviceRegisters);
        }
        

        private IReadOnlyDictionary<IRegister, IRegister> GetAllRequestedUpdatePatch(IReadOnlyCollection<IRegister> requestedRegisters,
                                                                                     IEnumerable<IRegister> receivedRegisters)
        {
            Dictionary<IRegister, IRegister> updatePatch = requestedRegisters
                                                           .Join(receivedRegisters,
                                                                 requesterRegister => requesterRegister.Address,
                                                                 receivedRegister => receivedRegister.Address,
                                                                 (requesterRegister, receivedRegister) =>
                                                                     (requestedRegister: _device.Table[requesterRegister.Address], receivedRegister))
                                                           .ToDictionary(pair => pair.requestedRegister,
                                                                         pair => pair.receivedRegister);

            if (updatePatch.Count != requestedRegisters.Count)
                throw new InvalidRegistersReceivedException("Wrong received registers count. " +
                                                            $"Expected: {requestedRegisters.Count.ToString()}; actual: {updatePatch.Count.ToString()}");
            return updatePatch;
        }
        
        private IReadOnlyDictionary<IRegister, IRegister> GetAllReceivedUpdatePatch(IEnumerable<IRegister> receivedRegisters)
        {
            var updatePatch = new Dictionary<IRegister, IRegister>();

            foreach (IRegister receivedRegister in receivedRegisters)
            {
                IRegister deviceRegister = GetDeviceRegister(receivedRegister);
                
                updatePatch.Add(deviceRegister!, receivedRegister);
            }

            return updatePatch;
        }

        private IReadOnlyList<IRegister> GetAllDeviceRegisters(IReadOnlyCollection<IRegister> requestedRegisters)
        {                                                         
            return requestedRegisters.Select(GetDeviceRegister)
                                     .ToArray();
        }
        
        private IRegister GetDeviceRegister(IRegister requestedRegister)
        {
            if (!_device.Table.TryGetRegister(requestedRegister.Address, out IRegister? deviceRegister))
                throw new InvalidRegistersReceivedException($"Register {requestedRegister.ToString()} not found on {_device.Name} device");
            
            return deviceRegister!;
        }

        private static void MarkRegistersAsUpdated(IReadOnlyCollection<IRegister> registers)
        {
            foreach (IRegister register in registers)
                register.MarkAsUpdated();
        }

        private static void UpdateRegisters(IReadOnlyDictionary<IRegister, IRegister> updatePatch)
        {
            foreach (var (deviceRegister, receivedRegister) in updatePatch)
                deviceRegister.Update(receivedRegister);
        }
    }
}