using Nandaka.Core;
using Serilog.Extensions.Logging;

namespace Nandaka.Serilog
{
    public static class SerilogILoggerFactoryExtensions
    {
        public static void ConfigureSerilogFactory(this LogConfiguration configuration)
        {
            var logFactory = new SerilogLoggerFactory(logger: null, dispose: true);

            configuration.ConfigureLogFactory(logFactory);            
        }
    }
}