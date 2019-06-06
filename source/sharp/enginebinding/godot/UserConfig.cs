using System;
using Godot;
using Illarion.Client.Common;
using Illarion.Client.EngineBinding.Interface;

namespace Illarion.Client.EngineBinding.Godot
{
    public class UserConfig : IUserConfig
    {
        public Language Language {get; private set;}

        private readonly ConfigFile configFile;

        public UserConfig()
        {
            configFile = new ConfigFile();
            configFile.Load(Constants.UserData.ConfigPath);

            int langVal = (int)configFile.GetValue("interface", "language", Language.English);

            if (Enum.IsDefined(typeof(Language), langVal)) Language = (Language) langVal;
            else Language = Language.English;
        }
    }
}