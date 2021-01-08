using System.Threading;

namespace Nandaka.Core.Helpers
{
    internal static class OtherExtensions
    {
        public static T GetWithReadLock<T>(this ReaderWriterLockSlim rwLock, ref T value)
            where T: struct
        {
            rwLock.EnterReadLock();
            T returnValue = value;
            rwLock.ExitReadLock();
            return returnValue;
        }
    }
}