using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Windows.Input;

using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

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
			Data.IO.DataMap dat = new Data.IO.DataMap(Database.MainPackage,new Data.IO.AppDataFile(@"asset\testcontrols.dat"));
			movemodifier = (float)dat["movement"].Data;
			jumparcmodifier = (float)dat["forcejumpx"].Data;
			gravity = (float)dat["gravity"].Data;
			jumpforce = (float)dat["forcejumpy"].Data;
			friction = (float)dat["friction"].Data;
		}

		public static void Flow() {
			float floorpos = MainPage.floor - (float)Database.Player.Height;
			bool grounded = Database.Player.Y >= floorpos;
			if(grounded){
				Database.Player.Velocity.Y = 0;
				Database.Player.Velocity.X += HorizontalInput() * movemodifier;
			} else if(Database.Player.Y + Database.Player.Velocity.Y > floorpos)
				Database.Player.Velocity.Y = floorpos - (float)Database.Player.Y;
			else
				Database.Player.Velocity.Y += gravity;
			if(JumpInput() && grounded){
				Database.Player.Velocity.Y -= jumpforce;//make 30?
				/*if(Key[VirtualKey.Shift])
					Database.player.Velocity.Y *= 1.25f;*/
				Database.Player.Velocity.X += HorizontalInput() * jumparcmodifier;
			}
			Database.Player.DoVelocity();
			if(grounded) {
				Database.Player.Velocity.X *= friction;
				if(HorizontalInput() != 0)
					if(System.Math.Abs(HorizontalInput()) > 1)
						Database.Player.State = 2;
					else
						Database.Player.State = 1;
				else
					Database.Player.State = 0;
			} else
				Database.Player.State = 3;
			if(Database.Player.Velocity.X > 0)
			Database.Player.Facing = false;
			if(Database.Player.Velocity.X < 0)
			Database.Player.Facing = true;
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

	public abstract class Behavior : Identifiable {
		protected bool ShouldMove;
		protected Direction MoveDirection;
		protected Reference<Entity> Target;

		public Behavior(Reference<Entity> reference) { Target = reference; }

		public abstract void Query();
		public abstract void Move();
		public abstract IdentityNumber GetID();
	}

	public class LinearBehavior {//: Behavior {



	}

}
