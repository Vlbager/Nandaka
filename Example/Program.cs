using System;
using Nandaka.Core.Device;
using Nandaka.Core.Network;
using Nandaka.MilliGanjubus;

namespace Example
{
    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("Example Started");

            var serialPortProvider = new SerialDataPortProvider("COM3");
            
            var slaveManager = ConcreteSlaveManager.Create();
            slaveManager.Start(MilliGanjubus.Create(serialPortProvider));
            
            var masterManager = ConcreteMasterManager.Create();
            masterManager.Start(MilliGanjubus.Create(serialPortProvider), new ForceUpdatePolicy());

            while (true)
            {
                byte currentByteValue = slaveManager.ConcreteDevice.TestByte;
                Console.WriteLine(currentByteValue);
                Console.ReadLine();
                masterManager.ConcreteDevice.TestByte++;
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}