namespace Illarion.Client.Common {
    public static class Constants {
        public static class Tile {
            public const int OverlayFactor = 1000;
            public const int SizeX = 76;
            public const int SizeY = 37;
        }

        public static class Map {
            public const int Chunksize = 20;
            public const int VisibleLayers = 10;
            public const int OffscreenTileThreshold = 1;
        }

        public static class UserData {
            public const string VersionPath = "user://map.version";
            public const string TilesetPath = "res://assets/tileset/tiles.res";
            public const string TileTablePath = "res://assets/tileset/tiles.tbl";
            public const string OverlayTablePath = "res://assets/tileset/overlays.tbl";
            public const string MapPath = "/Illarion-Map";
            public const string MainScene = "res://world.tscn";
            public const string TileFileName = "tilelist.bin";
            public const string OverlayFileName = "overlaylist.bin";
            public const string ConfigPath = "user://illarion.cfg";
        }

        public static class Server {
            public const int BaseIdMask = 0x001F;
            public const int OverlayIdMask = 0x03E0;
            public const int ShapeIdMask = 0xFC00;

            public const int UpdateServerPort = 443;
            public const string UpdateServerAddress = "https://c107-243.cloud.gwdg.de";

            public const string ZippedMapEndpoint = "/api/map/zipball";
            public const string MapVersionEndpoint = "/api/map/version";
        }

        public static class TableData {
            public const int TileIdColumn = 9;
            public const int OverlayIdColumn = 2;
            public const int TileNameColumn = 3;
            public const int OverlayNameColumn = 3;
        }
    }
}
