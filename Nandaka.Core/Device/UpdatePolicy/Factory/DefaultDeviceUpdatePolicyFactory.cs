using System;
using System.Collections.Generic;

namespace Nandaka.Core.Device
{
    /// <summary>
    /// <inheritdoc cref="DefaultUpdatePolicy"/>
    /// </summary>
    public sealed class DefaultDeviceUpdatePolicyFactory : IDeviceUpdatePolicyFactory
    {
        // todo: specify default values.
        private const int DefaultMaxErrorInRowCount = 3;
        private const int DefaultWaitResponseTimeoutMilliseconds = 300;
        private const int DefaultUpdateTimeoutMilliseconds = 300;
        
        private readonly TimeSpan _requestTimeout;
        private readonly TimeSpan _updateTimeout;
        private readonly int _maxErrorInRowCount;

        /// <param name="requestTimeout">Timeout between request and response</param>
        /// <param name="updateTimeout">Timeout between each update cycle</param>
        /// <param name="maxErrorInRowCount">The number of errors in a row, which is enough to disable the device</param>
        public DefaultDeviceUpdatePolicyFactory(TimeSpan requestTimeout, TimeSpan updateTimeout, int maxErrorInRowCount)
        {
            _requestTimeout = requestTimeout;
            _updateTimeout = updateTimeout;
            _maxErrorInRowCount = maxErrorInRowCount;
        }
        
        /// <param name="waitResponseTimeoutMilliseconds">Timeout between request and response</param>
        /// <param name="updateTimoutMilliseconds">Timeout between each update cycle</param>
        /// <param name="maxErrorInRowCount">The number of errors in a row, which is enough to disable the device</param>
        public DefaultDeviceUpdatePolicyFactory(int waitResponseTimeoutMilliseconds = DefaultWaitResponseTimeoutMilliseconds,
                                                int updateTimoutMilliseconds = DefaultUpdateTimeoutMilliseconds,
                                                int maxErrorInRowCount = DefaultMaxErrorInRowCount)
            : this(TimeSpan.FromMilliseconds(waitResponseTimeoutMilliseconds),
                   TimeSpan.FromMilliseconds(updateTimoutMilliseconds),
                   maxErrorInRowCount)
        {
        }

        public DeviceUpdatePolicy FactoryMethod(IReadOnlyCollection<ForeignDevice> devices)
        {
            return new DefaultUpdatePolicy(devices, _requestTimeout, _updateTimeout, _maxErrorInRowCount);
        }
    }
}