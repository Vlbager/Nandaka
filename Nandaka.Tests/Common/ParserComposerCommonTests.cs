using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Nandaka.Core.Helpers;
using Nandaka.Core.Protocol;
using Nandaka.Core.Session;
using Nandaka.Core.Table;
using Xunit;

namespace Nandaka.Tests.Common
{
    public class ParserComposerCommonTests : IParserComposerTests
    {
        private readonly MessageGenerator _messageGenerator;
        private readonly IProtocolInfo _protocolInfo;
        private readonly IParser<byte[], MessageReceivedEventArgs> _parser;
        private readonly IComposer<IMessage, byte[]> _composer;
        
        private int _messageCounter;
        private IMessage _parsedMessage;
        private AutoResetEvent _messageParsedResetEvent;

        public ParserComposerCommonTests(IParser<byte[], MessageReceivedEventArgs> parser, IComposer<IMessage, byte[]> composer,
                                          RegisterGenerator standardRegisterGenerator, IProtocolInfo protocolInfo)
        {
            _messageGenerator = new MessageGenerator(standardRegisterGenerator);
            _composer = composer;
            _protocolInfo = protocolInfo;
            _parser = parser;
            _parser.MessageParsed += OnMessageParsed;
            _messageParsedResetEvent = new AutoResetEvent(initialState: false);
        }
        
        public void AllValidSingleRegisterMessages()
        {
            IEnumerable<int> addresses = GetAllValidAddresses();

            foreach (IRegisterMessage message in _messageGenerator.GenerateSingleRegister(addresses, 1.ToEnumerable()))
                AssertRegisterMessage(message);
        }
        
        public void AllValidSizedRegisterMessages()
        {
            int maxRegisterSize = _messageGenerator.RegisterValueSize + _protocolInfo.AddressSize;
            int maxRegistersInMessage = _protocolInfo.MaxDataLength / maxRegisterSize;

            int[] addressPool = GetAllValidAddresses().Take(maxRegistersInMessage).ToArray();
            
            IEnumerable<int> messageSizes = Enumerable.Range(1, maxRegistersInMessage);
            
            foreach (IRegisterMessage message in _messageGenerator.Generate(messageSizes, addressPool, 1.ToEnumerable()))
                AssertRegisterMessage(message);
        }

        private void AssertRegisterMessage(IRegisterMessage message)
        {
            byte[] composed = _composer.Compose(message, out IReadOnlyCollection<IRegisterGroup> composedGroups);
                
            Assert.Equal(message.RegisterGroups.Count, composedGroups.Count);

            Assert.True(composed.Length <= _protocolInfo.MaxPacketLength);

            int currentCounterValue = _messageCounter;
            
            _parser.Parse(composed);

            Assert.True(_messageParsedResetEvent.WaitOne(TimeSpan.FromSeconds(1)));

            Assert.True(_messageCounter - currentCounterValue == 1);
            
            var parsedMessage = _parsedMessage as IReceivedMessage;
            Assert.NotNull(parsedMessage);
            Assert.Equal(message.OperationType, parsedMessage.OperationType);
            Assert.Equal(message.Type, parsedMessage.Type);
            Assert.Equal(message.SlaveDeviceAddress, parsedMessage.SlaveDeviceAddress);

            IRegister[] expectedRawRegisters = message.RegisterGroups
                                                      .SelectMany(group => group.GetRawRegisters())
                                                      .ToArray();

            Assert.Equal(expectedRawRegisters.Length, parsedMessage.Registers.Count);

            for (int i = 0; i < expectedRawRegisters.Length; i++)
            {
                IRegister expectedRegister = expectedRawRegisters[i];
                IRegister actualRegister = parsedMessage.Registers[i];
                
                Assert.Equal(expectedRegister.Address, actualRegister.Address);
                Assert.True(expectedRegister.ToBytes().SequenceEqual(actualRegister.ToBytes()));
            }
        }

        private IEnumerable<int> GetAllValidAddresses()
        {
            int messagesCount = (_protocolInfo.MaxRegisterAddress - _protocolInfo.MinRegisterAddress) / _messageGenerator.RegisterValueSize;

            return Enumerable.Range(_protocolInfo.MinRegisterAddress, messagesCount);
        }
        
        private void OnMessageParsed(object sender, MessageReceivedEventArgs e)
        {
            _messageCounter++;
            _parsedMessage = e.ReceivedMessage;
            _messageParsedResetEvent.Set();
        }
    }
}