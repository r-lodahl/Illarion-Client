public struct TileData
{
    public readonly int tileId;
    public readonly int overlayId;
    public readonly int layer;

    public TileData(int tileId, int overlayId, int layer) 
    {
        this.tileId = tileId;
        this.overlayId = overlayId;
        this.layer = layer;
    }
}