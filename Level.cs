
using Platformer.Data;
using Platformer.Render;

namespace Platformer.Level {

	public class Tile {
		private StaticSpriteReference imgref;

		public Tile(StaticSpriteReference img) {
			imgref = img;
		}

	}

	public class Level : Identifiable {
		private TileGrid grid;

		public Level() {
			grid = new TileGrid();
		}

		public Tile this[int x, int y] {
			get { return grid[x,y]; }
		}

		public IdentityNumber GetID() { return new IdentityNumber(1); }

	}

	public class TileGrid {
		private Tile[][] tiles;

		public TileGrid() {
			throw new System.NotImplementedException();
		}

		public Tile this[int x, int y] {
			get { return tiles[y][x]; }
		}

	}

}