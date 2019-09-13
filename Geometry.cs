using System;
using System.Numerics;

using Microsoft.Graphics.Canvas;
using Windows.Foundation;

using Platformer.Data;
using Platformer.Data.IO;
using Platformer.Data.Struct;
using Platformer.Render;

namespace Platformer.Geometry {

	public struct Coordinate {
		public double X, Y;

		public Coordinate(Point p) : this(p.X, p.Y) { }
		public Coordinate(double x, double y) {
			X = x;
			Y = y;
		}

		public static Coordinate operator +(Coordinate a, Coordinate b) { return new Coordinate(a.X + b.X, a.Y + b.Y); }
		public static Coordinate operator +(Coordinate a, (double,double) b) { return new Coordinate(a.X + b.Item1, a.Y + b.Item2); }
		public static Coordinate operator +(Coordinate a, Vector2 b) { return new Coordinate(a.X + b.X, a.Y + b.Y); }
		public static Coordinate operator -(Coordinate a, Coordinate b) { return new Coordinate(a.X - b.X, a.Y - b.Y); }
		public static Coordinate operator -(Coordinate a, (double,double) b) { return new Coordinate(a.X - b.Item1, a.Y - b.Item2); }
		public static Coordinate operator -(Coordinate a, Vector2 b) { return new Coordinate(a.X - b.X, a.Y - b.Y); }

		public static implicit operator Point(Coordinate c) { return new Point(c.X, c.Y); }

	}

	public struct BoundingBox {
		public double X, Y, Width, Height;

			public BoundingBox(double x, double y, double w, double h) {
				X = x;
				Y = y;
				Width = w;
				Height = h;
			}
			public BoundingBox(Coordinate o, double w, double h) : this(o.X, o.Y, w, h) { }
			public BoundingBox(Coordinate o, Vector2 s) : this(o.X, o.Y, s.X, s.Y) { }

			public bool Contains(Coordinate p) {
				return ((p.X > X) && (p.X < (X + Width))) && ((p.Y > Y) && (p.Y < (Y + Height)));
			}

			public static BoundingBox operator +(BoundingBox b, Vector2 t) { return new BoundingBox(b.X + t.X, b.Y + t.Y, b.Width, b.Height); }
			public static BoundingBox operator *(BoundingBox b, double m) { return new BoundingBox(b.X, b.Y, b.Width * m, b.Height * m); }
			public static BoundingBox operator *(BoundingBox b, float m) { return new BoundingBox(b.X, b.Y, b.Width * m, b.Height * m); }
			public static BoundingBox operator *(BoundingBox b, Vector2 m) { return new BoundingBox(b.X, b.Y, b.Width * m.X, b.Height * m.Y); }

			public static implicit operator Rect(BoundingBox b) { return new Rect(b.X, b.Y, b.Width, b.Height); }

	}

	public enum TileBehavior {
		EMPTY = 0,
		COLLIDE = 0b1,
		SLOW = 0b10,
		BOUNCE = 0b100,
		SWIM = 0b1000,
		DAMAGE = 0b1_0000,
		FAST = 0b10_0000
	}

	public class Tile : ShortIdentifiable {
		private TextureReference imgref;
		public TileBehavior Behavior { get; private set; }
		public readonly ShortIdentity Identity;

		/// <summary>
		/// This might look weird, but this creates an air tile.
		/// </summary>
		internal Tile() { Identity = new ShortIdentity(0); }
		public Tile(TextureReference img, ShortIdentity id) {
			imgref = img;
			Behavior = 0;
			Identity = id;
		}
		public Tile(TextureReference img, TileBehavior behavior, ShortIdentity id) {
			imgref = img;
			Behavior = behavior;
			Identity = id;
		}

		public ShortIdentity GetID() { return Identity; }

		public static implicit operator CanvasBitmap(Tile t) { return t.imgref.ToTexture(); }

	}

	public class Level : Identifiable {
		private TileGrid grid;
		public short Width { get { return grid.Width; } }
		public short Height { get { return grid.Height; } }

		public Level() {
			grid = new TileGrid();
		}

		public Tile this[ushort x, ushort y] {
			get { return grid[x,y]; }
		}

		public IdentityNumber GetID() { return new IdentityNumber(1); }

		public void Render(CanvasDrawingSession sender) {
			for(int x = (int)Math.Floor(Database.Camera.X - 1); x < Math.Ceiling(Database.Camera.X + 1) && x < Width; x++) {
				if(x < 0)
					continue;
				for(int y = (int)Math.Floor(Database.Camera.Y - 1); y < Math.Ceiling(Database.Camera.Y + 1) && y < Height; y++) {
					if(y < 0)
						continue;
					//TODO
					sender.DrawImage((CanvasBitmap)grid[(ushort)x,(ushort)y]);
				}
			}
		}

	}

	public class TileGrid {
		private Tile[][] tiles;
		public readonly Package Package;
		public readonly short Width;
		public readonly short Height;

		public TileGrid() {
			throw new System.NotImplementedException();
		}
		public TileGrid(Tile[][] t, Package p) {
			Package = p;
			tiles = t;
		}

		public Tile this[ushort x, ushort y] {
			get { return tiles[x][y]; }
		}

		public static TileGrid FromFile(AppDataFile file, Package p) {
			var stream = file.Load();
			int w1 = stream.ReadByte();
			int w0 = stream.ReadByte();
			int h1 = (byte)stream.ReadByte();
			int h0 = (byte)stream.ReadByte();
			ushort width = (ushort)((w1 * 0x100) + w0);
			ushort height = (ushort)((h1 * 0x100) + h0);
			Tile[][] grid = new Tile[width][];
			for(int x = 0; x < width; x++){
				grid[x] = new Tile[height];
				for(int y = 0; y < height; y++)
					grid[x][y] = Database.GetTile(p, (byte)stream.ReadByte());
			}	
			return new TileGrid(grid, p);
		}

	}

}