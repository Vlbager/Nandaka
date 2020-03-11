using System;
using System.Collections.Generic;
using System.Linq;

namespace Nandaka.Core.Table
{
    public class RegisterTable<T> where T : struct
    {
        private readonly T[] _readOnlyRegisters;
        private readonly T[] _writeOnlyRegisters;

        private readonly int _readOnlyRegistersOffset;
        private readonly int _writeOnlyRegistersOffset;

        public RegisterTable(int readOnlyRegistersCount, int readOnlyRegistersOffset,
            int writeOnlyRegistersCount, int writeOnlyRegistersOffset)
        {
            _readOnlyRegisters = new T[readOnlyRegistersCount];
            _readOnlyRegistersOffset = readOnlyRegistersOffset;
            _writeOnlyRegisters = new T[writeOnlyRegistersCount];
            _writeOnlyRegistersOffset = writeOnlyRegistersOffset;
        }

        public T GetRegister(int address)
        {
            int index = address - _readOnlyRegistersOffset;

            if (!IsValidIndexOfCollection(index, _readOnlyRegisters))
                // todo: create a custom exception.
                throw new ArgumentException($"{address} is not a valid address");

            return _readOnlyRegisters[index];
        }

        public T[] GetRegisters(int firstRegisterAddress, int count)
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

        public void SetRegister(int address, T value)
        {
            int index = address - _writeOnlyRegistersOffset;

            if (!IsValidIndexOfCollection(index, _writeOnlyRegisters))
                // todo: create a custom exception.
                throw new ArgumentException($"{address} is not a valid address");

            _writeOnlyRegisters[index] = value;
        }

        public void SetRegisters(int firstRegisterAddress, IReadOnlyCollection<T> values)
        {
            int beginIndex = firstRegisterAddress - _writeOnlyRegistersOffset;
            int endIndex = beginIndex + values.Count;

            if (!IsValidIndexOfCollection(beginIndex, _writeOnlyRegisters) || !IsValidIndexOfCollection(endIndex, _writeOnlyRegisters))
                // todo: create a custom exception.
                throw new ArgumentException($"Does not exists {values.Count} registers with {firstRegisterAddress} first address");

            int arrayIndex = beginIndex;
            foreach (T value in values)
                _writeOnlyRegisters[arrayIndex++] = value;
        }


        private bool IsValidIndexOfCollection(int index, IReadOnlyCollection<T> collection)
        {
            return (index >= 0 || index < collection.Count);
        }
    }
}
