using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using Nandaka.Core.Device;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.Tests.Util;
using Xunit;

namespace Nandaka.Tests.Session
{
    public sealed class MasterSyncSessionTests
    {
        private readonly MasterSyncSession _session;
        private readonly Mock<IProtocol> _protocolMock;
        private readonly Mock<IRegistersUpdatePolicy> _updatePolicyMock;
        private readonly TestDevice _device;

        private readonly List<int> _updatedRegisterAddresses;

        public MasterSyncSessionTests()
        {
            _protocolMock = new Mock<IProtocol>();
            _updatePolicyMock = new Mock<IRegistersUpdatePolicy>();
            _device = new TestDevice { UpdatePolicy = _updatePolicyMock.Object };
            _session = new MasterSyncSession(_protocolMock.Object, _device);
            _updatedRegisterAddresses = new List<int>();
            _device.OnRegisterChanged += (_, args) => _updatedRegisterAddresses.Add(args.RegisterAddress);
        }

        [Fact]
        [Trait("Get next message", "Session should just return message from policy mock")]
        public void GetMessageFromPolicy()
        {
            // Arrange
            int messageRequestCount = 0;
            var expectedMessage = EmptyMessage.Create();
            _updatePolicyMock.Setup(policy => policy.GetNextMessage(It.IsAny<ForeignDevice>()))
                             .Callback(() => messageRequestCount++)
                             .Returns(expectedMessage);


            // Act
            IRegisterMessage message = _session.GetNextMessage();

            // Assert
            message.Should().BeSameAs(expectedMessage);
            messageRequestCount.Should().Be(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        [Trait("Send Request", "Success result should be returned with same register address. Response should be always required")]
        public void SendReadRequestWithSingleRegister(bool isResponseMayBeSkippedByProtocol)
        {
            // Arrange
            SetupIsResponseMayBeSkippedByProtocol(isResponseMayBeSkippedByProtocol);
            IRegister register = _device.RoInt;
            SetupSentRegistersAddresses(new[] { register.Address });
            IRegisterMessage message = CreateRequestMessage(OperationType.Read, register);

            // Act
            RegisterRequestSentResult result = _session.SendRequest(message);

            // Assert
            result.IsResponseRequired.Should().Be(true, "read request always require response");
            result.RequestedAddresses.Should().Equal(register.Address);
            register.IsUpdated.Should().BeFalse("read request always require response for update");
        }
        
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [Trait("Send Request", "Success result should be returned with same register address. Response required depending on protocol")]
        public void SendWriteRequestWithSingleRegister(bool isResponseMayBeSkippedByProtocol)
        {
            // Arrange
            SetupIsResponseMayBeSkippedByProtocol(isResponseMayBeSkippedByProtocol);
            IRegister register = _device.RwInt;
            SetupSentRegistersAddresses(new[] { register.Address });
            IRegisterMessage message = CreateRequestMessage(OperationType.Write, register);

            // Act
            RegisterRequestSentResult result = _session.SendRequest(message);

            // Assert
            result.IsResponseRequired.Should().Be(!isResponseMayBeSkippedByProtocol);
            result.RequestedAddresses.Should().Equal(register.Address);
            register.IsUpdated.Should().Be(isResponseMayBeSkippedByProtocol, "we should mark register as updated from master side");
        }
        
        [Fact]
        [Trait("Send Request", "It's okay if protocol can't send all registers. Really sent registers should be in result")]
        public void SendReadRequestPartially()
        {
            // Arrange
            IRegister[] registersInMessage = { _device.RoInt, _device.RoByte, _device.RoInt2, _device.RoLong }; 
            IRegisterMessage message = CreateRequestMessage(OperationType.Read, registersInMessage);

            int[] sentRegistersAddresses = { _device.RoInt.Address, _device.RoByte.Address, _device.RoInt2.Address };
            SetupSentRegistersAddresses(sentRegistersAddresses);

            // Act
            RegisterRequestSentResult result = _session.SendRequest(message);

            // Assert
            result.RequestedAddresses.Should().Equal(sentRegistersAddresses);
        }
        
        [Fact]
        [Trait("Send Request", "Really sent registers should be in result. Register should be updated if response skipped")]
        public void SendWriteRequestPartiallyWhenResponseMayBeSkipped()
        {
            // Arrange
            SetupIsResponseMayBeSkippedByProtocol(true);
            IRegister[] registersInMessage = { _device.RwInt, _device.RwByte, _device.RwInt2, _device.RwLong }; 
            IRegisterMessage message = CreateRequestMessage(OperationType.Write, registersInMessage);

            int[] sentRegistersAddresses = { _device.RwInt.Address, _device.RwByte.Address, _device.RwInt2.Address };
            SetupSentRegistersAddresses(sentRegistersAddresses);

            // Act
            RegisterRequestSentResult result = _session.SendRequest(message);

            // Assert
            result.RequestedAddresses.Should().Equal(sentRegistersAddresses);
            _updatedRegisterAddresses.Should().Equal(sentRegistersAddresses);
        }
        
        [Fact]
        [Trait("Process Response", "Single received register should be updated")]
        public void ProcessReadResponseWithSingleRegister()
        {
            // Arrange
            IRegisterMessage message = CreateResponseMessageFromDeviceRegisters(OperationType.Read, _device.RoInt);
            
            int[] requestedRegistersAddresses = message.Registers.Select(register => register.Address).ToArray();
            
            var sentResult = new RegisterRequestSentResult(isResponseRequired: true, requestedRegistersAddresses);

            // Act
            _session.ProcessResponse(message, sentResult);

            // Assert
            _updatedRegisterAddresses.Should().Equal(requestedRegistersAddresses);
        }
        
        [Fact]
        [Trait("Process Response", "Single received register should be updated")]
        public void ProcessWriteResponseWithSingleRegister()
        {
            // Arrange
            IRegisterMessage message = CreateResponseMessageFromDeviceRegisters(OperationType.Write, _device.RwInt);
            
            int[] requestedRegistersAddresses = message.Registers.Select(register => register.Address).ToArray();
            
            var sentResult = new RegisterRequestSentResult(isResponseRequired: true, requestedRegistersAddresses);

            // Act
            _session.ProcessResponse(message, sentResult);

            // Assert
            _updatedRegisterAddresses.Should().Equal(requestedRegistersAddresses);
        }
        
        [Fact]
        [Trait("Process Response", "Received registers should be updated")]
        public void ProcessReadResponseWithManyRegisters()
        {
            // Arrange
            IRegisterMessage message = CreateResponseMessageFromDeviceRegisters(OperationType.Read, _device.RoInt, _device.RoShort, _device.RoLong);
            
            int[] requestedRegistersAddresses = message.Registers.Select(register => register.Address).ToArray();
            
            var sentResult = new RegisterRequestSentResult(isResponseRequired: true, requestedRegistersAddresses);

            // Act
            _session.ProcessResponse(message, sentResult);

            // Assert
            _updatedRegisterAddresses.Should().Equal(requestedRegistersAddresses);
        }
        
        [Fact]
        [Trait("Process Response", "Received registers register should be updated")]
        public void ProcessWriteResponseWithManyRegisters()
        {
            // Arrange
            IRegisterMessage message = CreateResponseMessageFromDeviceRegisters(OperationType.Write, _device.RwInt, _device.RwShort, _device.RwLong);
            
            int[] requestedRegistersAddresses = message.Registers.Select(register => register.Address).ToArray();
            
            var sentResult = new RegisterRequestSentResult(isResponseRequired: true, requestedRegistersAddresses);

            // Act
            _session.ProcessResponse(message, sentResult);

            // Assert
            _updatedRegisterAddresses.Should().Equal(requestedRegistersAddresses);
        }

        private IRegisterMessage CreateRequestMessage(OperationType operationType, params IRegister[] registers)
        {
            return new CommonMessage(_device.Address, MessageType.Request, operationType, registers);
        }
        
        private IRegisterMessage CreateResponseMessageFromDeviceRegisters(OperationType operationType, params IRegister[] deviceRegisters)
        {
            IRegister[] updateRegisters = deviceRegisters.Select(register => register.CreateCopy()).ToArray();
            return new CommonMessage(_device.Address, MessageType.Response, operationType, updateRegisters);
        }

        private void SetupIsResponseMayBeSkippedByProtocol(bool isResponseMayBeSkipped)
        {
            _protocolMock.SetupGet(proto => proto.IsResponseMayBeSkipped)
                         .Returns(isResponseMayBeSkipped);
        }
        
        private void SetupSentRegistersAddresses(IReadOnlyList<int> sentRegisters)
        {
            _protocolMock.Setup(proto => proto.SendAsPossible(It.IsAny<IRegisterMessage>(), out sentRegisters));
        }
    }
}