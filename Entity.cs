using System.Numerics;

using Microsoft.Graphics.Canvas;
using Windows.Foundation;

using Platformer.Data;
using Platformer.Render;

namespace Platformer.Object {

	public sealed class Player : Entity, Directional {
		private SpriteSheet sprites;
		public Vector2 Velocity = new Vector2(0,0);
		private bool _dir = false;
		public bool Facing {
			get { return _dir; }
			set {
				if(value == _dir)
					return;
				_dir = value;
				sprites.Reverse = _dir;
			}
		}
		private Rect boundingbox;
		public Point Position {
			get { return new Point(boundingbox.X,boundingbox.Y); }
			set {
				boundingbox.X = value.X;
				boundingbox.Y = value.Y;
			}
		}
		public double X {
			get { return Position.X; }
			set { Position = new Point(value,Position.Y); }
		}
		public double Y {
			get { return Position.Y; }
			set { Position = new Point(Position.X, value); }
		}
		private byte _state = 0;
		public byte State {
			get { return _state; }
			set {
				if(value == _state)
					return;
				_state = value;
				sprites.State = _state;
			}
		}
		public double Height { get { return boundingbox.Height; } }
		public double Width { get { return boundingbox.Width; } }

		public Player(SpriteSheet sheet) : base(new IdentityNumber(0)){
			sprites = sheet;
			boundingbox = new Rect(new Point(0,0),new Point(32,48));
		}

		public Direction GetFacing() { return _dir ? Direction.LEFT : Direction.RIGHT; }

		public void DoVelocity() { Position = new Point(X + Velocity.X, Y + Velocity.Y); }

		public void Render(CanvasDrawingSession session) { session.DrawImage(sprites, boundingbox); }

		public static implicit operator SpriteSheet(Player p) { return p.sprites; }

	}

	/*public class ScriptedEntity : Entity, Directional {
		private SpriteSheet sprites;
		public Vector2 Velocity { get; private set; }
		private Direction dir;

	public Direction GetFacing() { return dir; }

	}*/

}