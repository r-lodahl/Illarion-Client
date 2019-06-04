namespace Illarion.Client.Common
{
    public static class Direction
    {
        public static readonly Vector2i Up = new Vector2i(1, -1);
        public static readonly Vector2i UpRight = new Vector2i(1, 0);
        public static readonly Vector2i UpLeft = new Vector2i(0, -1);
        public static readonly Vector2i Right = new Vector2i(1, 1);
        public static readonly Vector2i Left = new Vector2i(-1, -1);
        public static readonly Vector2i DownRight = new Vector2i(0, 1);
        public static readonly Vector2i DownLeft = new Vector2i(-1, 0);
        public static readonly Vector2i Down = new Vector2i(-1, 1);
    }
}
