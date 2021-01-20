using Nandaka.Core.Helpers;
using Nandaka.Core.Session;
using Nandaka.Core.Util;
using Nandaka.MilliGanjubus.Models;

namespace Nandaka.MilliGanjubus.Utils
{
    public static class ErrorTypeConverter
    {
        private static readonly Map<ErrorType, MgErrorType> Map;
        
        static ErrorTypeConverter()
        {
            Map = new Map<ErrorType, MgErrorType>
            {
                { ErrorType.InvalidMetaData, MgErrorType.WrongGByte },
                { ErrorType.InvalidAddress, MgErrorType.WrongRegisterAddress },
                { ErrorType.TooMuchDataRequested, MgErrorType.WrongDataAmount }
            };
        }
        
        public static MgErrorType? Convert(this ErrorType self)
        {
            if (!Map.Forward.Contains(self))
                return null;
            
            return Map.Forward[self];
        }

        public static ErrorType? Convert(this MgErrorType self)
        {
            if (!Map.Reverse.Contains(self))
                return null;
            
            return Map.Reverse[self];
        }
    }
}