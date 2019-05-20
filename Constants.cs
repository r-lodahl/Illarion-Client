public static class Constants {
    public static class Tile {
         public const int OverlayFactor = 1000;
    }

    public static class Map {
        public const int Chunksize = 20;

    }

    public static class Server {
        public const int BaseIdMask = 0x001F;
        public const int OverlayIdMask = 0x03E0;
        public const int ShapeIdMask = 0xFC00;
        public const string UpdateServerAddress = "https://c107-243.cloud.gwdg.de";
    }

    public static class TableData {
        public const int TileIdColumn = 9;
        public const int OverlayIdColumn = 2;
        public const int TileNameColumn = 3;
        public const int OverlayNameColumn = 3;
    }
}