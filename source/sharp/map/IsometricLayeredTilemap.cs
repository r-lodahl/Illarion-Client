using Godot;
using System;
using System.Linq;
using Illarion.Client.Common;
using System.Collections.Generic;

namespace Illarion.Client.Map
{
	public class IsometricLayeredTilemap
	{
		private int x, y, layer;
		private int windowX, windowY;
		private int mapSizeHalfX, mapSizeHalfY;
				
		private TileIndex[] topRow;
		private TileIndex[] bottomRow;
		private TileIndex[] leftRow;
		private TileIndex[] rightRow;
		private TileIndex[] leftRowNoFirst;
		private TileIndex[] leftRowNoLast;
		private TileIndex[] rightRowNoFirst;
		private TileIndex[] rightRowNoLast;

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

			List<TileIndex> bottomTiles = new List<TileIndex>();
			List<TileIndex> topTiles = new List<TileIndex>();
			List<TileIndex> leftTiles = new List<TileIndex>();
			List<TileIndex> rightTiles = new List<TileIndex>();

			for (int ix = x-mapSizeHalfX; ix <= x+mapSizeHalfX; ix++) {
				for (int iy = y-mapSizeHalfY; iy <= y+mapSizeHalfY; iy++) {
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

					if (ix < x + mapSizeHalfX || iy < y + mapSizeHalfY)
					{
						position = new Vector2(
							ix * Constants.Tile.SizeX + Constants.Tile.SizeX * 0.5f,
							iy * (Constants.Tile.SizeY+1) + (Constants.Tile.SizeY + 1) * 0.5f
						);

						map.Add(
							new TileIndex(tileX, tileY+1),
							new Sprite[]{AddSpriteToTree(position), AddSpriteToTree(position)}
						);
					}

					if (ix == x - mapSizeHalfX) topTiles.Add(new TileIndex(ix - x, iy - y));
					if (ix == x + mapSizeHalfX) bottomTiles.Add(new TileIndex(ix - x, iy - y));
					if (iy == y - mapSizeHalfY) rightTiles.Add(new TileIndex(ix - x, iy - y));
					if (iy == y + mapSizeHalfY) rightTiles.Add(new TileIndex(ix - x, iy - y));
				}
			}

			topRow = topTiles.ToArray();
			bottomRow = bottomTiles.ToArray();
			leftRow = leftTiles.ToArray();
			rightRow = rightTiles.ToArray();

			rightRowNoFirst = rightTiles.Skip(1).ToArray();
			leftRowNoFirst = leftTiles.Skip(1).ToArray();
			
			rightRowNoLast = rightTiles.Take(rightTiles.Count - 1).ToArray();
			leftRowNoLast = leftTiles.Take(leftTiles.Count - 1).ToArray();
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
			if (change.Equals(Direction.Up))
			{
				MoveTilesUp(bottomRow);
			}
			else if (change.Equals(Direction.Down))
			{
				MoveTilesDown(topRow);
			}
			else if (change.Equals(Direction.Left))
			{
				MoveTilesLeft(rightRow);
			}
			else if (change.Equals(Direction.Right))
			{
				MoveTilesRight(leftRow);
			}
			else if (change.Equals(Direction.DownLeft))
			{
				MoveTilesDown(topRow);
				MoveTilesLeft(rightRowNoFirst);
			}
			else if (change.Equals(Direction.DownRight))
			{
				MoveTilesDown(topRow);
				MoveTilesRight(leftRowNoFirst);
			}
			else if (change.Equals(Direction.UpRight))
			{
				MoveTilesUp(bottomRow);
				MoveTilesRight(leftRowNoLast);
			}
			else if (change.Equals(Direction.UpLeft))
			{
				MoveTilesUp(bottomRow);
				MoveTilesLeft(rightRowNoLast);
			}
		}

		private void MoveTilesUp(TileIndex[] bottomTiles)
		{
			var direction = Direction.Up;

			foreach (var tileIndex in bottomTiles) 
			{
				var currentIndex = new TileIndex(tileIndex.x + x, tileIndex.y + y);
				var tile = RemoveFromMap(currentIndex);

				MoveTile(tile, 0, 2 * mapSizeHalfY * Constants.Tile.SizeY);
				
				InsertAsNewAt(tile, new TileIndex(currentIndex.y + direction.x, currentIndex.x + direction.y));
			}
		}

		private void MoveTilesDown(TileIndex[] topTiles)
		{
			var direction = Direction.Down;

			foreach (var tileIndex in topTiles) 
			{
				var currentIndex = new TileIndex(tileIndex.x + x, tileIndex.y + y);
				var tile = RemoveFromMap(currentIndex);

				MoveTile(tile, 0, -2 * mapSizeHalfY * Constants.Tile.SizeY);
				
				InsertAsNewAt(tile, new TileIndex(currentIndex.y + direction.x, currentIndex.x + direction.y));
			}
		}

		private void MoveTilesLeft(TileIndex[] rightTiles)
		{
			var direction = Direction.Left;

			foreach (var tileIndex in rightTiles) 
			{
				var currentIndex = new TileIndex(tileIndex.x + x, tileIndex.y + y);
				var tile = RemoveFromMap(currentIndex);

				MoveTile(tile, 0, -2 * mapSizeHalfX * Constants.Tile.SizeX);

				InsertAsNewAt(tile, new TileIndex(-currentIndex.y + direction.x, -currentIndex.x + direction.y));
			}
		}

		private void MoveTilesRight(TileIndex[] leftTiles)
		{
			var direction = Direction.Right;

			foreach (var tileIndex in leftTiles) 
			{
				var currentIndex = new TileIndex(tileIndex.x + x, tileIndex.y + y);
				var tile = RemoveFromMap(currentIndex);

				MoveTile(tile, 0, 2 * mapSizeHalfX * Constants.Tile.SizeX);
				
				InsertAsNewAt(tile, new TileIndex(-currentIndex.y + direction.x, -currentIndex.x + direction.y));
			}
		}

		private void InsertAsNewAt(Sprite[] tile, TileIndex index)
		{
			SetTileAppearanceOf(tile, index);
			map.Add(index, tile);
		}

		private Sprite[] RemoveFromMap(TileIndex index)
		{
			var tile = map[index];
			map.Remove(index);
			return tile;
		}

		private void MoveTile(Sprite[] tile, int movementX, int movementY)
		{
			var currentPosition = tile[0].GlobalPosition;
			var movedPostion = new Vector2(currentPosition.x + movementX, currentPosition.y + movementY);
			tile[0].GlobalPosition = movedPostion;
			tile[1].GlobalPosition = movedPostion;
		}

		private void SetTileAppearanceAt(TileIndex tileIndex)
		{
			SetTileAppearanceOf(map[tileIndex], tileIndex);
		}

		private void SetTileAppearanceOf(Sprite[] tile, TileIndex tileIndex)
		{
			TileData data = chunkLoader.GetTileDataAt(tileIndex);
			
			if (data.tileId == 0) return;

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
