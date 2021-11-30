using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.Model.Registers;
using Nandaka.Tests.Util;
using Xunit;

namespace Nandaka.Tests.Session
{
    public sealed class SlaveSyncSessionTests
    {
        private readonly SlaveSyncSession _session;
        private readonly Mock<IProtocol> _protocolMock;
        private readonly TestDevice _device;

        private readonly List<int> _updatedRegisterAddresses;
        private int _sentMessagesCount;

        public SlaveSyncSessionTests()
        {
            _protocolMock = new Mock<IProtocol>();
            _device = new TestDevice(new LocalDeviceRegisterFactory());
            _session = new SlaveSyncSession(_protocolMock.Object, _device, NullLogger.Instance);
            _updatedRegisterAddresses = new List<int>();
            _device.OnRegisterChanged += (_, args) => _updatedRegisterAddresses.Add(args.RegisterAddress);
            SetupMocks();
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [Trait("Process Request", "Single received register should be updated and response should be sent")]
        public void ProcessReadResponseWithSingleRegister(bool isResponseMayBeSkipped)
        {
            // Arrange
            SetupIsResponseMayBeSkippedByProtocol(isResponseMayBeSkipped);
            IRegisterMessage message = CreateRequestFromDeviceRegisters(OperationType.Read, _device.RwInt);
            
            int[] requestedRegistersAddresses = message.Registers.Select(register => register.Address).ToArray();

            // Act
            _session.ProcessRequest(message);

            // Assert
            _updatedRegisterAddresses.Should().Equal(requestedRegistersAddresses);
            _sentMessagesCount.Should().Be(1);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [Trait("Process Request", "Single received register should be updated and response should be skipped if protocol allow it")]
        public void ProcessWriteResponseWithSingleRegister(bool isResponseMayBeSkipped)
        {
            // Arrange
            SetupIsResponseMayBeSkippedByProtocol(isResponseMayBeSkipped);
            IRegisterMessage message = CreateRequestFromDeviceRegisters(OperationType.Write, _device.RoInt);
            
            int[] requestedRegistersAddresses = message.Registers.Select(register => register.Address).ToArray();

            // Act
            _session.ProcessRequest(message);

            // Assert
            _updatedRegisterAddresses.Should().Equal(requestedRegistersAddresses);
            _sentMessagesCount.Should().Be(isResponseMayBeSkipped ? 0 : 1, "response for write request may be skipped");
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [Trait("Process Request", "Received registers should be updated and response should be sent")]
        public void ProcessReadResponseWithManyRegisters(bool isResponseMayBeSkipped)
        {
            // Arrange
            SetupIsResponseMayBeSkippedByProtocol(isResponseMayBeSkipped);
            IRegisterMessage message = CreateRequestFromDeviceRegisters(OperationType.Read, _device.RwInt, _device.RwShort);
            
            int[] requestedRegistersAddresses = message.Registers.Select(register => register.Address).ToArray();

            // Act
            _session.ProcessRequest(message);

            // Assert
            _updatedRegisterAddresses.Should().Equal(requestedRegistersAddresses);
            _sentMessagesCount.Should().Be(1);
        }
        
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [Trait("Process Request", "Received registers should be updated and response should be skipped if protocol allow it")]
        public void ProcessWriteResponseWithManyRegisters(bool isResponseMayBeSkipped)
        {
            // Arrange
            SetupIsResponseMayBeSkippedByProtocol(isResponseMayBeSkipped);
            IRegisterMessage message = CreateRequestFromDeviceRegisters(OperationType.Write, _device.RoInt, _device.RoShort);
            
            int[] requestedRegistersAddresses = message.Registers.Select(register => register.Address).ToArray();

            // Act
            _session.ProcessRequest(message);

            // Assert
            _updatedRegisterAddresses.Should().Equal(requestedRegistersAddresses);
            _sentMessagesCount.Should().Be(isResponseMayBeSkipped ? 0 : 1, "response for write request may be skipped");
        }
        
        private void SetupIsResponseMayBeSkippedByProtocol(bool isResponseMayBeSkipped)
        {
            _protocolMock.SetupGet(proto => proto.IsResponseMayBeSkipped)
                         .Returns(isResponseMayBeSkipped);
        }

        private void SetupMocks()
        {
            _protocolMock.Setup(proto => proto.SendMessage(It.IsAny<IMessage>()))
                         .Callback<IMessage>(_ => _sentMessagesCount++);
        }

        private IRegisterMessage CreateRequestFromDeviceRegisters(OperationType operationType, params IRegister[] deviceRegisters)
        {
            IRegister[] receivedRegisters = deviceRegisters.Select(register => register.CreateCopy()).ToArray();
            return new CommonMessage(_device.Address, MessageType.Request, operationType, receivedRegisters);
        }
    }
}