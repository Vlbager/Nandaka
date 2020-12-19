using System;

namespace Nandaka.Core.Protocol
{
    public abstract class ApplicationParserBase<TIn> : IParser<TIn, MessageReceivedEventArgs>
    {
        private readonly DataLinkParserBase<TIn> _dataLinkParser;

        protected ApplicationParserBase(DataLinkParserBase<TIn> dataLinkParser)
        {
            _dataLinkParser = dataLinkParser;
            _dataLinkParser.MessageParsed += 
                (sender, checkedMessage) => MessageParsed?.Invoke(sender, ApplicationParse(checkedMessage));
        }

        public event EventHandler<MessageReceivedEventArgs>? MessageParsed;

        public void Parse(TIn data)
        {
            _dataLinkParser.Parse(data);
        }

        protected abstract MessageReceivedEventArgs ApplicationParse(byte[] data);
    }
}
