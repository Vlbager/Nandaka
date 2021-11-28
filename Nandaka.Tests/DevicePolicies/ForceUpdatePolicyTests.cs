using System;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Nandaka.Core.Device;
using Nandaka.Model.Device;
using Nandaka.Tests.Util;
using Xunit;

namespace Nandaka.Tests.DevicePolicies
{
    public sealed class ForceUpdatePolicyTests
    {
        private readonly IDeviceUpdatePolicy _policy;

        public ForceUpdatePolicyTests()
        {
            _policy = new ForceUpdatePolicy(TimeSpan.Zero, TimeSpan.Zero);
        }

        [Theory]
        [Trait("Is Device Should Be Processed", "Always should be processed")]
        [InlineData(DeviceState.Connected)]
        [InlineData(DeviceState.Disconnected)]
        public void CheckIsDeviceShouldBeProcessed(DeviceState state)
        {
            // Arrange
            var device = new TestDevice { State = state };

            // Act
            bool isDeviceShouldBeProcessed = _policy.IsDeviceShouldBeProcessed(device, NullLogger.Instance);

            // Assert
            isDeviceShouldBeProcessed.Should().BeTrue();
        }

        [Fact]
        [Trait("On Message Received", "Should not throw")]
        public void OnMessageReceived()
        {
            // Arrange
            var device = new TestDevice();

            // Act & Assert
            _policy.Invoking(policy => policy.OnMessageReceived(device, NullLogger.Instance))
                   .Should().NotThrow();
        }
        
        [Fact]
        [Trait("On Error Occured", "Should not throw")]
        public void OnErrorOccured()
        {
            // Arrange
            var device = new TestDevice();

            // Act & Assert
            _policy.Invoking(policy => policy.OnErrorOccured(device, DeviceError.ErrorReceived, NullLogger.Instance))
                   .Should().NotThrow();
        }
    }
}