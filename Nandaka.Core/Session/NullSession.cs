namespace Nandaka.Core.Session
{
    public sealed class NullSession : ISession 
    {
        public void ProcessNext()
        {
            //empty
        }
    }
}