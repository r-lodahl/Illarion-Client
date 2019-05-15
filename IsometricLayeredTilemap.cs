using Godot;
using System;

public class IsometricLayeredTilemap : Node
{
	const int tileSizeX = 76;
	const int tileSizeY = 37;
	
	const int offScreenTileThreshold = 2;
	
	private int centerX, centerY, centerLayer;
	
	private ChunkLoader chunkLoader;
	
	
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
		// Placeholder for now
		centerX = 0;
		centerY = 0;
		centerLayer = 0;
		
		chunkLoader = new ChunkLoader(centerX, centerY, centerLayer);
		
		CreateTiles();
    }
	
	private void CreateTiles()
	{
		int mapSizeXHalf = ((int)Mathf.Ceil(OS.GetWindowSize().x / tileSizeX))/2 + offScreenTileThreshold;
		int mapSizeYHalf = ((int)Mathf.Ceil(OS.GetWindowSize().y / tileSizeY))/2 + offScreenTileThreshold;
		
		for (int x = centerX-mapSizeXHalf; x < centerX+mapSizeXHalf; x++) {
			for (int y = centerY-mapSizeYHalf; y < centerY+mapSizeYHalf; y++) {
				Sprite tile = new Sprite();
				tile.GlobalPosition = new Vector2(x * tileSizeX, y * tileSizeY);
				AddChild(tile);
				
				tile = new Sprite();
				tile.GlobalPosition = new Vector2(x * tileSizeX + 0.5f * tileSizeX, y * tileSizeY + 0.5f * tileSizeY);
				AddChild(tile);
			}
		}
		
	}
	

//  // Called every frame. 'delta' is the elapsed time since the previous frame.
//  public override void _Process(float delta)
//  {
//      
//  }
}
