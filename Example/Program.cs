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
            masterManager.Start(MilliGanjubus.Create(serialPortProvider), new ForceUpdatePolicy(Int32.MaxValue));

            while (true)
            {
                byte currentByteValue = slaveManager.ConcreteDevice.TestByte;
                int currentIntValue = slaveManager.ConcreteDevice.TestInt;
                int currentReadOnlyValue = masterManager.ConcreteDevice.TestShort;
                Console.WriteLine($"current write-only byte value on slave device: {currentByteValue}");
                Console.WriteLine($"current write-only int value on slave device: {currentIntValue}");
                Console.WriteLine($"current read-only short value on master device: {currentReadOnlyValue}");
                
                masterManager.ConcreteDevice.TestByte++;
                masterManager.ConcreteDevice.TestInt++;
                slaveManager.ConcreteDevice.TestShort++;
                Console.ReadLine();
            }
            // ReSharper disable once FunctionNeverReturns
        }
    }
}