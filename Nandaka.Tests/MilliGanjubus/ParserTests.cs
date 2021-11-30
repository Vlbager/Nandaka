using System;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.Core.Util;
using Nandaka.MilliGanjubus.Components;
using Nandaka.Model.Registers;
using Xunit;
using Xunit.Sdk;
using ErrorMessage = Nandaka.Core.Session.ErrorMessage;

namespace Nandaka.Tests.MilliGanjubus
{
    public class ParserTests
    {
        private readonly IParser<byte[], MessageReceivedEventArgs> _parser;
        private int _messageCount;
        private IMessage? _parsedMessage;


        public ParserTests()
        {
            _parser = new MgApplicationParser();
            _parser.MessageParsed += Parser_MessageParsed;
        }

        private void Parser_MessageParsed(object? sender, MessageReceivedEventArgs e)
        {
            _messageCount++;
            _parsedMessage = e.ReceivedMessage;
        }

        [Theory]
        [Trait("ShouldParse", "Once")]
        // Single registerGroup to write request.
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0x01, 0x02, 0x01, 0x00 }, MessageType.Request, OperationType.Write, true)]
        [InlineData(new byte[] { 0xBB, 0x02, 0x08, 0x00, 0x03, 0x02, 0x01, 0x00 }, MessageType.Request, OperationType.Write, true)]
        // Multiple registers to write request.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x0A, 0x00, 0x03, 0x02, 0x01, 0x03, 0x04, 0x00 }, MessageType.Request, OperationType.Write, true)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x0C, 0x00, 0x03, 0x02, 0x01, 0x03, 0x04, 0x05, 0x06, 0x00 }, MessageType.Request, OperationType.Write, true)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x10, 0x00, 0x03, 0x02, 0x01, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x00 }, MessageType.Request, OperationType.Write, true)]
        // Single registerGroup to read response.
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0xA4, 0x02, 0x01, 0x00 }, MessageType.Response, OperationType.Read, true)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0xA6, 0x04, 0x02, 0x00 }, MessageType.Response, OperationType.Read, true)]
        // Multiple registers to read response.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x0A, 0x00, 0xA6, 0x02, 0x01, 0x03, 0x04, 0x00 }, MessageType.Response, OperationType.Read, true)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x0C, 0x00, 0xA6, 0x02, 0x01, 0x03, 0x04, 0x05, 0x06, 0x00 }, MessageType.Response, OperationType.Read, true)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x10, 0x00, 0xA6, 0x02, 0x01, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x00 }, MessageType.Response, OperationType.Read, true)]
        // Single registerGroup to write response.
        [InlineData(new byte[] { 0xBB, 0x01, 0x07, 0x00, 0xA1, 0x02, 0x00 }, MessageType.Response, OperationType.Write, false)]
        [InlineData(new byte[] { 0xBB, 0x02, 0x07, 0x00, 0xA3, 0x02, 0x00 }, MessageType.Response, OperationType.Write, false)]
        // Multiple registers to write response.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x08, 0x00, 0xA3, 0x02, 0x01, 0x00 }, MessageType.Response, OperationType.Write, false)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x0B, 0x00, 0xA3, 0x02, 0x01, 0x03, 0x04, 0x05, 0x00 }, MessageType.Response, OperationType.Write, false)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x10, 0x00, 0xA3, 0x02, 0x01, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x00 }, MessageType.Response, OperationType.Write, false)]
        // Single registerGroup to read request.
        [InlineData(new byte[] { 0xBB, 0x01, 0x07, 0x00, 0x04, 0x02, 0x00 }, MessageType.Request, OperationType.Read, false)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x07, 0x00, 0x06, 0x04, 0x00 }, MessageType.Request, OperationType.Read, false)]
        // Multiple registers to read request.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x08, 0x00, 0x06, 0x02, 0x01, 0x00 }, MessageType.Request, OperationType.Read, false)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x0B, 0x00, 0x06, 0x02, 0x01, 0x03, 0x04, 0x05, 0x00 }, MessageType.Request, OperationType.Read, false)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x10, 0x00, 0x06, 0x02, 0x01, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x00 }, MessageType.Request, OperationType.Read, false)]
        public void ParseSeries(byte[] buffer, MessageType messageType, OperationType operationType, bool withValues)
        {
            // Arrange
            // fill checkSums
            buffer[3] = CheckSum.Crc8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[^1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            _parser.Parse(buffer);
            // Asserts
            Assert.Equal(1, _messageCount);
            
            if (_parsedMessage is not IRegisterMessage registerMessage)
                throw new NotNullException();
            
            Assert.Equal(messageType, registerMessage.MessageType);
            
            Assert.Equal(registerMessage.OperationType, operationType);
            // Assert registers
            int byteIndex = 5;
            foreach (IRegister register in registerMessage.Registers)
            {
                Assert.Equal(buffer[byteIndex++], register.Address);
                if (!withValues)
                    continue;

                foreach (var byteInRegister in register.ToBytes())
                    Assert.Equal(buffer[byteIndex++], byteInRegister);
            }
            // Assert all data bytes added to message.
            Assert.Equal(byteIndex, buffer.Length - 1);
        }

        [Theory]
        [Trait("ShouldParse", "Once")]
        // Single registerGroup to write request.
        [InlineData(new byte[] { 0xBB, 0x01, 0x09, 0x00, 0x02, 0x01, 0x01, 0x02, 0x00 }, MessageType.Request, OperationType.Write, true)]
        // Multiple registers to write request.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x0A, 0x00, 0x02, 0x01, 0x02, 0x03, 0x04, 0x00 }, MessageType.Request, OperationType.Write, true)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x0C, 0x00, 0x02, 0x01, 0x04, 0x03, 0x04, 0x05, 0x06, 0x00 }, MessageType.Request, OperationType.Write, true)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x10, 0x00, 0x02, 0x01, 0x08, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x00 }, MessageType.Request, OperationType.Write, true)]
        // Single registerGroup to read response.
        [InlineData(new byte[] { 0xBB, 0x01, 0x09, 0x00, 0xA5, 0x01, 0x01, 0x02, 0x00 }, MessageType.Response, OperationType.Read, true)]
        // Multiple registers to read response.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x0A, 0x00, 0xA5, 0x01, 0x02, 0x03, 0x04, 0x00 }, MessageType.Response, OperationType.Read, true)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x0C, 0x00, 0xA5, 0x01, 0x04, 0x03, 0x04, 0x05, 0x06, 0x00 }, MessageType.Response, OperationType.Read, true)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x10, 0x00, 0xA5, 0x01, 0x08, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x00 }, MessageType.Response, OperationType.Read, true)]
        // Single registerGroup to write response.
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0xA2, 0x02, 0x02, 0x00 }, MessageType.Response, OperationType.Write, false)]
        // Multiple registers to write response.                                               
        [InlineData(new byte[] { 0xBB, 0xFF, 0x08, 0x00, 0xA2, 0x01, 0x02, 0x00 }, MessageType.Response, OperationType.Write, false)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x08, 0x00, 0xA2, 0x01, 0x04, 0x00 }, MessageType.Response, OperationType.Write, false)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0xA2, 0x01, 0x08, 0x00 }, MessageType.Response, OperationType.Write, false)]
        // Single registerGroup to read request.
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0x05, 0x02, 0x02, 0x00 }, MessageType.Request, OperationType.Read, false)]
        // Multiple registers to read request.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x08, 0x00, 0x05, 0x01, 0x02, 0x00 }, MessageType.Request, OperationType.Read, false)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x08, 0x00, 0x05, 0x01, 0x04, 0x00 }, MessageType.Request, OperationType.Read, false)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0x05, 0x01, 0x08, 0x00 }, MessageType.Request, OperationType.Read, false)]
        public void ParseRange(byte[] buffer, MessageType messageType, OperationType operationType, bool withValues)
        {
            // Arrange
            // fill checkSums
            buffer[3] = CheckSum.Crc8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[^1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            _parser.Parse(buffer);
            // Asserts
            Assert.Equal(1, _messageCount);
            
            if (_parsedMessage is not IRegisterMessage registerMessage)
                throw new NotNullException();
            
            Assert.Equal(messageType, registerMessage.MessageType);
            
            Assert.NotNull(registerMessage);
            Assert.Equal(registerMessage.OperationType, operationType);
            // Assert registers
            int byteIndex = 5;
            int currentAddress = buffer[byteIndex++];
            int lastAddress = buffer[byteIndex++];
            foreach (IRegister register in registerMessage.Registers)
            {
                Assert.Equal(currentAddress++, register.Address);
                if (!withValues)
                    continue;

                foreach (byte byteInRegister in register.ToBytes())
                    Assert.Equal(buffer[byteIndex++], byteInRegister);
            }

            // Assert all registers added to message.
            Assert.Equal(currentAddress, lastAddress + 1);
            // Assert all data bytes added to message.
            Assert.Equal(byteIndex, buffer.Length - 1);
        }

        [Theory]
        [Trait("ShouldParse", "Once")]
        [InlineData(new byte[] { 0xBB, 0x01, 0x07, 0x00, 0xE1, 0x01, 0x00 })]
        [InlineData(new byte[] { 0xBB, 0x02, 0x07, 0x00, 0xE2, 0x02, 0x00 })]
        [InlineData(new byte[] { 0xBB, 0xFF, 0x07, 0x00, 0xE3, 0x03, 0x00 })]
        [InlineData(new byte[] { 0xBB, 0x00, 0x07, 0x00, 0xE4, 0x04, 0x00 })]
        public void ParseErrorMessage(byte[] buffer)
        {
            // Arrange
            // fill checkSums
            buffer[3] = CheckSum.Crc8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[^1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            _parser.Parse(buffer);
            // Asserts
            Assert.Equal(1, _messageCount);

            if (_parsedMessage is not ErrorMessage errorMessage)
                throw new NotNullException();
            
            Assert.NotNull(errorMessage);
            Assert.Equal(_parsedMessage.SlaveDeviceAddress, buffer[1]);
            // Assert errorType
            Assert.NotNull(errorMessage);
            Assert.Equal(buffer[5], errorMessage.ProtocolSpecifiedErrorMessage!.ErrorCode);
        }

        [Fact]
        [Trait("ShouldParse", "Twice")]
        public void MultipleMessages()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x07, 0xB8, 0xA1, 0x01, 0x3A,
                0xBB, 0x01, 0x07, 0xB8, 0xA1, 0x01, 0x3A };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(2, _messageCount);
        }

        [Fact]
        [Trait("ShouldParse", "Twice")]
        public void MultipleMessagesWithGarbage()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x07, 0xB8, 0xA1, 0x01, 0x3A,
                0xAA, 0xBB, 0xEE, 0x67, 0x32,
                0xBB, 0x01, 0x07, 0xB8, 0xA1, 0x01, 0x3A };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(2, _messageCount);
        }

        [Fact]
        [Trait("ShouldParse", "Once")]
        public void WrongHeaderThenCorrectMessage()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x23, 0x00, 0xB8, 0x01, 0x01, 0xC5,
                0xBB, 0x01, 0x07, 0xB8, 0xA1, 0x01, 0x3A };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        [Trait("ShouldParse", "Once")]
        public void WrongChecksumThenCorrectMessage()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0x00,
                0xBB, 0x01, 0x07, 0xB8, 0xA1, 0x01, 0x3A };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        [Trait("ShouldParse", "Once")]
        public void PartOfValidMessageThenValidMessage()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x07, 0xB8, 0x01,
                0xBB, 0x01, 0x07, 0xB8, 0xA1, 0x01, 0x3A };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        [Trait("ShouldParse", "Once")]
        public void FragmentedMessage()
        {
            // Arrange
            var buffer1 = new byte[] { 0xBB };
            var buffer2 = new byte[] { 0x01, 0x07, };
            var buffer3 = new byte[] { 0xB8, 0xA1, 0x01 };
            var buffer4 = new byte[] { 0x3A };
            // Act
            _parser.Parse(buffer1);
            _parser.Parse(buffer2);
            _parser.Parse(buffer3);
            _parser.Parse(buffer4);
            // Assert
            Assert.Equal(1, _messageCount);
        }


        [Fact]
        [Trait("ShouldParse", "Once")]
        public void DoubleStartByte()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0xBB, 0x01, 0x07, 0xB8, 0xA1, 0x01, 0x3A };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        [Trait("ShouldParse", "Once")]
        public void DoubleHeader()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x07, 0xB8, 0xBB, 0x01, 0x07, 0xB8, 0xA1, 0x01, 0x3A };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        [Trait("ShouldNotParse", "AsApplicationDataError")]
        public void WrongDataAmount()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0xFF, 0x07, 0xCD, 0x01, 0x02, 0x96 };
            // Act
            Assert.Throws<Exception>(() =>_parser.Parse(buffer));
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        [Trait("ShouldNotParse", "AsApplicationDataError")]
        public void WrongGByte()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x88, 0x10, 0x8C, 0xBB, 0xCC, 0xBB, 0x04, 0x08, 0xE1, 0x00, 0xBB, 0x88, 0x08, 0x76, 0xED };
            // Act
            Assert.Throws<InvalidMetaDataReceivedException>(() => _parser.Parse(buffer));
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Theory]
        [Trait("ShouldNotParse", "AsApplicationDataError")]
        [InlineData(new byte[] { 0xBB, 0x88, 0x10, 0x00, 0xAA, 0xCC, 0xBB, 0x04, 0x08, 0xE1, 0x00, 0xBB, 0x88, 0x08, 0x76, 0x00 })]
        [InlineData(new byte[] { 0xBB, 0x88, 0x10, 0x00, 0x0A, 0xCC, 0xBB, 0x04, 0x08, 0xE1, 0x00, 0xBB, 0x88, 0x08, 0x76, 0x00 })]
        public void WrongFNibble(byte[] buffer)
        {
            // fill checkSums
            buffer[3] = CheckSum.Crc8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[^1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            Assert.Throws<InvalidMetaDataReceivedException>(() => _parser.Parse(buffer));
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        [Trait("ShouldNotParse", "AsApplicationDataError")]
        public void StartAddressIsGreaterThanEndAddress()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x00, 0x0C, 0x00, 0xA5, 0x04, 0x01, 0x03, 0x04, 0x05, 0x06, 0x00 };
            // fill checkSums
            buffer[3] = CheckSum.Crc8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[^1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            Assert.Throws<InvalidRegistersReceivedException>(() => _parser.Parse(buffer));
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        [Trait("ShouldNotParse", "AsApplicationDataError")]
        public void TooManyRegistersInRangeMessage()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x00, 0x0C, 0x00, 0xA5, 0x01, 0x07, 0x03, 0x04, 0x05, 0x06, 0x00 };
            // fill checkSums
            buffer[3] = CheckSum.Crc8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[^1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            Assert.Throws<TooMuchDataRequestedException>(() => _parser.Parse(buffer));
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        [Trait("ShouldNotParse", "")]
        public void ZeroPacket()
        {
            // Arrange
            var buffer = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(0, _messageCount);
        }

        [Fact]
        [Trait("ShouldNotParse", "")]
        public void WrongHeaderChecksum()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x07, 0x00, 0x01, 0x01, 0xC5 };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(0, _messageCount);
        }

        [Fact]
        [Trait("ShouldNotParse", "")]
        public void WrongMessageChecksum()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0x00 };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(0, _messageCount);
        }

        [Fact]
        [Trait("ShouldNotParse", "")]
        public void PacketTooLong()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x11, 0x00, 0xA5, 0x01, 0x08, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x11, 0x00 };
            // fill checkSums
            buffer[3] = CheckSum.Crc8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[^1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(0, _messageCount);
        }
    }
}
