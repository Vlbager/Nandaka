using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using Example.MasterExample;
using Nandaka.Core.Device;
using Nandaka.Core.Network;
using Nandaka.MilliGanjubus;

namespace Example
{
    [SuppressMessage("ReSharper", "FunctionNeverReturns")]
    public static class Program
    {
        public static void Main()
        {
            MasterExample();
            //LocalMasterSlaveExample();
        }

        private static void MasterExample()
        {
            Console.WriteLine("MasterExample started");
            
            var serialPortProvider = new SerialDataPortProvider("COM3", 57600);
            var protocol = MilliGanjubus.Create(serialPortProvider);
            
            var manager = new TestMasterManager();
            manager.Start(protocol, new ForceUpdatePolicy(TimeSpan.FromMilliseconds(300), TimeSpan.FromMilliseconds(1)));

            TestDevice device = manager.TestDevice;
            
            device.PropertyChanged += DeviceOnPropertyChanged;
            
            device.RwByte  = 3;
            device.RwShort = 3;
            device.RwInt   = 3;
            device.RwLong  = 3;
            device.RwUlong = 3;

            while (true)
            {
                
                
                //Console.WriteLine(device.RoByte);
                
                Thread.Sleep(1000);
            }
        }

        private static void DeviceOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TestDevice.RoLong))
                Console.WriteLine($"RoLong property has been changed by slave device");
        }

        private static void LocalMasterSlaveExample()
        {
            Console.WriteLine("LocalMasterSlaveExample Started");

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
        }
    }
}