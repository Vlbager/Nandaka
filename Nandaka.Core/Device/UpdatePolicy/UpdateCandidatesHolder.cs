using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Device
{
    public sealed class UpdateCandidatesHolder
    {
        private readonly IDeviceUpdatePolicy _policy;
        
        public IReadOnlyCollection<ForeignDevice> Candidates { get; }

        public UpdateCandidatesHolder(IReadOnlyCollection<ForeignDevice> devices, IDeviceUpdatePolicy updatePolicy)
        {
            Candidates = devices;
            _policy = updatePolicy;
        }
        
        public IEnumerable<ForeignDevice> GetDevicesForProcessing()
        {
            return Candidates.Where(IsDeviceShouldBeProcessed);
        }

        private bool IsDeviceShouldBeProcessed(ForeignDevice device)
        {
            return _policy.IsDeviceShouldBeProcessed(device);
        }
    }
}