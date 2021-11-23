using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Nandaka.Core
{
    public sealed class LogConfiguration
    {
        public ILoggerFactory Factory { get; private set; }

        public LogConfiguration()
        {
            Factory = new NullLoggerFactory();
        }

        public void ConfigureLogFactory(ILoggerFactory loggerFactory)
        {
            Factory = loggerFactory;
        }
    }
}