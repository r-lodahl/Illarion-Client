using Godot;
using System;
using Illarion.Client.Common;
using System.Collections.Generic;

namespace Illarion.Client.Map
{
	public class IsometricLayeredTilemap
	{
		private int x, y, layer;
		private int mapSizeX, mapSizeY;
		private Node root;
		private ChunkLoader chunkLoader;
		private Dictionary<int, Sprite> map;

		public IsometricLayeredTilemap(int x, int y, int layer, IMovementSupplier movementSupplier, Node root)
		{
			this.x = x;
			this.y = y;
			this.layer = layer;
			this.root = root;

			movementSupplier.layerChanged += OnMapLayerChanged;
			movementSupplier.movementDone += OnMapCenterChanged;

			chunkLoader = new ChunkLoader(x, y, movementSupplier);

			CreateTileMap();
		}

		private void CreateTileMap()
		{
			mapSizeX = ((int)Mathf.Ceil(OS.GetWindowSize().x / Constants.Tile.SizeX)) + Constants.Map.OffscreenTileThreshold;
			mapSizeY = ((int)Mathf.Ceil(OS.GetWindowSize().y / Constants.Tile.SizeY)) + Constants.Map.OffscreenTileThreshold;
			
			int mapSizeXHalf = mapSizeX/2;
			int mapSizeYHalf = mapSizeY/2;
			
			map = new Dictionary<int,Sprite>(mapSizeXHalf*2*mapSizeYHalf*2*2);

			// TODO: This is uneven. Its even if we work until <= mapSizeHalf but SKIP the tileX+1 tiles in this row
			for (int ix = x-mapSizeXHalf; ix < x+mapSizeXHalf; ix++) {
				for (int iy = y-mapSizeYHalf; iy < y+mapSizeYHalf; iy++) {
					int tileX = ix+iy;
					int tileY = ix-iy;

					Sprite tile = new Sprite();
					tile.GlobalPosition = new Vector2(
						ix * Constants.Tile.SizeX,
						iy * Constants.Tile.SizeY);
					root.AddChild(tile);
					map.Add(ToTileIndex(tileX, tileY), tile);
					
					tile = new Sprite();
					tile.GlobalPosition = new Vector2(
						ix * Constants.Tile.SizeX + 0.5f * Constants.Tile.SizeX,
						iy * Constants.Tile.SizeY + 0.5f * Constants.Tile.SizeY);
					root.AddChild(tile);
					map.Add(ToTileIndex(tileX+1, tileY), tile);
				}
			}
		}

		private void OnMapCenterChanged(object e, Vector2i change)
		{
			// For every change direction: translate and reload the specific map stripe.

			if (change.x == -1)
			{
				
			}
		}

		private void OnMapLayerChanged(object e, int newLayer)
		{
			//TODO: Reload all tiles
		}

		// Translates a tile-node so that it will be on the other side of the screen
		// => Mirrored around the center
		private void MirrorTilePosition()
		{

		}

		private int ToTileIndex(int tx, int ty) => tx * mapSizeY + ty;
	}
}
