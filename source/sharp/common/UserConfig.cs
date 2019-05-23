using Godot;
using System;

namespace Illarion.Client.Common
{
    public class UserConfig
    {
        private static UserConfig _instance;
        public static UserConfig Instance 
        {
            get {
                if (_instance == null) _instance = new UserConfig();
                return _instance;
            }
        }

        private ConfigFile configFile;

        public Language Language {get; private set;}

        private UserConfig()
        {
            configFile = new ConfigFile();
            configFile.Load(Constants.UserData.ConfigPath);

            int langVal = (int)configFile.GetValue("interface", "language", Language.English);

            if (Enum.IsDefined(typeof(Language), langVal)) 
            {
                Language = (Language) langVal;
            }
            else 
            {
                Language = Language.English;
            }



        }
    }
}
