using System;
using Illarion.Client.EngineBinding.Interface.Net;

namespace Illarion.Client.EngineBinding.Interface
{
    public static class Game
    {
        private static bool initialized;

        private static IFileSystem _fileSystem;
        private static ILogging _logger;
        private static IMath _math;
        private static IGraphics _graphics;
        private static IHttpFactory _httpFactory;
        private static IUserConfig _userConfig;

        public static IFileSystem FileSystem
        {
            get
            {
                if (!initialized) throw new InvalidOperationException("Engine Binding not yet initialized.");
                return _fileSystem;
            }
        }

        public static ILogging Logger
        {
            get
            {
                if (!initialized) throw new InvalidOperationException("Engine Binding not yet initialized.");
                return _logger;
            }
        }

        public static IMath Math
        {
            get
            {
                if (!initialized) throw new InvalidOperationException("Engine Binding not yet initialized.");
                return _math;
            }
        }

        public static IGraphics Graphics
        {
            get
            {
                if (!initialized) throw new InvalidOperationException("Engine Binding not yet initialized.");
                return _graphics;
            }
        }

        public static IHttpFactory HttpFactory
        {
            get
            {
                if (!initialized) throw new InvalidOperationException("Engine Binding not yet initialized.");
                return _httpFactory;
            }
        }

        public static IUserConfig UserConfig
        {
            get
            {
                if (!initialized) throw new InvalidOperationException("Engine Binding not yet initialized.");
                return _userConfig;
            }
        }

        public static void Initialize(IFileSystem fileSystem, ILogging logger, IMath math, IGraphics graphics, IHttpFactory httpFactory, IUserConfig userConfig)
        {
            _fileSystem = fileSystem;
            _logger = logger;
            _math = math;
            _graphics = graphics;
            _httpFactory = httpFactory;
            _userConfig = userConfig;
        }
    }
}