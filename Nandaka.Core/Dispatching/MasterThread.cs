using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Nandaka.Core.Device;
using Nandaka.Core.Session;

namespace Nandaka.Core.Dispatching
{
    internal class MasterThread
    {
        private readonly MasterDevice _device;
        private readonly MasterSession _session;
        private readonly Thread _thread;

        public MasterThread(MasterDevice device, MasterSession session)
        {
            _device = device;
            _session = session;
            _thread = new Thread(Routine) { IsBackground = true};
        }

        public void StartRoutine() => _thread.Start();

        public void Join() => _thread.Join();

        public void Abort() => _thread.Abort();

        private void Routine()
        {
            try
            {
                while (true)
                {
                    foreach (var VARIABLE in COLLECTION)
                    {
                        
                    }
                }
            }
            catch (Exception exception)
            {
                // todo: Add loger.
                Console.WriteLine(exception);
            }
        }
    }
}
