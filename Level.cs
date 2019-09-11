
using Platformer.Data;
using Platformer.Render;

namespace Platformer.Level {

	public enum TileBehavior {
		EMPTY = 0,
		COLLIDE = 0b1,
		SLOW = 0b10,
		BOUNCE = 0b100,
		SWIM = 0b1000,
		DAMAGE = 0b1_0000
	}

	public class Tile {
		private TextureReference imgref;
		public TileBehavior Behavior { get; private set; }

		public Tile(TextureReference img) {
			imgref = img;
			Behavior = 0;
		}
		public Tile(TextureReference img, TileBehavior behavior) {
			imgref = img;
			Behavior = behavior;
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