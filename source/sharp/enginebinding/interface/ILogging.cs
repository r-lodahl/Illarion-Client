namespace Illarion.Client.EngineBinding.Interface
{
    public interface ILogging
    {
        void Log(string message);
        void LogWarning(string message);
        void LogError(string message);
    }
}