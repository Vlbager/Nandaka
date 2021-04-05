using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Helpers;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.Tests.Util;
using Xunit;

namespace Nandaka.Tests.RegisterPolicies
{
    public static class WriteFirstRegisterPolicyTests
    {
        private static readonly WriteFirstUpdatePolicy UpdatePolicy = new();
        private static readonly RegisterGenerator<int> Generator = new();
        
        [Fact]
        [Trait("Get Write Request", "With single not updated write register")]
        public static void DeviceWithSingleNotUpdatedWriteRegister()
        {
            // Arrange
            IRegister[] registers =
            {
                new Register<int>(1, RegisterType.WriteRequest)
            };
            var device = TestDevice.Create(registers);
            
            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            
            // Assert
            Assert.Equal(OperationType.Write, message.OperationType);
            Assert.Equal(registers.Length, message.Registers.Count);
        }
        
        [Fact]
        [Trait("Get Read Request", "With single not updated read register")]
        public static void DeviceWithSingleNotUpdatedReadRegister()
        {
            // Arrange
            IRegister[] registers =
            {
                new Register<int>(1, RegisterType.ReadRequest)
            };
            var device = TestDevice.Create(registers);
            
            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            
            // Assert
            Assert.Equal(OperationType.Read, message.OperationType);
            Assert.Equal(registers.Length, message.Registers.Count);
        }
        
        [Fact]
        [Trait("Get empty Request", "With single updated write register")]
        public static void DeviceWithSingleUpdatedWriteRegister()
        {
            // Arrange
            IRegister[] registers =
            {
                new Register<int>(1, RegisterType.WriteRequest)
            };
            var device = TestDevice.Create(registers);
            
            device.Table.First().MarkAsUpdated();
            
            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            
            // Assert
            Assert.True(message is EmptyMessage);
        }
        
        [Fact]
        [Trait("Get empty message", "Without registers")]
        public static void DeviceWithoutRegisters()
        {
            // Arrange
            var device = TestDevice.Create(Array.Empty<IRegister>());
            
            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            
            // Assert
            Assert.True(message is EmptyMessage);
        }
        
        [Fact]
        [Trait("Get Write Request", "With not updated write and read register")]
        public static void DeviceWithNotUpdatedWriteAndReadRegister()
        {
            // Arrange
            IRegister[] registers =
            {
                new Register<int>(1, RegisterType.WriteRequest),
                new Register<int>(2, RegisterType.ReadRequest)
            };
            var device = TestDevice.Create(registers);
            
            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            
            // Assert
            Assert.Equal(OperationType.Write, message.OperationType);
            Assert.Equal(1, message.Registers.Count);
        }
        
        [Fact]
        [Trait("Get Read Request", "With updated write and read register")]
        public static void DeviceWithUpdatedWriteAndReadRegister()
        {
            // Arrange
            IRegister[] registers =
            {
                new Register<int>(1, RegisterType.WriteRequest),
                new Register<int>(2, RegisterType.ReadRequest)
            };
            var device = TestDevice.Create(registers);
            
            device.Table[1].MarkAsUpdated();
            
            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            
            // Assert
            Assert.Equal(OperationType.Read, message.OperationType);
            Assert.Equal(1, message.Registers.Count);
        }
        
        [Fact]
        [Trait("Get Write Request", "With mixed updated write and read register")]
        public static void DeviceWithMixedUpdatedWriteAndReadRegister()
        {
            // Arrange
            IRegister[] registers =
            {
                new Register<int>(0, RegisterType.WriteRequest),
                new Register<int>(1, RegisterType.WriteRequest),
                new Register<int>(2, RegisterType.ReadRequest)
            };
            var device = TestDevice.Create(registers);
            
            device.Table[1].MarkAsUpdated();
            
            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            
            // Assert
            Assert.Equal(OperationType.Write, message.OperationType);
            Assert.Equal(1, message.Registers.Count);
        }
        
        [Fact]
        [Trait("Get Write Request", "With many not ordered by address registers")]
        public static void DeviceWithManyWriteRegistersNotOrderedByAddress()
        {
            // Arrange
            IRegister<int>[] registers = Generator.Generate(Enumerable.Range(0, 10).OrderByDescending(a => a), RegisterType.WriteRequest)
                                                  .ToArray();
            
            var device = TestDevice.Create(registers);

            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            IEnumerable<IRegister> orderedRegisters = message.Registers.OrderBy(register => register.Address); 
            
            // Assert
            Assert.Equal(OperationType.Write, message.OperationType);
            Assert.Equal(10, message.Registers.Count);
            Assert.Equal(orderedRegisters, message.Registers);
        }
        
        [Fact]
        [Trait("Get Read Request", "With many not ordered by update time registers")]
        public static void DeviceWithManyReadRegistersNotOrderedByLastUpdateTime()
        {
            // Arrange
            DateTime registerUpdateTime = DateTime.Now;
            IRegister<int>[] registers = Generator.Generate(Enumerable.Range(0, 10), RegisterType.ReadRequest)
                                                  .With(register =>
                                                  {
                                                      register.ChangeLastUpdateTime(registerUpdateTime);
                                                      registerUpdateTime -= TimeSpan.FromHours(1);
                                                  })
                                                  .ToArray();
            
            var device = TestDevice.Create(registers);

            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            IEnumerable<IRegister> orderedRegisters = message.Registers.OrderBy(register => register.LastUpdateTime); 
            
            // Assert
            Assert.Equal(OperationType.Read, message.OperationType);
            Assert.Equal(10, message.Registers.Count);
            Assert.Equal(orderedRegisters, message.Registers);
        }
        
        [Fact]
        [Trait("Get Read Request", "With many ordered by update time, but by addresses registers")]
        public static void DeviceWithManyReadRegistersNotOrderedByAddresses()
        {
            // Arrange
            DateTime registerUpdateTime = DateTime.Now;
            IRegister<int>[] registers = Generator.Generate(Enumerable.Range(0, 10).OrderByDescending(a => a), RegisterType.ReadRequest)
                                                  .With(register => register.ChangeLastUpdateTime(registerUpdateTime))
                                                  .ToArray();
            
            var device = TestDevice.Create(registers);

            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            IEnumerable<IRegister> orderedRegisters = message.Registers.OrderBy(register => register.Address); 
            
            // Assert
            Assert.Equal(OperationType.Read, message.OperationType);
            Assert.Equal(10, message.Registers.Count);
            Assert.Equal(orderedRegisters, message.Registers);
        }
    }
}