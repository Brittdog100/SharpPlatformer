using System.Numerics;

using Microsoft.Graphics.Canvas;
using Windows.Foundation;

using Platformer.Data;
using Platformer.Data.IO;
using Platformer.Data.Struct;
using Platformer.Error;
using Platformer.Geometry;
using Platformer.Render;

namespace Platformer.Object {

    /// <summary>
    /// Outlines an entity in the game.
    /// </summary>
    public abstract class Entity : Identifiable {
		public IdentityNumber Identity {
			get;
			internal set;
		}
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
		public void SetID(IdentityNumber newIdentity) { Identity = newIdentity; }

		public abstract void Render(CanvasDrawingSession session);

	}

	/// <summary>
	/// Represents the player-controlled entity.
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

		private Player() : base(new IdentityNumber(0)) { }
		public Player(SpriteSheet sheet) : base(new IdentityNumber(0)){
			sprites = sheet;
			boundingbox = new BoundingBox(0, 0, 32, 48);
		}

		public Direction GetFacing() { return _dir ? Direction.LEFT : Direction.RIGHT; }

		public override void Render(CanvasDrawingSession session) { session.DrawImage(sprites, boundingbox); }
		new public void SetID(IdentityNumber newIdentity) { throw new ImmutableIdentityException(typeof(Player)); }

		public static Player FromDataMap(DataMap map) {
			DataMap.CheckForSet(map, "bounds", "sprites");
			Player output = new Player();
			output.boundingbox = (BoundingBox)map["bounds"].Data;
			output.sprites = (SpriteSheet)map["sprites"].Data;
			return output;
		}

		public static implicit operator SpriteSheet(Player p) { return p.sprites; }

	}

}