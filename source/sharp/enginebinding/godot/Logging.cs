using Godot;
using Illarion.Client.EngineBinding.Interface;

namespace Illarion.Client.EngineBinding.Godot 
{
    public class Logging : ILogging
    {
        public void Log(string message) => GD.Print(message);

        public void LogError(string message) => GD.PushError(message);

        public void LogWarning(string message) => GD.PushWarning(message);
    }
}