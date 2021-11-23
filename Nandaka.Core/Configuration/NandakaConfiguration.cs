namespace Nandaka.Core
{
    public static class NandakaConfiguration
    {
        public static LogConfiguration Log { get; }
        
        static NandakaConfiguration()
        {
            Log = new LogConfiguration();
        }
    }
}