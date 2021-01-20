using System;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Util;
using Nandaka.MilliGanjubus.Components;
using Xunit;

namespace Nandaka.Tests.MilliGanjubus
{
    public class ComposerTests
    {
        private const int TestDeviceAddress = 0x01;
        
        private readonly IComposer<IRegisterMessage, byte[]> _composer;
        private readonly RegisterMessageFactory _messageFactory;

        public ComposerTests()
        {
            _composer = new MgComposer();
            _messageFactory = new RegisterMessageFactory(TestDeviceAddress);
        }

        [Theory]
        // Single registerGroup to write request.
        [InlineData(2, 1, MessageType.Request, OperationType.Write , new byte[] { 0xBB, 0x01, 0x08, 0x00, 0x03, 0x02, 0x00, 0x00 })]
        // Multiple registers to write request.
        [InlineData(1, 2, MessageType.Request, OperationType.Write, new byte[] { 0xBB, 0x01, 0x0A, 0x00, 0x03, 0x01, 0x00, 0x03, 0x00, 0x00 })]
        [InlineData(1, 3, MessageType.Request, OperationType.Write, new byte[] { 0xBB, 0x01, 0x0C, 0x00, 0x03, 0x01, 0x00, 0x03, 0x00, 0x05, 0x00, 0x00 })]
        [InlineData(1, 5, MessageType.Request, OperationType.Write, new byte[] { 0xBB, 0x01, 0x10, 0x00, 0x03, 0x01, 0x00, 0x03, 0x00, 0x05, 0x00, 0x07, 0x00, 0x09, 0x00, 0x00 })]
        // Single registerGroup to read response.
        [InlineData(4, 1, MessageType.Response, OperationType.Read, new byte[] { 0xBB, 0x01, 0x08, 0x00, 0xA6, 0x04, 0x00, 0x00 })]
        // Multiple registers to read response.
        [InlineData(1, 2, MessageType.Response, OperationType.Read, new byte[] { 0xBB, 0x01, 0x0A, 0x00, 0xA6, 0x01, 0x00, 0x03, 0x00, 0x00 })]
        [InlineData(1, 3, MessageType.Response, OperationType.Read, new byte[] { 0xBB, 0x01, 0x0C, 0x00, 0xA6, 0x01, 0x00, 0x03, 0x00, 0x05, 0x00, 0x00 })]
        [InlineData(1, 5, MessageType.Response, OperationType.Read, new byte[] { 0xBB, 0x01, 0x10, 0x00, 0xA6, 0x01, 0x00, 0x03, 0x00, 0x05, 0x00, 0x07, 0x00, 0x09, 0x00, 0x00 })]
        // Single registerGroup to write response.
        [InlineData(2, 1, MessageType.Response, OperationType.Write, new byte[] { 0xBB, 0x01, 0x07, 0x00, 0xA3, 0x02, 0x00 })]
        // Multiple registers to write response.
        [InlineData(1, 2, MessageType.Response, OperationType.Write, new byte[] { 0xBB, 0x01, 0x08, 0x00, 0xA3, 0x01, 0x03, 0x00 })]
        [InlineData(1, 3, MessageType.Response, OperationType.Write, new byte[] { 0xBB, 0x01, 0x09, 0x00, 0xA3, 0x01, 0x03, 0x05, 0x00 })]
        [InlineData(1, 5, MessageType.Response, OperationType.Write, new byte[] { 0xBB, 0x01, 0x0B, 0x00, 0xA3, 0x01, 0x03, 0x05, 0x07, 0x09, 0x00 })]
        // Single registerGroup to read request.
        [InlineData(4, 1, MessageType.Request, OperationType.Read, new byte[] { 0xBB, 0x01, 0x07, 0x00, 0x06, 0x04, 0x00 })]
        // Multiple registers to read request.
        [InlineData(1, 2, MessageType.Request, OperationType.Read, new byte[] { 0xBB, 0x01, 0x08, 0x00, 0x06, 0x01, 0x03, 0x00 })]
        [InlineData(1, 3, MessageType.Request, OperationType.Read, new byte[] { 0xBB, 0x01, 0x09, 0x00, 0x06, 0x01, 0x03, 0x05, 0x00 })]
        [InlineData(1, 5, MessageType.Request, OperationType.Read, new byte[] { 0xBB, 0x01, 0x0B, 0x00, 0x06, 0x01, 0x03, 0x05, 0x07, 0x09, 0x00 })]
        [Trait("ShouldCompose", "AllMessage")]
        public void ComposeSeries(int firstRegisterAddress, int registerCount, MessageType messageType, OperationType operationType, byte[] expected)
        {
            // Arrange
            // fill checkSums
            expected[3] = CheckSum.Crc8(expected.AsSpan().Slice(0, 3).ToArray());
            expected[^1] = CheckSum.Crc8(expected.AsSpan().Slice(0, expected.Length - 1).ToArray());

            IRegisterMessage message = _messageFactory.CreateSeries(firstRegisterAddress, registerCount, operationType, messageType);
            
            // Act
            byte[] actual = _composer.Compose(message, out _);
            
            // Assert
            Assert.Equal(expected, actual);
        }
        
        [Theory]
        
        [InlineData(1, 3, MessageType.Request, OperationType.Write, new byte[] { 0xBB, 0x01, 0x0B, 0x00, 0x02, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00 })]
        [InlineData(1, 5, MessageType.Request, OperationType.Write, new byte[] { 0xBB, 0x01, 0x0D, 0x00, 0x02, 0x01, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]

        [InlineData(1, 3, MessageType.Response, OperationType.Read, new byte[] { 0xBB, 0x01, 0x0B, 0x00, 0xA5, 0x01, 0x03, 0x00, 0x00, 0x00, 0x00 })]
        [InlineData(1, 5, MessageType.Response, OperationType.Read, new byte[] { 0xBB, 0x01, 0x0D, 0x00, 0xA5, 0x01, 0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 })]

        [InlineData(1, 3, MessageType.Response, OperationType.Write, new byte[] { 0xBB, 0x01, 0x08, 0x00, 0xA2, 0x01, 0x03, 0x00 })]
        [InlineData(1, 5, MessageType.Response, OperationType.Write, new byte[] { 0xBB, 0x01, 0x08, 0x00, 0xA2, 0x01, 0x05, 0x00 })]

        [InlineData(1, 3, MessageType.Request, OperationType.Read, new byte[] { 0xBB, 0x01, 0x08, 0x00, 0x05, 0x01, 0x03, 0x00 })]
        [InlineData(1, 5, MessageType.Request, OperationType.Read, new byte[] { 0xBB, 0x01, 0x08, 0x00, 0x05, 0x01, 0x05, 0x00 })]
        
        [Trait("ShouldCompose", "AllMessage")]
        public void ComposeRange(int firstRegisterAddress, int registerCount, MessageType messageType, OperationType operationType, byte[] expected)
        {
            // Arrange
            // fill checkSums
            expected[3] = CheckSum.Crc8(expected.AsSpan().Slice(0, 3).ToArray());
            expected[^1] = CheckSum.Crc8(expected.AsSpan().Slice(0, expected.Length - 1).ToArray());

            IRegisterMessage message = _messageFactory.CreateRange(firstRegisterAddress, registerCount, operationType, messageType);
            
            // Act
            byte[] actual = _composer.Compose(message, out _);
            
            // Assert
            Assert.Equal(expected, actual);
        }
    }
}