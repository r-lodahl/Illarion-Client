using Godot;
using System;
using Illarion.Client.Common;
using System.Collections.Generic;

namespace Illarion.Client.Map
{
	public class IsometricLayeredTilemap
	{
		private int x, y, layer;
		private int windowX, windowY;
		private int mapSizeHalfX, mapSizeHalfY;
				
		private Node root;
		private Dictionary<TileIndex, Sprite> map;

		private ChunkLoader chunkLoader;
		private TileSet tileset;

		public IsometricLayeredTilemap(int x, int y, int layer, IMovementSupplier movementSupplier, Node root, TileSet tileset)
		{
			this.x = x;
			this.y = y;
			this.layer = layer;
			this.root = root;
			this.windowX = (int) OS.GetWindowSize().x;
			this.windowY = (int) OS.GetWindowSize().y;

			movementSupplier.layerChanged += OnMapLayerChanged;
			movementSupplier.movementDone += OnMapCenterChanged;

			chunkLoader = new ChunkLoader(x, y, movementSupplier);

			CreateTileMap();
			ReloadMap();
		}

		private void CreateTileMap()
		{
			mapSizeHalfX = (((int)Mathf.Ceil(windowX / Constants.Tile.SizeX)) + Constants.Map.OffscreenTileThreshold)/2;
			mapSizeHalfY = (((int)Mathf.Ceil(windowY / Constants.Tile.SizeY)) + Constants.Map.OffscreenTileThreshold)/2;
			
			map = new Dictionary<TileIndex, Sprite>(mapSizeHalfX*2*mapSizeHalfY*2*2);

			// TODO: This is uneven. Its even if we work until <= mapSizeHalf but SKIP the tileX+1 tiles in this row
			for (int ix = x-mapSizeHalfX; ix < x+mapSizeHalfX; ix++) {
				for (int iy = y-mapSizeHalfY; iy < y+mapSizeHalfY; iy++) {
					int tileX = x+ix+iy;
					int tileY = y+ix-iy;

					Sprite tile = new Sprite();
					tile.GlobalPosition = new Vector2(
						ix * Constants.Tile.SizeX,
						iy * Constants.Tile.SizeY);
					root.AddChild(tile);
					map.Add(new TileIndex {x = tileX, y = tileY}, tile);

					tile = new Sprite();
					tile.GlobalPosition = new Vector2(
						ix * Constants.Tile.SizeX + 0.5f * Constants.Tile.SizeX,
						iy * Constants.Tile.SizeY + 0.5f * Constants.Tile.SizeY);
					root.AddChild(tile);
					map.Add(new TileIndex {x = tileX+1, y= tileY}, tile);
				}
			}
		}

		private void OnWindowSizeChanged(object e) 
		{
			windowX = (int) OS.GetWindowSize().x;
			windowY = (int) OS.GetWindowSize().y;

			DeleteTileMap();
			CreateTileMap();
			ReloadMap();
		}

		private void DeleteTileMap()
		{
			foreach (var tile in map.Values)
			{
				root.RemoveChild(tile);
			}

			map.Clear();

			GC.Collect();
		}


		private void OnMapCenterChanged(object e, Vector2i change)
		{
			// For every change direction: translate and reload the specific map stripe.
			HashSet<TileIndex> translationTiles = new HashSet<TileIndex>();

			// Left: Remove Rights
			if (change.x != 1 && change.y != 1)
			{
				int ix = x + mapSizeHalfX + y + mapSizeHalfY;
				int iy = 0;

				while (ix >= 0)
				{
					translationTiles.Add(new TileIndex {x = ix, y= iy});
					ix--;
					iy++;
				}
			}
			// Right: Remove Lefts
			else if (change.x != -1 && change.y != -1)
			{
				int ix = x - mapSizeHalfX + y - mapSizeHalfY; 
				int iy = 0;

				while (ix <= 0)
				{
					translationTiles.Add(new TileIndex {x = ix, y= iy});
					ix++;
					iy--;
				}
			}

			// Down: Remove Ups
			if (change.x != -1 && change.y != 1)
			{
				int ix = x - mapSizeHalfX + y - mapSizeHalfY;
				int iy = 0;

				while (ix <= 0)
				{
					translationTiles.Add(new TileIndex {x = ix, y= iy});
					ix++;
					iy++;
				}
			}
			// Up: Remove Downs
			else if (change.x != 1 && change.y != -1)
			{
				int ix = x + mapSizeHalfX + y + mapSizeHalfY;
				int iy = 0;

				while (ix >= 0)
				{
					translationTiles.Add(new TileIndex {x = ix, y= iy});
					ix--;
					iy--;
				}
			}

			foreach (var tileIndex in translationTiles)
			{
				var movedIndex = MoveTileToOpposedDirection(tileIndex);
				SetTileSprite(movedIndex);	
			}
		}

		private TileIndex MoveTileToOpposedDirection(TileIndex tileIndex) 
		{
			var tileSprite = map[tileIndex];
			map.Remove(tileIndex);

			TileIndex movedIndex = new TileIndex {x = -tileIndex.x, y = -tileIndex.y};

			map[movedIndex] = tileSprite;
			tileSprite.GlobalPosition = new Vector2(windowX - tileSprite.GlobalPosition.x, windowY - tileSprite.GlobalPosition.y); 

			return movedIndex;
		}

		private void SetTileSprite(TileIndex tileIndex)
		{
			map[tileIndex].Texture = tileset.TileGetTexture(chunkLoader.GetTileIdAt(tileIndex, layer));
		}


		private void OnMapLayerChanged(object e, int newLayer)
		{
			ReloadMap();
		}

		private void ReloadMap()
		{
			foreach (var element in map)
			{
				
			}
		}
	}
}
