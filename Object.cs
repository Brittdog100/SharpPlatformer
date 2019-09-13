using System.Numerics;

using Microsoft.Graphics.Canvas;
using Windows.Foundation;

using Platformer.Data;
using Platformer.Geometry;
using Platformer.Render;

namespace Platformer.Object {

	/// <summary>
	/// Outlines an entity in the game.
	/// </summary>
	public abstract class Entity : Identifiable {
		public readonly IdentityNumber Identity;
		public Vector2 Velocity;
		protected BoundingBox boundingbox;
		public Coordinate Position {
			get { return new Coordinate(boundingbox.X, boundingbox.Y); }
			set {
				boundingbox.X = value.X;
				boundingbox.Y = value.Y;
			}
		}
		public double X {
			get { return Position.X; }
			set { boundingbox.X = value; }
		}
		public double Y {
			get { return Position.Y; }
			set { boundingbox.Y = value; }
		}

		public Entity(IdentityNumber id) { Identity = id; }

		public void DoVelocity() { Position += Velocity; }

		public IdentityNumber GetID() { return Identity; }

		public abstract void Render(CanvasDrawingSession session);

	}

	/// <summary>
	/// Represents a player-controlled entity.
	/// </summary>
	public sealed class Player : Entity, Directional, Identifiable {
		private SpriteSheet sprites;
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
			boundingbox = new BoundingBox(0, 0, 32, 48);
		}

		public Direction GetFacing() { return _dir ? Direction.LEFT : Direction.RIGHT; }

		public override void Render(CanvasDrawingSession session) { session.DrawImage(sprites, boundingbox); }

		public static implicit operator SpriteSheet(Player p) { return p.sprites; }

	}

}