using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Registers;
using Nandaka.MilliGanjubus.Components;
using Nandaka.Tests.Util;
using Xunit;

namespace Nandaka.Tests.MilliGanjubus
{
    public sealed class MgTableTests
    {
        [Fact]
        [Trait("Valid", "64 int32 registers")]
        public void FullInt32Table()
        {
            // Arrange
            IRegister<int>[] allRegisters = GetFullInt32Table();

            // Act
            var mgRegisterTable = MgRegisterTable.CreateWithValidation(allRegisters);
            
            // Assert
            IRegister<byte>[] byteRegisters = mgRegisterTable.ToMgRegisters(allRegisters);

            Assert.Equal(256, byteRegisters.Length);
        }
        
        [Fact]
        [Trait("Valid", "1 read register")]
        public void MinimalValidReadTable()
        {
            // Arrange
            IRegister<byte>[] registers = { new Register<byte>(0, RegisterType.ReadRequest) };

            // Act
            var mgRegisterTable = MgRegisterTable.CreateWithValidation(registers);
            
            // Assert
            IRegister<byte>[] byteRegisters = mgRegisterTable.ToMgRegisters(registers);

            Assert.Single(byteRegisters);
            Assert.Equal(RegisterType.ReadRequest, byteRegisters.Single().RegisterType);
            Assert.Equal(0, byteRegisters.Single().Address);
        }
        
        [Fact]
        [Trait("Valid", "1 write register")]
        public void MinimalValidWriteTable()
        {
            // Arrange
            IRegister<byte>[] registers = { new Register<byte>(128, RegisterType.WriteRequest) };

            // Act
            var mgRegisterTable = MgRegisterTable.CreateWithValidation(registers);
            
            // Assert
            IRegister<byte>[] byteRegisters = mgRegisterTable.ToMgRegisters(registers);

            Assert.Single(byteRegisters);
            Assert.Equal(RegisterType.WriteRequest, byteRegisters.Single().RegisterType);
            Assert.Equal(128, byteRegisters.Single().Address);
        }
        
        [Fact]
        [Trait("Valid", "1 read register, 1 write register")]
        public void MinimalValidReadWriteTable()
        {
            // Arrange
            IRegister<byte>[] registers =
            {
                new Register<byte>(0, RegisterType.ReadRequest),
                new Register<byte>(128, RegisterType.WriteRequest)
            };

            // Act
            var mgRegisterTable = MgRegisterTable.CreateWithValidation(registers);
            
            // Assert
            IRegister<byte>[] byteRegisters = mgRegisterTable.ToMgRegisters(registers);

            Assert.Equal(2, byteRegisters.Length);
            
            Assert.Equal(RegisterType.ReadRequest, byteRegisters[0].RegisterType);
            Assert.Equal(0, byteRegisters[0].Address);
            
            Assert.Equal(RegisterType.WriteRequest, byteRegisters[1].RegisterType);
            Assert.Equal(128, byteRegisters[1].Address);
        }

        [Fact]
        [Trait("Valid", "int16 register")]
        public void Int16Register()
        {
            // Arrange
            const short numeratedValue = (1 << 8) | 0;
            IRegister<short>[] registers = { new Register<short>(0, RegisterType.ReadRequest, numeratedValue) };
            
            // Act
            var mgRegistersTable = MgRegisterTable.CreateWithValidation(registers);
            
            // Assert
            IRegister<byte>[] mgRegisters = mgRegistersTable.ToMgRegisters(registers);
            
            Assert.Equal(sizeof(short), mgRegisters.Length);

            for (byte i = 0; i < mgRegisters.Length; i++)
            {
                Assert.Equal(i, mgRegisters[i].Value);
                mgRegisters[i].Value++;
            }

            var userRegister = mgRegistersTable.ToUserRegisters(mgRegisters).FirstOrDefault() as IRegister<short>;
            Assert.NotNull(userRegister);
            
            Assert.Equal((2 << 8) | 1, userRegister!.Value);
        }
        
        [Fact]
        [Trait("Valid", "uint32 register")]
        public void UInt32Register()
        {
            // Arrange
            const uint numeratedValue = (3 << 24) | (2 << 16) | (1 << 8) | 0;
            IRegister<uint>[] registers = { new Register<uint>(0, RegisterType.ReadRequest, numeratedValue) };
            
            // Act
            var mgRegistersTable = MgRegisterTable.CreateWithValidation(registers);
            
            // Assert
            IRegister<byte>[] mgRegisters = mgRegistersTable.ToMgRegisters(registers);
                        
            Assert.Equal(sizeof(uint), mgRegisters.Length);

            for (byte i = 0; i < mgRegisters.Length; i++)
            {
                Assert.Equal(i, mgRegisters[i].Value);
                mgRegisters[i].Value++;
            }

            var userRegister = mgRegistersTable.ToUserRegisters(mgRegisters).FirstOrDefault() as IRegister<uint>;
            Assert.NotNull(userRegister);
            
            Assert.Equal((uint)((4 << 24) | (3 << 16) | (2 << 8) | 1), userRegister!.Value);
        }
        
        [Fact]
        [Trait("Valid", "int64 register")]
        public void Int64Register()
        {
            // Arrange
            const long numeratedValue = (7L << 56) | (6L << 48) | (5L << 40) | (4L << 32) | (3 << 24) | (2 << 16) | (1 << 8) | 0;
            IRegister<long>[] registers = { new Register<long>(0, RegisterType.ReadRequest, numeratedValue) };
            
            // Act
            var mgRegistersTable = MgRegisterTable.CreateWithValidation(registers);
            
            // Assert
            IRegister<byte>[] mgRegisters = mgRegistersTable.ToMgRegisters(registers);
            for (byte i = 0; i < mgRegisters.Length; i++)
            {
                Assert.Equal(mgRegisters[i].Value, i);
                mgRegisters[i].Value++;
            }

            var userRegister = mgRegistersTable.ToUserRegisters(mgRegisters).FirstOrDefault() as IRegister<long>;
            Assert.NotNull(userRegister);
            
            Assert.Equal((8L << 56) | (7L << 48) | (6L << 40) | (5L << 32) | (4 << 24) | (3 << 16) | (2 << 8) | 1, userRegister!.Value);
        }
        
        [Fact]
        [Trait("Invalid", "At least one register should be defined")]
        public void EmptyTable()
        {
            // Arrange 
            IRegister<byte>[] registers = Array.Empty<IRegister<byte>>();
            
            // Act & Assert
            Assert.Throws<ConfigurationException>(() => MgRegisterTable.CreateWithValidation(registers));
        }

        [Fact]
        [Trait("Invalid", "Addresses should be in range")]
        public void NotInRangeAddresses()
        {
            // Arrange 
            IRegister<byte>[] registers = new RegisterGenerator<byte>().Generate(new[] { 0, 3 }, RegisterType.ReadRequest)
                                                                     .ToArray();
            
            // Act & Assert
            Assert.Throws<ConfigurationException>(() => MgRegisterTable.CreateWithValidation(registers));
        }

        [Fact]
        [Trait("Invalid", "Address duplicate")]
        public void RegistersWithSameAddresses()
        {
            // Arrange 
            IRegister<int>[] registers = new RegisterGenerator<int>().Generate(new[] { 0, 0 }, RegisterType.ReadRequest)
                                                                     .ToArray();
            
            // Act & Assert
            Assert.Throws<ConfigurationException>(() => MgRegisterTable.CreateWithValidation(registers));
        }

        [Fact]
        [Trait("Invalid", "Read register address out of range")]
        public void InvalidReadRegisterAddresses()
        {
            // Arrange 
            IRegister<int>[] registers = new RegisterGenerator<int>().Generate(new[] { -1, 128 }, RegisterType.ReadRequest)
                                                                     .ToArray();
            
            // Act & Assert
            Assert.Throws<ConfigurationException>(() => MgRegisterTable.CreateWithValidation(new[] {registers[0]}));
            Assert.Throws<ConfigurationException>(() => MgRegisterTable.CreateWithValidation(new[] {registers[1]}));
        }
        
        [Fact]
        [Trait("Invalid", "Write register address out of range")]
        public void InvalidWriteRegisterAddresses()
        {
            // Arrange 
            IRegister<int>[] registers = new RegisterGenerator<int>().Generate(new[] { 127, 256 }, RegisterType.WriteRequest)
                                                                     .ToArray();
            
            // Act & Assert
            Assert.Throws<ConfigurationException>(() => MgRegisterTable.CreateWithValidation(new[] {registers[0]}));
            Assert.Throws<ConfigurationException>(() => MgRegisterTable.CreateWithValidation(new[] {registers[1]}));
        }

        [Fact]
        [Trait("Invalid", "Too big struct")]
        public void BigStruct()
        {
            // Arrange
            IRegister<TenBytesTestStruct>[] registers = new RegisterGenerator<TenBytesTestStruct>().Generate(0.ToEnumerable(), RegisterType.ReadRequest)
                                                                                                   .ToArray();
            
            // Act & Assert
            Assert.Throws<ConfigurationException>(() => MgRegisterTable.CreateWithValidation(registers));
        }

        private static IRegister<int>[] GetFullInt32Table()
        {
            IEnumerable<int> readRegistersAddresses = Enumerable.Range(0, 32)
                                                                .Select(num => num * 4);

            IEnumerable<int> writeRegistersAddresses = Enumerable.Range(32, 32)
                                                                 .Select(num => num * 4);

            var intRegisterGenerator = new RegisterGenerator<int>();

            IEnumerable<IRegister<int>> readRegisters = intRegisterGenerator.Generate(readRegistersAddresses, RegisterType.ReadRequest);
            IEnumerable<IRegister<int>> writeRegisters = intRegisterGenerator.Generate(writeRegistersAddresses, RegisterType.WriteRequest);

            IRegister<int>[] allRegisters = readRegisters.Concat(writeRegisters)
                                                         .ToArray();
            return allRegisters;
        }
    }
}