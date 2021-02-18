namespace Nandaka.Core.Session
{
    public sealed class NullSessionHandler : ISessionHandler 
    {
        public void ProcessNext()
        {
            //empty
        }

        public void Dispose()
        {
            //empty
        }
    }
}