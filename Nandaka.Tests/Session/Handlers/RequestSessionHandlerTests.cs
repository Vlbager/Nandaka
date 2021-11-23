using System;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Tests.Util;
using Xunit;

namespace Nandaka.Tests.Session
{

    public sealed class RequestSessionHandlerTests
    {
        private static readonly IRegisterMessage DefaultResponse = new CommonMessage(TestDevice.TestDeviceAddress, MessageType.Response, OperationType.None); 

        private readonly Mock<IProtocol> _protocolMock;
        private readonly Mock<IRequestSession<IMessage, DefaultSentResult>> _sessionMock;
        private readonly Mock<IErrorMessageHandler> _errorMessageHandlerMock;
        private readonly RequestSessionHandler<IMessage, DefaultSentResult> _handler;

        private int _sentMessagesCount;
        private int _processedResponsesCount;
        private int _errorResponsesCount;
        
        public RequestSessionHandlerTests()
        {
            _protocolMock = new Mock<IProtocol>();
            _sessionMock = new Mock<IRequestSession<IMessage, DefaultSentResult>>();
            _errorMessageHandlerMock = new Mock<IErrorMessageHandler>();
            var device = new TestDevice();
            _handler = new RequestSessionHandler<IMessage, DefaultSentResult>(_sessionMock.Object, _protocolMock.Object, device,
                                                                              TimeSpan.FromSeconds(1), _errorMessageHandlerMock.Object, 
                                                                              NullLogger.Instance);
            SetupMocksAsDefault();
        }
        
        [Fact]
        [Trait("Do not send message", "Handling empty message should not increment sent messages counter")]
        public void HandleEmptyMessage()
        {
            // Arrange
            _sessionMock.Setup(session => session.GetNextMessage())
                        .Returns(EmptyMessage.Create());

            // Act
            _handler.ProcessNext();

            // Assert
            _sentMessagesCount.Should().Be(0, "empty message can't be sent");
        }
        
        [Fact]
        [Trait("Send request", "Handling register request without response should not increment responses counter, but sent messages counter")]
        public void HandleRegisterMessageWithoutResponse()
        {
            // Arrange
            SetupSessionMockForRaiseMessageReceivedEvent(new MessageReceivedEventArgs(DefaultResponse), isResponseRequired: false);

            // Act
            _handler.ProcessNext();
            
            // Assert
            _sentMessagesCount.Should().Be(1);
            _processedResponsesCount.Should().Be(0, "response isn't required");
        }
        
        [Fact]
        [Trait("Send request", "Handling single register request with response should once increment responses count")]
        public void HandleSingleRegisterMessageWithResponse()
        {
            // Arrange
            SetupSessionMockForRaiseMessageReceivedEvent(new MessageReceivedEventArgs(DefaultResponse), isResponseRequired: true);
            
            // Act
            _handler.ProcessNext();
            
            // Assert
            _sentMessagesCount.Should().Be(1);
            _processedResponsesCount.Should().Be(1);
        }
        
        [Fact]
        [Trait("Send request", "Event was raised before handler starts process next message. It means that raised message should not be received")]
        public void RaiseMessageReceivedEventThenHandleMessage()
        {
            // Arrange
            RaiseMessageReceivedEvent(new MessageReceivedEventArgs(DefaultResponse));
            
            // Act && Assert
            _handler.Invoking(handler => handler.ProcessNext())
                    .Should().Throw<DeviceNotRespondException>("message was received before handler starts listening");
        }
        
        [Fact]
        [Trait("Send request", "Handling register request with error response should increment error messages count")]
        public void HandleRegisterMessageWithErrorResponse()
        {
            // Arrange
            var errorResponse = ErrorMessage.CreateCommon(TestDevice.TestDeviceAddress, MessageType.Response, ErrorType.InvalidMetaData);
            
            SetupSessionMockForRaiseMessageReceivedEvent(new MessageReceivedEventArgs(errorResponse), isResponseRequired: true);

            // Act
            _handler.ProcessNext();
            
            // Assert
            _sentMessagesCount.Should().Be(1);
            _errorResponsesCount.Should().Be(1);
        }

        [Fact]
        [Trait("Send request", "Handling register request with not responding device should throw exception")]
        public void HandleRegisterMessageWithNotRespondingDevice()
        {
            // Act & Assert
            _handler.Invoking(handler => handler.ProcessNext())
                    .Should().Throw<DeviceNotRespondException>();
        }
        
        [Fact]
        [Trait("Send request", "Protocol exception should be rethrown to session handler")]
        public void HandleMessageWithProtocolLevelException()
        {
            // Arrange
            SetupSessionMockForRaiseMessageReceivedEvent(new MessageReceivedEventArgs(new InvalidOperationException()), isResponseRequired: true);
            
            // Act & Assert
            _handler.Invoking(handler => handler.ProcessNext())
                    .Should().Throw<InvalidOperationException>();
        }

        private void RaiseMessageReceivedEvent(MessageReceivedEventArgs eventArgs)
        {
            _protocolMock.Raise(proto => proto.MessageReceived += null, eventArgs);
        }

        private void SetupSessionMockForRaiseMessageReceivedEvent(MessageReceivedEventArgs eventArgs, bool isResponseRequired)
        {
            _sessionMock.Setup(session => session.SendRequest(It.IsAny<IMessage>()))
                        .Callback<IMessage>(_ =>
                        {
                            RaiseMessageReceivedEvent(eventArgs);
                            _sentMessagesCount++;
                        })
                        .Returns(new DefaultSentResult(isResponseRequired));
        }
        
        private void SetupMocksAsDefault()
        {
            _sessionMock.Setup(session => session.GetNextMessage())
                        .Returns(It.IsAny<IMessage>());
            
            _sessionMock.Setup(session => session.SendRequest(It.IsAny<IMessage>()))
                        .Callback<IMessage>(_ => _sentMessagesCount++)
                        .Returns(new DefaultSentResult(true));

            _sessionMock.Setup(session => session.ProcessResponse(It.IsAny<IMessage>(), It.IsAny<DefaultSentResult>()))
                        .Callback<IMessage, DefaultSentResult>((_, _) => _processedResponsesCount++);

            _errorMessageHandlerMock.Setup(handler => handler.OnErrorReceived(It.IsAny<ErrorMessage>(), It.IsAny<ILogger>()))
                                    .Callback<ErrorMessage, ILogger>((_, _) => _errorResponsesCount++);
        }
    }
}