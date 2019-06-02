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
		private Dictionary<TileIndex, Sprite[]> map;
		
		private ChunkLoader chunkLoader;
		private TileSet tileset;

		public IsometricLayeredTilemap(int x, int y, int layer, IMovementSupplier movementSupplier, Node root, TileSet tileset)
		{
			this.x = x;
			this.y = y;
			this.layer = layer;
			this.root = root;
			this.windowX = 1140;//(int) OS.GetWindowSize().x; DBG
			this.windowY = 740;//(int) OS.GetWindowSize().y; DBG
			this.tileset = tileset;

			movementSupplier.LayerChanged += OnMapLayerChanged;
			movementSupplier.MovementDone += OnMapCenterChanged;

			chunkLoader = new ChunkLoader(x, y, movementSupplier);

			CreateTileMap();
			ReloadMap();
		}

		public int GetZScore(int x, int y, int layer)
		{
			return layer * Constants.Map.LayerZScoreFactor + y - x;
		}

		private void CreateTileMap()
		{
			mapSizeHalfX = (((int)Mathf.Ceil(windowX / Constants.Tile.SizeX)) + Constants.Map.OffscreenTileThreshold)/2;
			mapSizeHalfY = (((int)Mathf.Ceil(windowY / Constants.Tile.SizeY)) + Constants.Map.OffscreenTileThreshold)/2;
			
			map = new Dictionary<TileIndex, Sprite[]>(mapSizeHalfX*2*mapSizeHalfY*2*2);

			// TODO: This is uneven. Its even if we work until <= mapSizeHalf but SKIP the tileX+1 tiles in this row
			for (int ix = x-mapSizeHalfX; ix < x+mapSizeHalfX; ix++) {
				for (int iy = y-mapSizeHalfY; iy < y+mapSizeHalfY; iy++) {
					int tileX = x-y+ix-iy;
					int tileY = x+y+ix+iy;

					Vector2 position = new Vector2(
						ix * Constants.Tile.SizeX,
						iy * (Constants.Tile.SizeY+1)
					);

					map.Add(
						new TileIndex(tileX, tileY),
						new Sprite[]{AddSpriteToTree(position), AddSpriteToTree(position)}
					);

					position = new Vector2(
						ix * Constants.Tile.SizeX + Constants.Tile.SizeX * 0.5f,
						iy * (Constants.Tile.SizeY+1) + (Constants.Tile.SizeY + 1) * 0.5f
					);

					map.Add(
						new TileIndex(tileX, tileY+1),
						new Sprite[]{AddSpriteToTree(position), AddSpriteToTree(position)}
					);
				}
			}
		}

		private Sprite AddSpriteToTree(Vector2 screenPosition)
		{
			Sprite sprite = new Sprite();
			sprite.GlobalPosition = screenPosition;
			sprite.RegionEnabled = true;
			sprite.Texture = tileset.TileGetTexture(0);
			sprite.ZAsRelative = false;
			root.AddChild(sprite);
			return sprite;
		}


		private void OnWindowSizeChanged(object e) 
		{
			/*windowX = (int) OS.GetWindowSize().x;
			windowY = (int) OS.GetWindowSize().y;

			DeleteTileMap();
			CreateTileMap();
			ReloadMap(); DBG*/
		}

		private void DeleteTileMap()
		{
			foreach (var tile in map.Values)
			{
				root.RemoveChild(tile[0]);
				root.RemoveChild(tile[1]);
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
				// TopRight Tile Coordinate
				int ix = x - y + (x + mapSizeHalfX-1) - (y-mapSizeHalfY);
				int iy = x + y + (x + mapSizeHalfX-1) + (y-mapSizeHalfY);
				
				//Goal: Bottom Right
				int goalY = x + y + (x + mapSizeHalfX-1) + (y + mapSizeHalfY);

				while (iy < goalY)
				{
					translationTiles.Add(new TileIndex(ix, iy));
					ix--;
					iy++;
				}
			}
			// Right: Remove Lefts
			else if (change.x != -1 && change.y != -1)
			{
				// TopLeft Tile Coordinate
				int ix = x - y + (x - mapSizeHalfX) - (y-mapSizeHalfY);
				int iy = x + y + (x - mapSizeHalfX) + (y-mapSizeHalfY);
				
				// Goal: Bottom Left
				int goalY = x + y + (x - mapSizeHalfX) + (y + mapSizeHalfY);

				while (iy < goalY)
				{
					translationTiles.Add(new TileIndex(ix, iy));
					ix--;
					iy++;
				}
			}

			// Down: Remove Ups
			if (change.x != -1 && change.y != 1)
			{
				// TopLeft Tile Coordinate
				int ix = x - y + (x - mapSizeHalfX) - (y-mapSizeHalfY);
				int iy = x + y + (x - mapSizeHalfX) + (y-mapSizeHalfY);

				// Goal: Top Right
				int goalX = x - y + (x + mapSizeHalfX) - (y - mapSizeHalfY);

				while (ix < goalX)
				{
					translationTiles.Add(new TileIndex(ix, iy));
					ix++;
					iy++;
				}
			}
			// Up: Remove Downs
			else if (change.x != 1 && change.y != -1)
			{
				// BottomLeft Tile Coordinate
				int ix = x - y + (x - mapSizeHalfX) - (y+mapSizeHalfY-1);
				int iy = x + y + (x - mapSizeHalfX) + (y+mapSizeHalfY-1);
				
				// Goal: Bottom Right
				int goalX = x - y + (x + mapSizeHalfX) - (y+mapSizeHalfY-1);


				while (ix < goalX)
				{
					translationTiles.Add(new TileIndex(ix, iy));
					ix++;
					iy++;
				}
			}

			foreach (var tileIndex in translationTiles)
			{
				var movedIndex = MoveTileToOpposedDirection(tileIndex);
				SetTileAppearanceAt(movedIndex);	
			}
		}

		private TileIndex MirrorTilePositionAroundCenter(TileIndex tileIndex, Vector2i direction) 
		{
			var tileSprite = map[tileIndex];
			map.Remove(tileIndex);

			int movedTilePositionX = tileIndex.x + (direction.x * 2 * mapSizeHalfX);
			int movedTilePositionY = tileIndex.y + (direction.y * 2 * mapSizeHalfY);

			TileIndex movedIndex = new TileIndex(-tileIndex.x, -tileIndex.y);

			map[movedIndex] = tileSprite;

			Vector2 movedPosition = new Vector2(
				windowX - tileSprite[0].GlobalPosition.x,
				windowY - tileSprite[0].GlobalPosition.y
			); 

			tileSprite[0].GlobalPosition = movedPosition;
			tileSprite[1].GlobalPosition = movedPosition;

			return movedIndex;
		}

		private void SetTileAppearanceAt(TileIndex tileIndex)
		{
			TileData data = chunkLoader.GetTileDataAt(tileIndex);
			
			if (data.tileId == 0) return;

			var tile = map[tileIndex];

			tile[0].RegionRect = tileset.TileGetRegion(data.tileId);
			tile[1].RegionRect = tileset.TileGetRegion(data.overlayId);

			int zScore = GetZScore(tileIndex.x, tileIndex.y, data.layer);
			tile[0].ZIndex = zScore;
			tile[1].ZIndex = zScore + Constants.Tile.OverlayZScore;
		}


		private void OnMapLayerChanged(object e, int newLayer)
		{
			ReloadMap();
		}

		private void ReloadMap()
		{
			foreach (var tileIndex in map.Keys) SetTileAppearanceAt(tileIndex);
		}
	}
}
