using System;
using FluentAssertions;
using Nandaka.Core.Device;
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
            bool isDeviceShouldBeProcessed = _policy.IsDeviceShouldBeProcessed(device);

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
            _policy.Invoking(policy => policy.OnMessageReceived(device))
                   .Should().NotThrow();
        }
        
        [Fact]
        [Trait("On Error Occured", "Should not throw")]
        public void OnErrorOccured()
        {
            // Arrange
            var device = new TestDevice();

            // Act & Assert
            _policy.Invoking(policy => policy.OnErrorOccured(device, DeviceError.ErrorReceived))
                   .Should().NotThrow();
        }
    }
}