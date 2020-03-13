using System;
using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Table
{
    public class RegisterTable<TRegisterType> where TRegisterType : struct
    {
        private readonly TRegisterType[] _readOnlyRegisters;
        private readonly TRegisterType[] _writeOnlyRegisters;

        private readonly int _readOnlyRegistersOffset;
        private readonly int _writeOnlyRegistersOffset;

        private RegisterTable(int readOnlyRegistersCount, int readOnlyRegistersOffset,
            int writeOnlyRegistersCount, int writeOnlyRegistersOffset)
        {
            _readOnlyRegisters = new TRegisterType[readOnlyRegistersCount];
            _readOnlyRegistersOffset = readOnlyRegistersOffset;
            _writeOnlyRegisters = new TRegisterType[writeOnlyRegistersCount];
            _writeOnlyRegistersOffset = writeOnlyRegistersOffset;
        }

        #region Create Methods

        public static RegisterTable<byte> CreateByteTable(int readOnlyRegistersCount, int readOnlyRegistersOffset,
            int writeOnlyRegistersCount, int writeOnlyRegistersOffset)
        {
            return new RegisterTable<byte>(readOnlyRegistersCount, readOnlyRegistersOffset, writeOnlyRegistersCount, writeOnlyRegistersOffset);
        }

        public static RegisterTable<ushort> CreateUInt16Table(int readOnlyRegistersCount, int readOnlyRegistersOffset,
            int writeOnlyRegistersCount, int writeOnlyRegistersOffset)
        {
            return new RegisterTable<ushort>(readOnlyRegistersCount, readOnlyRegistersOffset, writeOnlyRegistersCount, writeOnlyRegistersOffset);
        }

        public static RegisterTable<uint> CreateUInt32Table(int readOnlyRegistersCount, int readOnlyRegistersOffset,
            int writeOnlyRegistersCount, int writeOnlyRegistersOffset)
        {
            return new RegisterTable<uint>(readOnlyRegistersCount, readOnlyRegistersOffset, writeOnlyRegistersCount, writeOnlyRegistersOffset);
        }

        public static RegisterTable<ulong> CreateUInt64Table(int readOnlyRegistersCount, int readOnlyRegistersOffset,
            int writeOnlyRegistersCount, int writeOnlyRegistersOffset)
        {
            return new RegisterTable<ulong>(readOnlyRegistersCount, readOnlyRegistersOffset, writeOnlyRegistersCount, writeOnlyRegistersOffset);
        }

        #endregion

        public TRegisterType GetRegister(int address)
        {
            int index = address - _readOnlyRegistersOffset;

            if (!IsValidIndexOfCollection(index, _readOnlyRegisters))
                // todo: create a custom exception.
                throw new ArgumentException($"{address} is not a valid address");

            return _readOnlyRegisters[index];
        }

        public TRegisterType[] GetRegisters(int firstRegisterAddress, int count)
        {
            int beginIndex = firstRegisterAddress - _readOnlyRegistersOffset;
            int endIndex = beginIndex + count;

            if (!IsValidIndexOfCollection(beginIndex, _readOnlyRegisters) || !IsValidIndexOfCollection(endIndex, _readOnlyRegisters))
                // todo: create a custom exception.
                throw new ArgumentException($"Does not exists {count} registers with {firstRegisterAddress} first address");

            return _readOnlyRegisters
                .Skip(beginIndex)
                .Take(count)
                .ToArray();
        }

        public void SetRegister(int address, TRegisterType value)
        {
            int index = address - _writeOnlyRegistersOffset;

            if (!IsValidIndexOfCollection(index, _writeOnlyRegisters))
                // todo: create a custom exception.
                throw new ArgumentException($"{address} is not a valid address");

            _writeOnlyRegisters[index] = value;
        }

        public void SetRegisters(int firstRegisterAddress, IReadOnlyCollection<TRegisterType> values)
        {
            int beginIndex = firstRegisterAddress - _writeOnlyRegistersOffset;
            int endIndex = beginIndex + values.Count;

            if (!IsValidIndexOfCollection(beginIndex, _writeOnlyRegisters) || !IsValidIndexOfCollection(endIndex, _writeOnlyRegisters))
                // todo: create a custom exception.
                throw new ArgumentException($"Does not exists {values.Count} registers with {firstRegisterAddress} first address");

            int arrayIndex = beginIndex;
            foreach (TRegisterType value in values)
                _writeOnlyRegisters[arrayIndex++] = value;
        }


        private bool IsValidIndexOfCollection(int index, IReadOnlyCollection<TRegisterType> collection)
        {
            return (index >= 0 || index < collection.Count);
        }
    }
}
