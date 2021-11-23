using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Nandaka.Core.Device
{
    internal sealed class UpdateCandidatesHolder
    {
        private readonly IDeviceUpdatePolicy _policy;
        private readonly IReadOnlyCollection<ForeignDevice> _candidates;

        public UpdateCandidatesHolder(IReadOnlyCollection<ForeignDevice> devices, IDeviceUpdatePolicy updatePolicy)
        {
            _candidates = devices;
            _policy = updatePolicy;
        }
        
        public IEnumerable<ForeignDevice> GetDevicesForProcessing(ILogger logger)
        {
            return _candidates.Where(device => _policy.IsDeviceShouldBeProcessed(device, logger));
        }

        public override string ToString()
        {
            return String.Join(Environment.NewLine, _candidates.Select(device => device.ToLogLine()));
        }
    }
}