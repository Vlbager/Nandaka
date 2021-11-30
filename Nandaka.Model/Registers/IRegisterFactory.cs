namespace Nandaka.Model.Registers
{
    public interface IRegisterFactory
    {
        IReadOnlyRegister<T> CreateReadOnly<T>(int address, T defaultValue = default) 
            where T: struct;

        IRegister<T> Create<T>(int address, T defaultValue = default)
            where T: struct;
    }
}