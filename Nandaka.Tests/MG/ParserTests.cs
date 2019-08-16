using System;
using Xunit;
using Nandaka;
using Nandaka.MilliGanjubus;


namespace Nandaka.MG.Tests
{
    public class ParserTests
    {
        private IParser<byte[]> _parser;
        private int _messageCount;
        private IProtocolMessage _parsedMessage;


        public ParserTests()
        {
            _parser = new MilliGanjubusParser();
            _parser.MessageParsed += parser_MessageParsed;
        }

        private void parser_MessageParsed(object sender, IProtocolMessage e)
        {
            _messageCount++;
            _parsedMessage = e;
        }

        [Theory]
        [InlineData(new byte[] { 0xBB, 0x01, 0x07, 0x00, 0x01, 0x01, 0x00 })]
        [InlineData(new byte[] { 0xBB, 0x02, 0x07, 0x00, 0xA1, 0x01, 0x00 })]
        [InlineData(new byte[] { 0xBB, 0xFF, 0x07, 0x00, 0x01, 0x02, 0x00 })]
        [InlineData(new byte[] { 0xBB, 0x01, 0x08, 0x00, 0x03, 0x02, 0x01, 0x00 })]
        [InlineData(new byte[] { 0xBB, 0xFF, 0x08, 0x00, 0x03, 0x02, 0x01, 0x00 })]
        [InlineData(new byte[] { 0xBB, 0x00, 0x08, 0x00, 0x03, 0x02, 0x01, 0x00 })]
        public void MessageParsing(byte[] buffer)
        {
            // Arrange
            // fill checksums
            buffer[3] = CheckSum.CRC8(buffer.AsSpan().Slice(0, 3).ToArray());
            buffer[buffer.Length - 1] = CheckSum.CRC8(buffer.AsSpan().Slice(0, buffer.Length - 1).ToArray());
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        public void DoubleStartByte()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0xC5 };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(1, _messageCount);
        }

        [Fact]
        public void DoubleHeader()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x07, 0xB8, 0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0xC5 };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(1, _messageCount);
        }


        [Fact]
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
        public void WrongMessageChecksum()
        {
            // Arrange
            var buffer = new byte[] { 0xBB, 0x01, 0x07, 0xB8, 0x01, 0x01, 0x00 };
            // Act
            _parser.Parse(buffer);
            // Assert
            Assert.Equal(0, _messageCount);
        }
    }
}