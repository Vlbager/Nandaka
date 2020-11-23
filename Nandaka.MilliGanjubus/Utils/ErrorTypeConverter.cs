using Nandaka.Core.Helpers;
using Nandaka.Core.Session;
using Nandaka.MilliGanjubus.Models;

namespace Nandaka.MilliGanjubus.Utils
{
    public static class ErrorTypeConverter
    {
        private static readonly Map<ErrorType, MilliGanjubusErrorType> Map;
        
        static ErrorTypeConverter()
        {
            Map = new Map<ErrorType, MilliGanjubusErrorType>
            {
                { ErrorType.InvalidMetaData, MilliGanjubusErrorType.WrongGByte },
                { ErrorType.InvalidAddress, MilliGanjubusErrorType.WrongRegisterAddress },
                { ErrorType.TooMuchDataRequested, MilliGanjubusErrorType.WrongDataAmount }
            };
        }
        
        public static MilliGanjubusErrorType? Convert(this ErrorType self)
        {
            if (!Map.Forward.Contains(self))
                return null;
            
            return Map.Forward[self];
        }

        public static ErrorType? Convert(this MilliGanjubusErrorType self)
        {
            if (!Map.Reverse.Contains(self))
                return null;
            
            return Map.Reverse[self];
        }
    }
}