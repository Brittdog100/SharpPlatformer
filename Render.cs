using System;
using System.Numerics;
using System.Threading.Tasks;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Platformer {

	public sealed partial class MainPage : Page {

		void canvas_CreateResources(ICanvasAnimatedControl sender,CanvasCreateResourcesEventArgs args) {
			//Create all sprites.
			args.TrackAsyncAction(Initialization.CreateSprites(sender).AsAsyncAction());
			Initialization.CreateSheets();
			//Create the player object, however that's decided.
			Initialization.PreparePlayer();
		}

		void canvas_DrawAnimated(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args) {
			//TODO: rendering code here
			Input.Logic.Flow();
			Database.player.Render(args.DrawingSession);
			args.DrawingSession.DrawLine(new Vector2(0,floor),new Vector2(10000,floor),Color.FromArgb(255,0,0,0));
		}

		internal void setBackground(Brush b) { Background = b; }

	}

}

namespace Platformer.Render {
using Data.IO;

	public class Sprite {
		protected CanvasBitmap[] frame;
		public byte Length { get; private set; }
		protected int index = 0;

		public Sprite(CanvasBitmap[] frames) {
			frame = frames;
			Length = (byte)frames.Length;
		}

		public CanvasBitmap NextFrame() {
			int outf = index++;
			if(index >= Length)
				index = 0;
			return frame[outf];
		}
		public void Reset() { index = 0; }

		public bool HasNext() { return frame[index] != null; }

		public static Sprite FromDataMap(Data.IO.DataMap map) {
			if(map.IsLocked())
				return null;
			map.Lock();
			byte l = (byte)map["length"].Data;
			bool t = (bool)map["timed"].Data;
			bool d = (bool)map["directional"].Data;
			var tf = new CanvasBitmap[l];
			for(int x = 0; x < l; x++) {
				object tmp = map["frame" + x].Data;
				tf[x] = (CanvasBitmap)tmp;
			}
			byte w = 0;
			if(t)
				w = (byte)map["wait"].Data;
			if(d) {
				var rf = new CanvasBitmap[l];
				for(int x = 0; x < l; x++)
					rf[x] = (CanvasBitmap)map["rframe" + x].Data;
				if(t)
					return new DirectionalSprite(tf,rf,w);
				else return new DirectionalSprite(tf,rf);
			} else if(t) return new TimedSprite(tf, w); 
			else return new Sprite(tf);
		}
		
		public static implicit operator CanvasBitmap(Sprite s) { return s.NextFrame(); }

	}

	public class TimedSprite : Sprite {
		protected readonly byte delay;
		protected byte t;

		public TimedSprite(CanvasBitmap[] frames,byte timer) : base(frames) {
			delay = timer;
			t = 0;
		}

		new public CanvasBitmap NextFrame() {
			if(++t >= delay){
				t = 0;
				return base.NextFrame();
			} else
				return frame[index];
		}

	}

	public class DirectionalSprite : TimedSprite {
		private CanvasBitmap[] rframes;
		private bool _rev = false;
		public bool Reverse {
			get { return _rev; }
			set {
				if(value == _rev)
					return;
				Reset();
				_rev = value;
			}
		}

		public DirectionalSprite(CanvasBitmap[] f,CanvasBitmap[] r,byte d = 0) : base(f,d) { rframes = r; }

		new public CanvasBitmap NextFrame() {
			if(_rev) {
				if(++t >= delay) {
				t = 0;
				if(++index >= Length)
					index = 0;
				return rframes[index];
				} else return rframes[index];
			} else return base.NextFrame();
		}

		public static implicit operator CanvasBitmap(DirectionalSprite s) { return s.NextFrame(); }

	}

	public class SpriteSheet {
		private Sprite[] sprite;
		private SpriteReference[] backup;
		private byte _state = 0;
		public byte State {
			get { return _state; }
			set {
				if(value == _state)
					return;
				VerifySprite();
				if(sprite[_state] != null)
					sprite[_state].Reset();
				_state = value;
				VerifySprite();
				if(sprite[_state] is DirectionalSprite)
					(sprite[_state] as DirectionalSprite).Reverse = _rev;
			}
		}
		private bool _rev = false;
		public bool Reverse {
			get { return _rev; }
			set {
				if(value == _rev)
					return;
				_rev = value;
				if(sprite[_state] is DirectionalSprite)
					(sprite[_state] as DirectionalSprite).Reverse = _rev;
			}
		}

		public SpriteSheet(Sprite[] states) {
			sprite = states;
			State = 0;
		}
		public SpriteSheet(DataMap map) {
			State = 0;
			byte s = (byte)map["states"].Data;
			sprite = new Sprite[s];
			backup = new SpriteReference[s];
			for(int x = 0; x < s; x++) {
				backup[x] = (SpriteReference)map["state" + x].Data;
				sprite[x] = backup[x].ToSprite();
			}
		}

		public CanvasBitmap NextFrame() {
			VerifySprite();
			if(sprite[_state] is DirectionalSprite)
				return (sprite[_state] as DirectionalSprite).NextFrame();
			if(sprite[_state] is TimedSprite)
				return (sprite[_state] as TimedSprite).NextFrame();
			return sprite[_state].NextFrame();
		}
		public void VerifySprite() {
			if(sprite[_state] == null)
				sprite[_state] = backup[_state].ToSprite();
		}

		public static implicit operator CanvasBitmap(SpriteSheet s) { return s.NextFrame(); }

	}

	public struct SpriteReference {
		public readonly Data.Struct.Package Package;
		public readonly string Key;

		public SpriteReference(Data.Struct.Package p, string k) {
			Package = p;
			Key = k;
		}

		public Sprite ToSprite() { return Database.GetSprite(Package, Key); }
		new public string ToString() { return (Package.Identifier + ":" + Key);}

	}

}
