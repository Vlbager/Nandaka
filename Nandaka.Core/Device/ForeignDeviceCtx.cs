using System.Collections.Generic;
using Nandaka.Core.Registers;
using Nandaka.Core.Session;

namespace Nandaka.Core.Device
{
    public abstract class ForeignDeviceCtx : NandakaDeviceCtx
    {
        internal IRegistersUpdatePolicy UpdatePolicy { get; }
        public DeviceState State { get; set; }
        public Dictionary<DeviceError, int> ErrorCounter { get; }

        protected ForeignDeviceCtx(int address, DeviceState state, IRegistersUpdatePolicy updatePolicy, ISpecificMessageHandler specificMessageHandler)
            :base(address, specificMessageHandler)
        {
            UpdatePolicy = updatePolicy;
            ErrorCounter = new Dictionary<DeviceError, int>();
            State = state;
        }

        protected ForeignDeviceCtx(int address, IRegistersUpdatePolicy updatePolicy, DeviceState state)
            : this(address, state, updatePolicy, new NullSpecificMessageHandler()) { }
        protected new static Register<T> CreateRwRegister<T>(int address, T value = default)
            where T: struct
        {
            return new Register<T>(address, RegisterType.WriteRequest, value);
        }

        protected new static Register<T> CreateRoRegister<T>(int address, T value = default)
            where T: struct
        {
            return new Register<T>(address, RegisterType.ReadRequest, value);
        }

        // internal void Reflect(bool isManagedByMaster)
        // {
        //     if (_isReflected)
        //         return;
        //     
        //     Type type = GetType();
        //
        //     var validator = new RegisterTableValidator();
        //     
        //     foreach (PropertyInfo propertyInfo in type.GetProperties())
        //     {
        //         Type propertyType = propertyInfo.PropertyType;
        //         
        //         if (!propertyInfo.PropertyType.IsInheritedFromInterface(nameof(IRegister)))
        //             continue;
        //
        //         if (!(propertyInfo.GetValue(this) is IRegister registerGroup))
        //             continue;
        //
        //         if (propertyType == typeof(IRwRegister<>))
        //         {
        //             registerGroup.SetRegisterTypeViaReflection(isManagedByMaster
        //                 ? RegisterType.WriteRequest
        //                 : RegisterType.ReadRequest);
        //         }
        //         else if (propertyType == typeof(IRoRegister<>))
        //         {
        //             registerGroup.SetRegisterTypeViaReflection(isManagedByMaster
        //                 ? RegisterType.ReadRequest
        //                 : RegisterType.WriteRequest);
        //         }
        //         else
        //         {
        //             continue;
        //         }
        //
        //         registerGroup.OnRegisterChanged += (_, _) => RaisePropertyChanged(propertyInfo.Name);
        //
        //         IEnumerable<RegisterModifyAttribute> attributes = propertyInfo.CustomAttributes
        //             .SafeCast<CustomAttributeData, RegisterModifyAttribute>();
        //
        //         foreach (RegisterModifyAttribute attribute in attributes)
        //             attribute.Modify(registerGroup);
        //
        //         validator.AddWithValidate(registerGroup);
        //     }
        //
        //     RegisterGroups = validator.GetRegisters();
        //
        //     _isReflected = true;
        // }
    }
}
