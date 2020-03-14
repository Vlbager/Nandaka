namespace Nandaka.Core.Table
{
    public interface IRegister
    {
        int Address { get; }

        /// <summary>
        /// Convert register value in little-endianness bytes
        /// </summary>
        byte[] ToBytes();
    }
}
