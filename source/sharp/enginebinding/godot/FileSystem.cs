using Godot;
using Illarion.Client.EngineBinding.Interface;

namespace Illarion.Client.EngineBinding.Godot
{
    public class FileSystem : IFileSystem
    {
        public string GetUserDirectory() => OS.GetUserDataDir();
    }
}