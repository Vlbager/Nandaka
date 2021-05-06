using System;
using System.Collections.Generic;

namespace Nandaka.Core.Device
{
    /// <summary>
    /// <inheritdoc cref="ForceUpdatePolicy"/>
    /// </summary>
    public sealed class ForceUpdatePolicyFactory : IDeviceUpdatePolicyFactory
    {
        private const int DefaultWaitResponseTimeoutMilliseconds = 300;
        private const int DefaultUpdateTimeoutMilliseconds = 300;

        private readonly TimeSpan _requestTimeout;
        private readonly TimeSpan _updateTimeout;
        
        /// <param name="requestTimeout">Timeout between request and response</param>
        /// <param name="updateTimeout">Timeout between each update cycle</param>
        public ForceUpdatePolicyFactory(TimeSpan requestTimeout, TimeSpan updateTimeout)
        {
            _requestTimeout = requestTimeout;
            _updateTimeout = updateTimeout;
        }

        /// <param name="waitResponseTimeoutMilliseconds">Timeout between request and response</param>
        /// <param name="updateTimoutMilliseconds">Timeout between each update cycle</param>
        public ForceUpdatePolicyFactory(int waitResponseTimeoutMilliseconds = DefaultWaitResponseTimeoutMilliseconds,
                                        int updateTimoutMilliseconds = DefaultUpdateTimeoutMilliseconds)
            : this(TimeSpan.FromMilliseconds(waitResponseTimeoutMilliseconds),
                   TimeSpan.FromMilliseconds(updateTimoutMilliseconds))
        {
        } 
        
        public DeviceUpdatePolicy FactoryMethod(IReadOnlyCollection<ForeignDevice> devices)
        {
            return new ForceUpdatePolicy(devices, _requestTimeout, _updateTimeout);
        }
    }
}