using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Nandaka.Core
{
    internal class Log : ILog
    {
        private static volatile ILog _instance;
        private static readonly object s_syncRoot = new object();

        private Log()
        {
            // todo: implement this.
        }

        private static Log Create()
        {
            // todo: implement this.
            return new Log();
        }
        
        public static ILog Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                lock (s_syncRoot)
                {
                    if (_instance != null)
                        return _instance;

                    _instance = Create();
                }

                return _instance;
            }
        }

        public void AppendMessage(LogMessageType type, string message)
        {
            // todo: implement this.
        }
    }
}