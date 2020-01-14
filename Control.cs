using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Input;

using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

using Platformer.Error;

namespace Platformer {

	public sealed partial class MainPage : Page {
		public static int floor = 500;

		private void keyPressed(CoreWindow sender, KeyEventArgs args) { Input.Logic.Key[args.VirtualKey] = true; }
		private void keyReleased(CoreWindow sender, KeyEventArgs args) { Input.Logic.Key[args.VirtualKey] = false; }

	}

}

namespace Platformer.Input {

	public static class Logic {
		public static KeyMap Key = new KeyMap();
		private static float
			movemodifier,
			jumparcmodifier,
			gravity,
			jumpforce,
			friction;

		static Logic() {
			Data.IO.DataMap dat = new Data.IO.DataMap(Core.MainPackage,new Data.IO.AppDataFile(@"asset\testcontrols.dat"));
			movemodifier = (float)dat["movement"].Data;
			jumparcmodifier = (float)dat["forcejumpx"].Data;
			gravity = (float)dat["gravity"].Data;
			jumpforce = (float)dat["forcejumpy"].Data;
			friction = (float)dat["friction"].Data;
		}

		public static void Flow() {
			Geometry.RenderCoordinate pos = Core.Player.Position.InScreenSpace();
			float floorpos = MainPage.floor - ((float)Core.Player.Height * 16 * Core.RenderScale);
			bool grounded = pos.Y >= floorpos;
			if(grounded){
				Core.Player.Velocity.Y = 0;
				Core.Player.Velocity.X += HorizontalInput() * movemodifier;
			} else if(pos.Y + Core.Player.Velocity.Y > floorpos)
				Core.Player.Velocity.Y = floorpos - (float)pos.Y;
			else
				Core.Player.Velocity.Y += gravity;
			if(JumpInput() && grounded){
				Core.Player.Velocity.Y -= jumpforce;//make 30?
				/*if(Key[VirtualKey.Shift])
					Database.player.Velocity.Y *= 1.25f;*/
				Core.Player.Velocity.X += HorizontalInput() * jumparcmodifier;
			}
			Core.Player.DoVelocity();
			if(grounded) {
				Core.Player.Velocity.X *= friction;
				if(HorizontalInput() != 0)
					if(System.Math.Abs(HorizontalInput()) > 1)
						Core.Player.State = 2;
					else
						Core.Player.State = 1;
				else
					Core.Player.State = 0;
			} else
				Core.Player.State = 3;
			if(Core.Player.Velocity.X > 0)
			Core.Player.Facing = false;
			if(Core.Player.Velocity.X < 0)
			Core.Player.Facing = true;
		}


		public static int VerticalInput() {
			int output = 0;
			if(Key[VirtualKey.S])
				output += 1;
			if(Key[VirtualKey.W])
				output -= 1;
			return output;
		}
		public static int HorizontalInput() {
			int output = 0;
			if(Key['d'])
				output += 1;
			if(Key['a'])
				output -= 1;
			if(Key[VirtualKey.Shift])
				output *= 2;
			return output;
		}
		public static bool JumpInput() { return Key[' ']; }

	}

	//Probably move this into Data?
	public sealed class KeyMap : Dictionary<VirtualKey,bool> {

		new public bool this[VirtualKey key] {
			get {
				if(ContainsKey(key))
					return base[key];
				else
					return false;
			}
			set { base[key] = value; }
		}
		public bool this[char key] {
			get { return this[toKey(key)]; }
			set { base[toKey(key)] = value; }
		}

		private static VirtualKey toKey(char c) {
			switch(c){
			case 'w':
			case 'W':
				return VirtualKey.W;
			case 'a':
			case 'A':
				return VirtualKey.A;
			case 's':
			case 'S':
				return VirtualKey.S;
			case 'd':
			case 'D':
				return VirtualKey.D;
			case ' ':
				return VirtualKey.Space;
			}
			return VirtualKey.None;
		}

	}

}

namespace Platformer.Logic {
using Data;
using Data.Struct;
using Object;

	public abstract class Behavior : Identifiable {
		protected bool ShouldMove;
		protected Direction MoveDirection;
		protected Reference<Entity> Target;

		public Behavior(Reference<Entity> reference) { Target = reference; }

		public abstract void Query();
		public abstract void Move();

		public abstract IdentityNumber GetID();
		public abstract void SetID(IdentityNumber newIdentity);

	}

	public class LinearBehavior : Behavior {

		public LinearBehavior(Reference<Entity> reference) : base(reference) {
			ShouldMove = true;
			MoveDirection = Direction.LEFT;
		}

		public override void Query() { }
		public override void Move() {
			
		}
		public override IdentityNumber GetID() { return Target.Identity; }
		public override void SetID(IdentityNumber newIdentity) { throw new ImmutableIdentityException(typeof(LinearBehavior)); }

	}

}
