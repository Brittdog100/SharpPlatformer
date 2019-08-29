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
			float floorpos = MainPage.floor - (float)Database.player.Height;
			bool grounded = Database.player.Y >= floorpos;
			if(grounded){
				Database.player.Velocity.Y = 0;
				Database.player.Velocity.X += HorizontalInput() * movemodifier;
			} else if(Database.player.Y + Database.player.Velocity.Y > floorpos)
				Database.player.Velocity.Y = floorpos - (float)Database.player.Y;
			else
				Database.player.Velocity.Y += gravity;
			if(JumpInput() && grounded){
				Database.player.Velocity.Y -= jumpforce;//make 30?
				/*if(Key[VirtualKey.Shift])
					Database.player.Velocity.Y *= 1.25f;*/
				Database.player.Velocity.X += HorizontalInput() * jumparcmodifier;
			}
			Database.player.DoVelocity();
			if(grounded) {
				Database.player.Velocity.X *= friction;
				if(HorizontalInput() != 0)
					if(System.Math.Abs(HorizontalInput()) > 1)
						Database.player.State = 2;
					else
						Database.player.State = 1;
				else
					Database.player.State = 0;
			} else
				Database.player.State = 3;
			if(Database.player.Velocity.X > 0)
				Database.player.Facing = false;
			if(Database.player.Velocity.X < 0)
				Database.player.Facing = true;
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