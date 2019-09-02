

namespace Platformer.Level {

	public class Tile {

	}

	public class Level {
		private TileGrid grid;

		public Level() {
			grid = new TileGrid();
		}

		public Tile this[int x, int y] {
			get { return grid[x,y]; }
		}

	}

	public class TileGrid {
		private Tile[][] tiles;

		public TileGrid() {
			
		}

		public Tile this[int x, int y] {
			get { return tiles[y][x]; }
		}

	}

}