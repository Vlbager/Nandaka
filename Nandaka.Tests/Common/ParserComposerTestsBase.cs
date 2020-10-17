using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;
using Xunit;

namespace Nandaka.Tests.Common
{
    public abstract class ParserComposerTestsBase
    {
        private readonly MessageGenerator _messageGenerator;
        private readonly IParser<byte[], MessageReceivedEventArgs> _parser;
        private readonly IComposer<IMessage, byte[]> _composer;

        private readonly IProtocolInfo _protocolInfo;

        private int _messageCounter;
        private IMessage _parsedMessage;

        protected ParserComposerTestsBase(IParser<byte[], MessageReceivedEventArgs> parser, IComposer<IMessage, byte[]> composer,
                                          RegisterGenerator registerGenerator, IProtocolInfo protocolInfo)
        {
            _composer = composer;
            _protocolInfo = protocolInfo;
            _parser = parser;
            _parser.MessageParsed += OnMessageParsed;
            _messageGenerator = new MessageGenerator(registerGenerator);
        }

        [Fact]
        [Trait("ShouldParseCompose", "All")]
        public void AllValidSingleRegisterMessages()
        {
            IEnumerable<int> addresses = GetAllValidAddresses();

            foreach (IRegisterMessage message in _messageGenerator.GenerateSingleRegister(addresses, 1.ToEnumerable()))
                AssertRegisterMessage(message);
        }

        protected void AssertRegisterMessage(IRegisterMessage message)
        {
            byte[] composed = _composer.Compose(message, out IReadOnlyCollection<IRegisterGroup> composedGroups);
                
            Assert.Equal(message.RegisterGroups.Count, composedGroups.Count);

            Assert.True(composed.Length <= _protocolInfo.MaxPacketLength);

            int currentCounterValue = _messageCounter;
            
            _parser.Parse(composed);

            Assert.True(currentCounterValue - _messageCounter == 1);
            
            var parsedMessage = _parsedMessage as IRegisterMessage;
            Assert.NotNull(parsedMessage);
            Assert.Equal(message.OperationType, parsedMessage.OperationType);
            Assert.Equal(message.Type, parsedMessage.Type);
            Assert.Equal(message.SlaveDeviceAddress, parsedMessage.SlaveDeviceAddress);

            IRegister[] expectedRawRegisters = message.RegisterGroups
                                                      .SelectMany(group => group.GetRawRegisters())
                                                      .ToArray();

            IRegister[] actualRawRegisters = parsedMessage.RegisterGroups
                                                          .SelectMany(group => group.GetRawRegisters())
                                                          .ToArray();

            Assert.Equal(expectedRawRegisters.Length, actualRawRegisters.Length);

            for (int i = 0; i < expectedRawRegisters.Length; i++)
            {
                IRegister expectedRegister = expectedRawRegisters[i];
                IRegister actualRegister = actualRawRegisters[i];
                
                Assert.Equal(expectedRegister.Address, actualRegister.Address);
                Assert.True(expectedRegister.ToBytes().SequenceEqual(actualRegister.ToBytes()));
            }
        }

        private IEnumerable<int> GetAllValidAddresses()
        {
            int messagesCount = (_protocolInfo.MaxRegisterAddress - _protocolInfo.MinRegisterAddress) / _messageGenerator.RegisterSize;

            return Enumerable.Range(_protocolInfo.MinRegisterAddress, messagesCount);
        }
        
        private void OnMessageParsed(object sender, MessageReceivedEventArgs e)
        {
            _messageCounter++;
            _parsedMessage = e.ReceivedMessage;
        }
    }
}