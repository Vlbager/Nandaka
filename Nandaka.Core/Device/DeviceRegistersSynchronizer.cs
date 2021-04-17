using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Device
{
    internal sealed class DeviceRegistersSynchronizer
    {
        private readonly NandakaDevice _device;

        public DeviceRegistersSynchronizer(NandakaDevice device)
        {
            _device = device;
        }

        public IReadOnlyList<IRegister> UpdateAllRequested(IReadOnlyCollection<int> requestedAddresses, IEnumerable<IRegister> receivedRegisters)
        {
            IReadOnlyDictionary<IRegister, IRegister> updatePatch = GetAllRequestedUpdatePatch(requestedAddresses, receivedRegisters);

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
            return requestedRegisters.Select(register => GetDeviceRegister(register.Address))
                                     .ToArray();
        }

        public void MarkAsUpdatedAllRequested(IReadOnlyCollection<int> requestedAddresses)
        {
            IReadOnlyList<IRegister> requestedRegisters = GetAllRequestedRegisters(requestedAddresses);
            
            MarkRegistersAsUpdated(requestedRegisters);
        }
        

        private IReadOnlyDictionary<IRegister, IRegister> GetAllRequestedUpdatePatch(IReadOnlyCollection<int> requestedAddresses,
                                                                                     IEnumerable<IRegister> receivedRegisters)
        {
            Dictionary<IRegister, IRegister> updatePatch = requestedAddresses
                                                           .Join(receivedRegisters,
                                                                 requestedAddress => requestedAddress,
                                                                 receivedRegister => receivedRegister.Address,
                                                                 (requestedAddress, receivedRegister) =>
                                                                     (requestedRegister: _device.Table[requestedAddress], receivedRegister))
                                                           .ToDictionary(pair => pair.requestedRegister,
                                                                         pair => pair.receivedRegister);

            if (updatePatch.Count != requestedAddresses.Count)
                throw new InvalidRegistersReceivedException("Wrong received registers count. " +
                                                            $"Expected: {requestedAddresses.Count.ToString()}; actual: {updatePatch.Count.ToString()}");
            return updatePatch;
        }
        
        private IReadOnlyDictionary<IRegister, IRegister> GetAllReceivedUpdatePatch(IEnumerable<IRegister> receivedRegisters)
        {
            var updatePatch = new Dictionary<IRegister, IRegister>();

            foreach (IRegister receivedRegister in receivedRegisters)
            {
                IRegister deviceRegister = GetDeviceRegister(receivedRegister.Address);
                
                updatePatch.Add(deviceRegister!, receivedRegister);
            }

            return updatePatch;
        }

        private IReadOnlyList<IRegister> GetAllRequestedRegisters(IReadOnlyCollection<int> requestedAddresses)
        {                                                         
            return requestedAddresses.Select(GetDeviceRegister)
                                     .ToArray();
        }
        
        private IRegister GetDeviceRegister(int address)
        {
            if (!_device.Table.TryGetRegister(address, out IRegister? deviceRegister))
                throw new InvalidRegistersReceivedException($"Register {address.ToString()} not found on {_device.Name} device");
            
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