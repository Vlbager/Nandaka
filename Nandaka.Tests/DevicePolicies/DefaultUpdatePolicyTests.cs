using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Nandaka.Core.Device;
using Nandaka.Tests.Util;
using Xunit;

namespace Nandaka.Tests.DevicePolicies
{
    public sealed class DefaultUpdatePolicyTests
    {
        private readonly IDeviceUpdatePolicy _policy;

        public DefaultUpdatePolicyTests()
        {
            _policy = new DefaultUpdatePolicy(TimeSpan.Zero, TimeSpan.Zero, maxErrorInRowCount: 2);
        }

        [Fact]
        [Trait("Is Device Should Be Processed", "Should be processed in connected state")]
        public void CheckIsConnectedDeviceShouldBeProcessed()
        {
            // Arrange
            var device = new TestDevice { State = DeviceState.Connected };

            // Act
            bool isDeviceShouldBeProcessed = _policy.IsDeviceShouldBeProcessed(device, NullLogger.Instance);

            // Assert
            isDeviceShouldBeProcessed.Should().BeTrue();
        }
        
        [Theory]
        [InlineData(DeviceState.Corrupted)]
        [InlineData(DeviceState.NotResponding)]
        [InlineData(DeviceState.Disconnected)]
        [Trait("Is Device Should Be Processed", "Should not be processed any not connected state")]
        public void CheckIsDisconnectedDeviceShouldBeProcessed(DeviceState state)
        {
            // Arrange
            var device = new TestDevice { State = state };

            // Act
            bool isDeviceShouldBeProcessed = _policy.IsDeviceShouldBeProcessed(device, NullLogger.Instance);

            // Assert
            isDeviceShouldBeProcessed.Should().BeFalse();
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
        [Trait("On Error Occured", "Should stay connected")]
        public void OnSingleErrorOccured()
        {
            // Arrange
            var device = new TestDevice();

            // Act
            _policy.OnErrorOccured(device, DeviceError.ErrorReceived, NullLogger.Instance);
            
            // Assert
            device.State.Should().Be(DeviceState.Connected);
        }
        
        [Fact]
        [Trait("On Error Occured", "Should be disconnected")]
        public void OnDoubleSameErrorsOccured()
        {
            // Arrange
            var device = new TestDevice();

            // Act
            _policy.OnErrorOccured(device, DeviceError.ErrorReceived, NullLogger.Instance);
            _policy.OnErrorOccured(device, DeviceError.ErrorReceived, NullLogger.Instance);
            
            // Assert
            device.State.Should().NotBe(DeviceState.Connected);
        }
        
        [Fact]
        [Trait("On Error Occured", "Should stay connected")]
        public void OnDoubleDifferentErrorsOccured()
        {
            // Arrange
            var device = new TestDevice();

            // Act
            _policy.OnErrorOccured(device, DeviceError.ErrorReceived, NullLogger.Instance);
            _policy.OnErrorOccured(device, DeviceError.WrongPacketData, NullLogger.Instance);
            
            // Assert
            device.State.Should().Be(DeviceState.Connected);
        }
        
        [Fact]
        [Trait("On Error Occured", "Should stay connected")]
        public void OnErrorThenMessageThenErrorOccured()
        {
            // Arrange
            var device = new TestDevice();

            // Act
            _policy.OnErrorOccured(device, DeviceError.ErrorReceived, NullLogger.Instance);
            _policy.OnMessageReceived(device, NullLogger.Instance);
            _policy.OnErrorOccured(device, DeviceError.ErrorReceived, NullLogger.Instance);

            // Assert
            device.State.Should().Be(DeviceState.Connected);
        }
    }
}