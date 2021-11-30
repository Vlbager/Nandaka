using System;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Registers;
using Nandaka.Model.Registers;
using Nandaka.Tests.Util;
using Xunit;

namespace Nandaka.Tests.Core
{
    public static class RegisterTableTests
    {
        private static readonly RegisterGenerator<int> Generator = new RegisterGenerator<int>();
        
        [Fact]
        [Trait("Create valid RegisterTable", "With single read register")]
        public static void CreateValidTableWithSingleReadRegister()
        {
            // Arrange
            IRegister[] registers = 
            {
                new Register<int>(1, RegisterType.ReadRequest)
            };
            
            // Act
            var table = RegisterTable.CreateWithValidation(registers);
            
            // Assert
            Assert.Single(table);
            Assert.True(table.TryGetRegister(1, out _));
        }
        
        [Fact]
        [Trait("Create valid RegisterTable", "With single write register")]
        public static void CreateValidTableWithSingleWriteRegister()
        {
            // Arrange
            IRegister[] registers = 
            {
                new Register<int>(99, RegisterType.ReadRequest)
            };
            
            // Act
            var table = RegisterTable.CreateWithValidation(registers);
            
            // Assert
            Assert.Single(table);
            Assert.True(table.TryGetRegister(99, out _));
        }
        
        [Fact]
        [Trait("Create valid RegisterTable", "With many registers")]
        public static void CreateValidTableWithManyRegisters()
        {
            // Arrange
            IRegister<int>[] registers = Generator.Generate(Enumerable.Range(0, 10), RegisterType.ReadRequest)
                                                  .ToArray();
            
            // Act
            var table = RegisterTable.CreateWithValidation(registers);
            
            // Assert
            Assert.Equal(registers.Length, table.Count);
        }
        
        [Fact]
        [Trait("Create valid RegisterTable", "With 0, Int32.MaxValue register addresses")]
        public static void CreateValidTableWithCornerRegisterAddresses()
        {
            // Arrange
            IRegister[] registers = 
            {
                new Register<int>(0, RegisterType.ReadRequest),
                new Register<int>(Int32.MaxValue, RegisterType.ReadRequest)
            };
            
            // Act
            var table = RegisterTable.CreateWithValidation(registers);
            
            // Assert
            Assert.Equal(registers.Length, table.Count);
            Assert.NotNull(table[0]);
            Assert.NotNull(table[Int32.MaxValue]);
        }
        
        [Fact]
        [Trait("Create invalid RegisterTable", "With -1 address register")]
        public static void CreateTableWithOutOfRangeRegisterAddress()
        {
            // Arrange
            IRegister[] registers = 
            {
                new Register<int>(0, RegisterType.ReadRequest),
                new Register<int>(Int32.MaxValue, RegisterType.ReadRequest),
                new Register<int>(-1, RegisterType.ReadRequest)
            };
            
            // Act & Assert
            Assert.Throws<ConfigurationException>(() => RegisterTable.CreateWithValidation(registers));
        }
        
        [Fact]
        [Trait("Create invalid RegisterTable", "With 2 same addresses registers")]
        public static void CreateTableWithDuplicateOfRegisterAddresses()
        {
            // Arrange
            IRegister[] registers = 
            {
                new Register<int>(1, RegisterType.ReadRequest),
                new Register<int>(1, RegisterType.ReadRequest)
            };
            
            // Act & Assert
            Assert.Throws<ConfigurationException>(() => RegisterTable.CreateWithValidation(registers));
        }
        
        [Fact]
        [Trait("Create invalid RegisterTable", "With wrong register type")]
        public static void CreateTableWithWrongRegisterType()
        {
            // Arrange
            IRegister[] registers = 
            {
                new Register<int>(1, RegisterType.Raw),
            };
            
            // Act & Assert
            Assert.Throws<ConfigurationException>(() => RegisterTable.CreateWithValidation(registers));
        }
    }
}