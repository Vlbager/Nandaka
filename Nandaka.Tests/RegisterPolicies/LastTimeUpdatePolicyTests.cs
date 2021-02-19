﻿using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Device;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;
using Nandaka.Tests.Util;
using Xunit;

namespace Nandaka.Tests.RegisterPolicies
{
    public static class LastTimeUpdatePolicyTests
    {
        private class TestDevice : ForeignDevice
        {
            public override string Name => nameof(TestDevice);
            
            private TestDevice(int address, RegisterTable table, DeviceState state) 
                : base(address, table, state) { }

            public static TestDevice Create(IEnumerable<IRegister> registers)
            {
                var table = RegisterTable.CreateWithValidation(registers);
                return new TestDevice(1, table, DeviceState.Connected);
            }
        }
        
        private static readonly LastTimeUpdatePolicy UpdatePolicy = new();

        [Fact]
        [Trait("Get Write Request", "With single write register")]
        public static void DeviceWithSingleWriteRegister()
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
        [Trait("Get Read Request", "With single read register")]
        public static void DeviceWithSingleReadRegister()
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
        [Trait("Get empty Request", "Without any register")]
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
        [Trait("Get Write Request", "With outdated write register and up to date read register")]
        public static void DeviceWithOutdatedWriteRegister()
        {
            // Arrange
            IRegister[] registers =
            {
                new Register<int>(1, RegisterType.WriteRequest),
                new Register<int>(2, RegisterType.ReadRequest)
            };
            var device = TestDevice.Create(registers);
            device.Table[2].ChangeLastUpdateTime(DateTime.Now);
            
            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            
            // Assert
            Assert.Equal(OperationType.Write, message.OperationType);
            Assert.Equal(1, message.Registers.Count);
        }
        
        [Fact]
        [Trait("Get Read Request", "With outdated read register and up to date write register")]
        public static void DeviceWithOutdatedReadRegister()
        {
            // Arrange
            IRegister[] registers =
            {
                new Register<int>(1, RegisterType.WriteRequest),
                new Register<int>(2, RegisterType.ReadRequest)
            };
            var device = TestDevice.Create(registers);
            device.Table[1].ChangeLastUpdateTime(DateTime.Now);
            
            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            
            // Assert
            Assert.Equal(OperationType.Read, message.OperationType);
            Assert.Equal(1, message.Registers.Count);
        }
        
        [Fact]
        [Trait("Get Read Request", "With outdated not ordered by address read register and up to date write register")]
        public static void DeviceWithOutdatedNotOrderedReadRegister()
        {
            // Arrange
            IRegister[] registers =
            {
                new Register<int>(1, RegisterType.WriteRequest),
                new Register<int>(2, RegisterType.ReadRequest),
                new Register<int>(8, RegisterType.ReadRequest),
                new Register<int>(5, RegisterType.ReadRequest)
            };
            var device = TestDevice.Create(registers);
            device.Table[1].ChangeLastUpdateTime(DateTime.Now);
            
            // Act
            IRegisterMessage message = UpdatePolicy.GetNextMessage(device);
            IOrderedEnumerable<IRegister> orderedRegisters = message.Registers.OrderBy(register => register.Address);
            
            // Assert
            Assert.Equal(OperationType.Read, message.OperationType);
            Assert.Equal(3, message.Registers.Count);
            Assert.Equal(orderedRegisters, message.Registers);
        }
    }
}