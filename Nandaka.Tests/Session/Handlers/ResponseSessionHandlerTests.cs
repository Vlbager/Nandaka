using System;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.Tests.Util;
using Xunit;

namespace Nandaka.Tests.Session
{
    public sealed class ResponseSessionHandlerTests
    {
        private static readonly IRegisterMessage DefaultRequest = new CommonMessage(TestDevice.TestDeviceAddress, MessageType.Request, OperationType.None); 
        
        private readonly Mock<IProtocol> _protocolMock;
        private readonly Mock<IResponseSession<IMessage>> _sessionMock;
        private readonly ResponseSessionHandler<IMessage> _handler;

        private int _processedResponsesCount;

        public ResponseSessionHandlerTests()
        {
            _protocolMock = new Mock<IProtocol>();
            _sessionMock = new Mock<IResponseSession<IMessage>>();
            var device = new TestDevice();
            _handler = new ResponseSessionHandler<IMessage>(_sessionMock.Object, _protocolMock.Object, device, NullLogger.Instance);
            
            SetupMocksAsDefault();
        }

        [Fact]
        [Trait("Handle request", "Single message should be processed")]
        public void HandleSingleMessage()
        {
            // Arrange
            RaiseMessageReceivedEvent(new MessageReceivedEventArgs(DefaultRequest));
            
            // Act
            _handler.ProcessNext();
            
            // Assert
            _processedResponsesCount.Should().Be(1);
        }
        
        [Fact]
        [Trait("Handle request", "Single message should be processed")]
        public void HandleMoreThanOneMessage()
        {
            // Arrange
            const int messagesCount = 3;
            for (int i = 0; i < messagesCount; i++)
                RaiseMessageReceivedEvent(new MessageReceivedEventArgs(DefaultRequest));
            
            // Act & Assert
            for (int i = 0; i < messagesCount; i++)
                _handler.ExecutionTimeOf(handler => handler.ProcessNext())
                        .Should().BeLessThan(5.Seconds());
            
            // Assert
            _processedResponsesCount.Should().Be(messagesCount);
        }
        
        [Fact]
        [Trait("Handle request", "Protocol exception should be rethrown to session handler")]
        public void HandleMessageWithProtocolLevelException()
        {
            // Arrange
            RaiseMessageReceivedEvent(new MessageReceivedEventArgs(new InvalidOperationException()));
            
            // Act & Assert
            _handler.Invoking(handler => handler.ProcessNext())
                    .Should().Throw<InvalidOperationException>();
        }

        private void RaiseMessageReceivedEvent(MessageReceivedEventArgs eventArgs)
        {
            _protocolMock.Raise(proto => proto.MessageReceived += null, eventArgs);
        }

        private void SetupMocksAsDefault()
        {
            _sessionMock.Setup(session => session.ProcessRequest(It.IsAny<IMessage>()))
                        .Callback<IMessage>(_ => _processedResponsesCount++);
        }
    }
}