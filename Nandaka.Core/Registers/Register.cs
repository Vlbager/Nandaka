using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using Nandaka.Core.Exceptions;
using Nandaka.Core.Helpers;

namespace Nandaka.Core.Registers
{
    public sealed class Register<T> : IRegister<T>, IDisposable
        where T : struct
    {
        private static readonly int GenericParameterSize = Marshal.SizeOf<T>();

        private readonly ReaderWriterLockSlim _rwLock = new();

        private IRegisterByteValueConverter<T> _rvConverter = RegisterValueConverterFactory.GetLittleEndianConverter<T>();
        
        private T _value;
        private bool _isUpdated;
        private DateTime _lastUpdateTime;

        public int Address { get; }
        public RegisterType RegisterType { get; }
        public int DataSize => GenericParameterSize;
        
        public TimeSpan UpdateInterval { get; internal set; }

        public T Value
        {
            get => _rwLock.GetWithReadLock(ref _value);
            set => SetRegisterValue(value, isUpdated: false);
        }

        public bool IsUpdated => _rwLock.GetWithReadLock(ref _isUpdated);

        public DateTime LastUpdateTime => _rwLock.GetWithReadLock(ref _lastUpdateTime);
        
        public event EventHandler<RegisterChangedEventArgs>? OnRegisterChanged;

        public Register(int address, RegisterType registerType, T value = default)
        {
            Address = address;
            RegisterType = registerType;
            _value = value;
        }
        
        public void Update(IRegister updateRegister)
        {
            if (updateRegister is not IRegister<T> updateRegisterTyped)
                throw new InvalidRegistersReceivedException("Wrong register type to update");

            switch (updateRegister.RegisterType)
            {
                case RegisterType.Raw:
                    SetRegisterValue(updateRegisterTyped.Value, isUpdated: true);
                    break;
                
                case RegisterType.RawWithoutValues:
                    MarkAsUpdated();
                    break;

                default:
                    throw new NandakaBaseException("Wrong update register type");
            }
        }

        public void MarkAsUpdated()
        {
            SetRegisterValue(Value, isUpdated: true);
        }

        public IRegister CreateCopyFromBytes(IReadOnlyList<byte> bytes)
        {
            return new Register<T>(Address, RegisterType.Raw, _rvConverter.FromBytes(bytes));
        }

        public IRegister CreateCopy()
        {
            return new Register<T>(Address, RegisterType.RawWithoutValues);
        }

        public Type GetValueType()
        {
            return Value.GetType();
        }

        public byte[] ToBytes()
        {
            return _rvConverter.ToBytes(Value);
        }

        internal void SetRvConverter(bool isLittleEndian)
        {
            _rvConverter = isLittleEndian 
                ? RegisterValueConverterFactory.GetLittleEndianConverter<T>() 
                : RegisterValueConverterFactory.GetBigEndianConverter<T>();
        }

        public void Dispose()
        {
            _rwLock.Dispose();
        }

        public override int GetHashCode()
        {
            return Address.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return Equals(obj as Register<T>);
        }

        public bool Equals(Register<T>? register)
        {
            return Address == register?.Address;
        }

        private void SetRegisterValue(T value, bool isUpdated)
        {
            _rwLock.EnterWriteLock();
            _value = value;
            _isUpdated = isUpdated;
            _lastUpdateTime = DateTime.Now;
            _rwLock.ExitWriteLock();
            OnRegisterChanged?.Invoke(this, new RegisterChangedEventArgs(Address));
        }
    }
}