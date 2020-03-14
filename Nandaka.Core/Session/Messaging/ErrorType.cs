namespace Nandaka.Core.Session
{
    public enum ErrorType
    {
        // temp: 0 - no error
        InvalidRegisterAddress = 1,
        TooMuchDataRequested,
        SpecificCodeNotDefined,
        InternalProtocolError
    }

}