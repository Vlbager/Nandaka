namespace Nandaka.Core.Table
{
    public enum MessageType
    {
        ReadDataRequest,
        WriteDataRequest,
        ReadDataResponse,
        WriteDataResponse,
        ErrorMessage,
        // todo: think about it.
        ApplicationDataError,
        None
    }

}
