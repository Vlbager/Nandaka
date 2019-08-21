using System;
using System.Collections.Generic;
using System.Text;

namespace Nandaka
{
    abstract class ApplicationParserBase<T> : IParser<T, IProtocolMessage>
    {
        private readonly DataLinkParserBase<T> _dataLinkParser;

        protected ApplicationParserBase(DataLinkParserBase<T> dataLinkParser)
        {
            _dataLinkParser = dataLinkParser;
            _dataLinkParser.MessageParsed += 
                (sender, checkedMessage) => MessageParsed(sender, ApplicationParse(checkedMessage));
        }

        public int AwaitingReplyAddress
        {
            get => _dataLinkParser.AwaitingReplyAddress;
            set => _dataLinkParser.AwaitingReplyAddress = value;
        }

        public event EventHandler<IProtocolMessage> MessageParsed;

        public void Parse(T data)
        {
            _dataLinkParser.Parse(data);
        }

        protected abstract IProtocolMessage ApplicationParse(byte[] data);
    }
}
