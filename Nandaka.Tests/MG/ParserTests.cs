using System;
using Xunit;
using Nandaka.MilliGanjubus;


namespace Nandaka.Tests.MG
{
    public class ParserTests
    {
        private readonly IParser<byte[], IMessage> _parser;
        private int _messageCount;
        private IMessage _parsedMessage;


        public ParserTests()
        {
            _parser = new MilliGanjubusApplicationParser();
            _parser.MessageParsed += parser_MessageParsed;
        }

        private void parser_MessageParsed(object sender, IMessage e)
        {
            _messageCount++;
            _parsedMessage = e;
        }

        [Theory]
        [Trait("ShouldParse", "Once")]
        // Single register to write request.
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0x01, 0x02, 0x01, 0x00 }, MessageType.WriteDataRequest, true)]
        [InlineData(new byte[] { 0xBB, 0x02, 0x08, 0x00, 0x03, 0x02, 0x01, 0x00 }, MessageType.WriteDataRequest, true)]
        // Multiple registers to write request.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x0A, 0x00, 0x03, 0x02, 0x01, 0x03, 0x04, 0x00 }, MessageType.WriteDataRequest, true)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x0C, 0x00, 0x03, 0x02, 0x01, 0x03, 0x04, 0x05, 0x06, 0x00 }, MessageType.WriteDataRequest, true)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x10, 0x00, 0x03, 0x02, 0x01, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x00 }, MessageType.WriteDataRequest, true)]
        // Single register to read response.
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0xA4, 0x02, 0x01, 0x00 }, MessageType.ReadDataResponse, true)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0xA6, 0x04, 0x02, 0x00 }, MessageType.ReadDataResponse, true)]
        // Multiple registers to read response.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x0A, 0x00, 0xA6, 0x02, 0x01, 0x03, 0x04, 0x00 }, MessageType.ReadDataResponse, true)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x0C, 0x00, 0xA6, 0x02, 0x01, 0x03, 0x04, 0x05, 0x06, 0x00 }, MessageType.ReadDataResponse, true)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x10, 0x00, 0xA6, 0x02, 0x01, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x00 }, MessageType.ReadDataResponse, true)]
        // Single register to write response.
        [InlineData(new byte[] { 0xBB, 0x01, 0x07, 0x00, 0xA1, 0x02, 0x00 }, MessageType.WriteDataResponse, false)]
        [InlineData(new byte[] { 0xBB, 0x02, 0x07, 0x00, 0xA3, 0x02, 0x00 }, MessageType.WriteDataResponse, false)]
        // Multiple registers to write response.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x08, 0x00, 0xA3, 0x02, 0x01, 0x00 }, MessageType.WriteDataResponse, false)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x0B, 0x00, 0xA3, 0x02, 0x01, 0x03, 0x04, 0x05, 0x00 }, MessageType.WriteDataResponse, false)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x10, 0x00, 0xA3, 0x02, 0x01, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x00 }, MessageType.WriteDataResponse, false)]
        // Single register to read request.
        [InlineData(new byte[] { 0xBB, 0x01, 0x07, 0x00, 0x04, 0x02, 0x00 }, MessageType.ReadDataRequest, false)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x07, 0x00, 0x06, 0x04, 0x00 }, MessageType.ReadDataRequest, false)]
        // Multiple registers to read request.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x08, 0x00, 0x06, 0x02, 0x01, 0x00 }, MessageType.ReadDataRequest, false)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x0B, 0x00, 0x06, 0x02, 0x01, 0x03, 0x04, 0x05, 0x00 }, MessageType.ReadDataRequest, false)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x10, 0x00, 0x06, 0x02, 0x01, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x00 }, MessageType.ReadDataRequest, false)]
        public void ParseSeries(byte[] buffer, MessageType messageType, bool withValues)
        {
            // Arrange
            // fill checkSums
            buffer[3] = CheckSum.Crc8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[buffer.Length - 1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            _parser.AwaitingReplyAddress = buffer[1];
            _parser.Parse(buffer);
            // Asserts
            Assert.Equal(1, _messageCount);
            Assert.Equal(messageType, _parsedMessage.MessageType);
            Assert.Equal(_parsedMessage.DeviceAddress, _parser.AwaitingReplyAddress);
            // Assert registers
            int byteIndex = 5;
            foreach (var register in _parsedMessage.Registers)
            {
                Assert.Equal(buffer[byteIndex++], register.Address);
                if (!withValues)
                {
                    continue;
                }
                foreach (var byteInRegister in register.GetBytes())
                {
                    Assert.Equal(buffer[byteIndex++], byteInRegister);
                }
            }
            // Assert all data bytes added to message.
            Assert.Equal(byteIndex, buffer.Length - 1);
        }

        [Theory]
        [Trait("ShouldParse", "Once")]
        // Single register to write request.
        [InlineData(new byte[] { 0xBB, 0x01, 0x09, 0x00, 0x02, 0x01, 0x01, 0x02, 0x00 }, MessageType.WriteDataRequest, true)]
        // Multiple registers to write request.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x0A, 0x00, 0x02, 0x01, 0x02, 0x03, 0x04, 0x00 }, MessageType.WriteDataRequest, true)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x0C, 0x00, 0x02, 0x01, 0x04, 0x03, 0x04, 0x05, 0x06, 0x00 }, MessageType.WriteDataRequest, true)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x10, 0x00, 0x02, 0x01, 0x08, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x00 }, MessageType.WriteDataRequest, true)]
        // Single register to read response.
        [InlineData(new byte[] { 0xBB, 0x01, 0x09, 0x00, 0xA5, 0x01, 0x01, 0x02, 0x00 }, MessageType.ReadDataResponse, true)]
        // Multiple registers to read response.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x0A, 0x00, 0xA5, 0x01, 0x02, 0x03, 0x04, 0x00 }, MessageType.ReadDataResponse, true)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x0C, 0x00, 0xA5, 0x01, 0x04, 0x03, 0x04, 0x05, 0x06, 0x00 }, MessageType.ReadDataResponse, true)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x10, 0x00, 0xA5, 0x01, 0x08, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x10, 0x00 }, MessageType.ReadDataResponse, true)]
        // Single register to write response.
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0xA2, 0x02, 0x02, 0x00 }, MessageType.WriteDataResponse, false)]
        // Multiple registers to write response.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x08, 0x00, 0xA2, 0x01, 0x02, 0x00 }, MessageType.WriteDataResponse, false)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x08, 0x00, 0xA2, 0x01, 0x04, 0x00 }, MessageType.WriteDataResponse, false)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0xA2, 0x01, 0x08, 0x00 }, MessageType.WriteDataResponse, false)]
        // Single register to read request.
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0x05, 0x02, 0x02, 0x00 }, MessageType.ReadDataRequest, false)]
        // Multiple registers to read request.
        [InlineData(new byte[] { 0xBB, 0xFF, 0x08, 0x00, 0x05, 0x01, 0x02, 0x00 }, MessageType.ReadDataRequest, false)]
        [InlineData(new byte[] { 0xBB, 0x00, 0x08, 0x00, 0x05, 0x01, 0x04, 0x00 }, MessageType.ReadDataRequest, false)]
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0x05, 0x01, 0x08, 0x00 }, MessageType.ReadDataRequest, false)]
        public void ParseRange(byte[] buffer, MessageType messageType, bool withValues)
        {
            // Arrange
            // fill checkSums
            buffer[3] = CheckSum.Crc8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[buffer.Length - 1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            _parser.AwaitingReplyAddress = buffer[1];
            _parser.Parse(buffer);
            // Asserts
            Assert.Equal(1, _messageCount);
            Assert.Equal(messageType, _parsedMessage.MessageType);
            Assert.Equal(_parsedMessage.DeviceAddress, _parser.AwaitingReplyAddress);
            // Assert registers
            int byteIndex = 5;
            int currentAddress = buffer[byteIndex++];
            int lastAddress = buffer[byteIndex++];
            foreach (var register in _parsedMessage.Registers)
            {
                Assert.Equal(currentAddress++, register.Address);
                if (!withValues)
                {
                    continue;
                }
                foreach (var byteInRegister in register.GetBytes())
                {
                    Assert.Equal(buffer[byteIndex++], byteInRegister);
                }
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
            buffer[buffer.Length - 1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            _parser.AwaitingReplyAddress = buffer[1];
            _parser.Parse(buffer);
            // Asserts
            Assert.Equal(1, _messageCount);
            Assert.Equal(MessageType.ErrorMessage, _parsedMessage.MessageType);
            Assert.Equal(_parsedMessage.DeviceAddress, buffer[1]);
            // Assert errorType
            var milliGanjubusMessage = _parsedMessage as MilliGanjubusMessage;
            Assert.NotNull(milliGanjubusMessage);
            Assert.Equal(buffer[5], milliGanjubusMessage.ErrorCode);
        }

        [Fact]
        [Trait("ShouldParse", "Twice")]
        public void MultipleMessages()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0xC5,
                0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0xC5 };
            // Act
            _parser.AwaitingReplyAddress = 0x01;
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(2, _messageCount);
        }

        [Fact]
        [Trait("ShouldParse", "Twice")]
        public void MultipleMessagesWithGarbage()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0xC5,
                0xAA, 0xBB, 0xEE, 0x67, 0x32,
                0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0xC5 };
            // Act
            _parser.AwaitingReplyAddress = 0x01;
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
                0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0xC5 };
            // Act
            _parser.AwaitingReplyAddress = 0x01;
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
                0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0xC5 };
            // Act
            _parser.AwaitingReplyAddress = 0x01;
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
                0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0xC5 };
            // Act
            _parser.AwaitingReplyAddress = 0x01;
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
            var buffer3 = new byte[] { 0xB8, 0x01, 0x01 };
            var buffer4 = new byte[] { 0xC5 };
            // Act
            _parser.AwaitingReplyAddress = 0x01;
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
            var buffer = new byte[] { 0xBB, 0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0xC5 };
            // Act
            _parser.AwaitingReplyAddress = 0x01;
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        [Trait("ShouldParse", "Once")]
        public void DoubleHeader()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x07, 0xB8, 0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0xC5 };
            // Act
            _parser.AwaitingReplyAddress = 0x01;
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        [Trait("ShouldParse", "AsApplicationDataError")]
        public void WrongDataAmount()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0xFF, 0x07, 0xCD, 0x01, 0x02, 0x96 };
            // Act
            _parser.AwaitingReplyAddress = buffer[1];
            _parser.Parse(buffer);
            var milliGanjubusMessage = _parsedMessage as MilliGanjubusMessage;
            // Assert
            Assert.Equal(1, _messageCount);
            Assert.Empty(_parsedMessage.Registers);
            Assert.NotNull(milliGanjubusMessage);
            Assert.Equal((int)MilliGanjubusErrorType.WrongDataAmount, milliGanjubusMessage.ErrorCode);
        }

        [Fact]
        [Trait("ShouldParse", "AsApplicationDataError")]
        public void WrongGByte()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x88, 0x10, 0x8C, 0xBB, 0xCC, 0xBB, 0x04, 0x08, 0xE1, 0x00, 0xBB, 0x88, 0x08, 0x76, 0xED };
            // Act
            _parser.AwaitingReplyAddress = buffer[1];
            _parser.Parse(buffer);
            var milliGanjubusMessage = _parsedMessage as MilliGanjubusMessage;
            // Assert
            Assert.Equal(1, _messageCount);
            Assert.Empty(_parsedMessage.Registers);
            Assert.NotNull(milliGanjubusMessage);
            Assert.Equal((int)MilliGanjubusErrorType.WrongGByte, milliGanjubusMessage.ErrorCode);
        }

        [Theory]
        [Trait("ShouldParse", "AsApplicationDataError")]
        [InlineData(new byte[] { 0xBB, 0x88, 0x10, 0x00, 0xAA, 0xCC, 0xBB, 0x04, 0x08, 0xE1, 0x00, 0xBB, 0x88, 0x08, 0x76, 0x00 })]
        [InlineData(new byte[] { 0xBB, 0x88, 0x10, 0x00, 0x0A, 0xCC, 0xBB, 0x04, 0x08, 0xE1, 0x00, 0xBB, 0x88, 0x08, 0x76, 0x00 })]
        public void WrongFNibble(byte[] buffer)
        {
            // fill checkSums
            buffer[3] = CheckSum.Crc8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[buffer.Length - 1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            _parser.AwaitingReplyAddress = buffer[1];
            _parser.Parse(buffer);
            var milliGanjubusMessage = _parsedMessage as MilliGanjubusMessage;
            // Assert
            Assert.Equal(1, _messageCount);
            Assert.Empty(_parsedMessage.Registers);
            Assert.NotNull(milliGanjubusMessage);
            Assert.Equal((int)MilliGanjubusErrorType.WrongGByte, milliGanjubusMessage.ErrorCode);
        }

        [Fact]
        [Trait("ShouldParse", "AsApplicationDataError")]
        public void StartAddressIsGreaterThanEndAddress()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x00, 0x0C, 0x00, 0xA5, 0x04, 0x01, 0x03, 0x04, 0x05, 0x06, 0x00 };
            // fill checkSums
            buffer[3] = CheckSum.Crc8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[buffer.Length - 1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            _parser.AwaitingReplyAddress = buffer[1];
            _parser.Parse(buffer);
            var milliGanjubusMessage = _parsedMessage as MilliGanjubusMessage;
            // Assert
            Assert.Equal(1, _messageCount);
            Assert.Empty(_parsedMessage.Registers);
            Assert.NotNull(milliGanjubusMessage);
            Assert.Equal((int)MilliGanjubusErrorType.WrongRegisterAddress, milliGanjubusMessage.ErrorCode);
        }

        [Fact]
        [Trait("ShouldParse", "AsApplicationDataError")]
        public void TooManyRegistersInRangeMessage()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x00, 0x0C, 0x00, 0xA5, 0x01, 0x07, 0x03, 0x04, 0x05, 0x06, 0x00 };
            // fill checkSums
            buffer[3] = CheckSum.Crc8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[buffer.Length - 1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            _parser.AwaitingReplyAddress = buffer[1];
            _parser.Parse(buffer);
            var milliGanjubusMessage = _parsedMessage as MilliGanjubusMessage;
            // Assert
            Assert.Equal(1, _messageCount);
            Assert.Empty(_parsedMessage.Registers);
            Assert.NotNull(milliGanjubusMessage);
            Assert.Equal((int)MilliGanjubusErrorType.WrongDataAmount, milliGanjubusMessage.ErrorCode);
        }

        [Fact]
        [Trait("ShouldNotParse", "")]
        public void ZeroPacket()
        {
            // Arrange
            var buffer = new byte[] { 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, };
            // Act
            _parser.AwaitingReplyAddress = 0x00;
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
            _parser.AwaitingReplyAddress = buffer[1];
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
            _parser.AwaitingReplyAddress = buffer[1];
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
            buffer[buffer.Length - 1] = CheckSum.Crc8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            _parser.AwaitingReplyAddress = 0x01;
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(0, _messageCount);
        }
    }
}
