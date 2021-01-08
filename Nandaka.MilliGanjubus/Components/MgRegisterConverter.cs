using System.Collections.Generic;
using Nandaka.Core.Device;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Registers;

namespace Nandaka.MilliGanjubus.Components
{
    internal sealed class MgRegisterConverter
    {
        private readonly Dictionary<int, MgRegisterTable> _deviceTables;
        private readonly HashSet<int> _addressPool;

        private MgRegisterConverter()
        {
            _deviceTables = new Dictionary<int, MgRegisterTable>();
            _addressPool = new HashSet<int>();
        }

        public static MgRegisterConverter Create(IReadOnlyCollection<NandakaDeviceCtx> devices)
        {
            var tableMap = new MgRegisterConverter();
            foreach (NandakaDeviceCtx device in devices)
                tableMap.AddWithValidation(device);

            return tableMap;
        }

        public IRegister<byte>[] ConvertToMgRegisters(int deviceAddress, IReadOnlyList<IRegister> userRegisters)
        {
            if (!_deviceTables.TryGetValue(deviceAddress, out MgRegisterTable? mgTable))
                throw new NandakaBaseException($"Device with address '{deviceAddress.ToString()}' not found");

            return mgTable.ToMgRegisters(userRegisters);
        }

        public IRegister[] ConvertToUserRegisters(int deviceAddress, IReadOnlyList<IRegister<byte>> mgRegisters)
        {
            if (!_deviceTables.TryGetValue(deviceAddress, out MgRegisterTable? mgTable))
                throw new NandakaBaseException($"Device with address '{deviceAddress.ToString()}' not found");

            return mgTable.ToUserRegisters(mgRegisters);
        }

        private void AddWithValidation(NandakaDeviceCtx device)
        {
            int address = device.Address;
                
            if (_addressPool.Contains(address))
                throw new ConfigurationException($"More than one device with address '{address.ToString()}' has been defined");

            _addressPool.Add(address);

            MgRegisterTable registerTable = MgRegisterTable.CreateWithValidation(device.Registers);
            
            _deviceTables.Add(address, registerTable);
        }
    }
}