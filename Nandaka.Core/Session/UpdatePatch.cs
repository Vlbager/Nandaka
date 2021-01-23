using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Registers;

namespace Nandaka.Core.Session
{
    public sealed class UpdatePatch
    {
        private readonly IReadOnlyDictionary<IRegister, IRegister> _patch;

        public IRegister[] DeviceRegisters => _patch.Keys.ToArray();

        private UpdatePatch(IReadOnlyDictionary<IRegister, IRegister> patch)
        {
            _patch = patch;
        }

        public void Apply()
        {
            foreach (var (deviceRegister, receivedRegister) in _patch)
                deviceRegister.Update(receivedRegister);
        }

        public static UpdatePatch GetPatchForAllRegisters(NandakaDevice device, IReadOnlyCollection<int> requestedAddresses, 
                                                          IEnumerable<IRegister> receivedRegisters)
        {
            Dictionary<IRegister, IRegister> result = requestedAddresses
                                                      .Join(receivedRegisters,
                                                            requestedAddress => requestedAddress,
                                                            receivedRegister => receivedRegister.Address,
                                                            (requestedAddress, receivedRegister) => 
                                                                (requestedRegister: device.Table[requestedAddress], receivedRegister))
                                                      .ToDictionary(pair => pair.requestedRegister,
                                                                    pair => pair.receivedRegister);

            if (result.Count != requestedAddresses.Count)
                throw new InvalidRegistersReceivedException("Wrong received registers count. " +
                                                            $"Expected: {requestedAddresses.Count.ToString()}; actual: {result.Count.ToString()}");

            return new UpdatePatch(result);
        }

        public static UpdatePatch GetPatchForPossibleRegisters(NandakaDevice device, IEnumerable<IRegister> receivedRegisters)
        {
            var result = new Dictionary<IRegister, IRegister>();

            foreach (IRegister receivedRegister in receivedRegisters)
            {
                if (!device.Table.TryGetRegister(receivedRegister.Address, out IRegister? deviceRegister))
                    throw new InvalidRegistersReceivedException($"Register with address {receivedRegister.Address.ToString()} was not found");
                
                result.Add(deviceRegister!, receivedRegister);
            }

            return new UpdatePatch(result);
        }
    }
}