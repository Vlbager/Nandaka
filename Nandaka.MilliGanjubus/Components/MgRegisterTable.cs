using System;
using System.Collections.Generic;
using System.Linq;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;
using Nandaka.Core.Registers;
using Nandaka.MilliGanjubus.Models;
using Nandaka.Model.Registers;

namespace Nandaka.MilliGanjubus.Components
{
    internal sealed class MgRegisterTable
    {
        private readonly Dictionary<int, MgMappedRegister> _internalTable;

        private MgRegisterTable(Dictionary<int, MgMappedRegister> internalTable)
        {
            _internalTable = internalTable;
        }

        public static MgRegisterTable CreateWithValidation(IReadOnlyCollection<IRegister> registers)
        {
            if (registers.IsEmpty())
                throw new ConfigurationException("Device should contain at least one register");
            
            ValidateRegisters(registers);
            
            var mgTable = new Dictionary<int, MgMappedRegister>(registers.Count);
            var addressPool = new HashSet<int>();

            foreach (IRegister register in registers)
            {
                if (mgTable.ContainsKey(register.Address))
                    throw new ConfigurationException($"More than one register with address '{register.Address.ToString()}' was defined");

                if (register.DataSize > MgInfo.MaxRegisterValueSize)
                    throw new ConfigurationException($"MG contract error: Register '{register.Address.ToString()}' value size is more than 9 bytes");
                
                byte[] registerBytes = register.ToBytes();
                
                var mgRegisters = new List<IRegister<byte>>(registerBytes.Length);

                for (var registerByteIndex = 0; registerByteIndex < registerBytes.Length; registerByteIndex++)
                {
                    int byteRegisterAddress = register.Address + registerByteIndex;
                    if (addressPool.Contains(byteRegisterAddress))
                        throw new ConfigurationException("MG contract error: more than one byte register with address " +
                                                         $"'{byteRegisterAddress.ToString()}' was defined." +
                                                         " Make sure that each byte of your register can have its own address");

                    addressPool.Add(byteRegisterAddress);
                    mgRegisters.Add(new Register<byte>(byteRegisterAddress, register.RegisterType, registerBytes[registerByteIndex]));
                }
                
                mgTable.Add(register.Address, new MgMappedRegister(register, mgRegisters.ToArray()));
            }

            return new MgRegisterTable(mgTable);
        }

        public IRegister<byte>[] ToMgRegisters(IReadOnlyCollection<IRegister> userRegisters)
        {
            var result = new List<IRegister<byte>>(userRegisters.Count);
            
            foreach (IRegister userRegister in userRegisters)
            {
                if (!_internalTable.TryGetValue(userRegister.Address, out MgMappedRegister? mappedRegister))
                    throw new NandakaBaseException($"MG registers with address '{userRegister.Address}' not found");

                byte[] bytes = userRegister.ToBytes();
                for (var byteIndex = 0; byteIndex < bytes.Length; byteIndex++)
                    mappedRegister.MgRegisters[byteIndex].Value = bytes[byteIndex];

                result.AddRange(mappedRegister.MgRegisters);   
            }

            return result.ToArray();
        }

        public IRegister[] ToUserRegisters(IReadOnlyList<IRegister<byte>> mgRegisters)
        {
            var userRegisters = new List<IRegister>();

            int mgRegisterIndex = 0; 
            while (mgRegisterIndex < mgRegisters.Count)
            {
                IRegister receivedHeadMgRegister = mgRegisters[mgRegisterIndex];
                int userRegisterAddress = receivedHeadMgRegister.Address;
                if (!_internalTable.TryGetValue(userRegisterAddress, out MgMappedRegister? mapped))
                    throw new InvalidRegistersReceivedException($"Can't find user register with address '{userRegisterAddress.ToString()}'");

                IRegister userRegister = mapped.UserRegister;
                if (receivedHeadMgRegister.RegisterType == RegisterType.RawWithoutValues)
                    userRegisters.Add(userRegister.CreateCopy());
                
                var receivedBytes = new List<byte>(userRegister.DataSize);
                
                foreach (IRegister<byte> mappedMgRegister in mapped.MgRegisters)
                {
                    IRegister<byte> receivedMgRegister = mgRegisters[mgRegisterIndex];
                    if (mappedMgRegister.Address != receivedMgRegister.Address)
                        throw new InvalidRegistersReceivedException($"Expected byte register address: '{mappedMgRegister.Address.ToString()}'. " +
                                                                    $"Actual byte register address: '{receivedMgRegister.Address.ToString()}'");
                    
                    receivedBytes.Add(receivedMgRegister.Value);
                    mgRegisterIndex++;
                }
                
                userRegisters.Add(userRegister.CreateCopyFromBytes(receivedBytes));
            }

            return userRegisters.ToArray();
        }

        private static void ValidateRegisters(IReadOnlyCollection<IRegister> registers)
        {
            IEnumerable<IRegister> roRegisters = registers.Where(register => register.RegisterType == RegisterType.ReadRequest);
            ValidateRegisterAddresses(roRegisters, MgInfo.ReadRequestRegistersAddressRange);

            IEnumerable<IRegister> rwRegisters = registers.Where(register => register.RegisterType == RegisterType.WriteRequest);
            ValidateRegisterAddresses(rwRegisters, MgInfo.WriteRequestRegistersAddressRange);
        }

        private static void ValidateRegisterAddresses(IEnumerable<IRegister> registers, Range addressRange)
        {
            int nextAddress = addressRange.Start.Value;
            foreach (IRegister register in registers.OrderBy(register => register.Address))
            {
                int address = register.Address;
                if (!addressRange.IfIncludes(register.Address))
                    throw new ConfigurationException( $"MG contract error: can't create {address.ToString()} register " +
                                                     $"with address '{address.ToString()}'");
                
                if (register.Address != nextAddress)
                    throw new ConfigurationException("MG contract error: register addresses should be in range. " +
                                                     "Make sure that each byte of your register can have its own address" +
                                                     $"Expected address: '{nextAddress.ToString()}', actual address: '{address.ToString()}'");

                nextAddress += register.DataSize;
            }
        }
    }
}