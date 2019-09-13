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

using Platformer.Error;

namespace Platformer {
using Data.Struct;
using Render;
using Geometry;

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
			Database.Player.Render(args.DrawingSession);
			args.DrawingSession.DrawLine(new Vector2(0,floor),new Vector2(10000,floor),Color.FromArgb(255,0,0,0));
		}

		internal void setBackground(Brush b) { Background = b; }

		public static Coordinate GetRelative(Coordinate c) { return c - Database.Camera; }

	}

	public static partial class Database {
		private static Bundle<Sprite>[] sprites = new Bundle<Sprite>[0x100];
		private static Bundle<Texture>[] tilesprites = new Bundle<Texture>[0x100];
		internal static Coordinate Camera = new Coordinate(0, 0);
		public static float RenderScale = 2.0f;

		/// <summary>
		/// Registers a sprite.
		/// </summary>
		/// <param name="pack"></param>
		/// <param name="key"></param>
		/// <param name="sprite"></param>
		/// <returns></returns>
		internal static bool AddSprite(Package pack, string key, Sprite sprite) { return sprites[pack].Add(sprite, key); }
		/// <summary>
		/// Gets a registered sprite.
		/// </summary>
		/// <param name="pack">The package the sprite is registered under.</param>
		/// <param name="key">The name of the sprite.</param>
		/// <returns></returns>
		/// <exception cref="UnregisteredSpriteException">If no Sprite is registered under the given Package/Key pair.</exception>
		/// <exception cref="UnregisteredPackageException">If the given package is not registered.</exception>
		public static Sprite GetSprite(Package pack, string key) {
			if(!_registered[pack])
				throw new UnregisteredPackageException(pack);
			try { return sprites[pack][key]; } catch(Exception e) { throw new UnregisteredSpriteException(pack, key, e); }
		}
		/// <summary>
		/// Gets the bundle containing the sprites for the given package.
		/// </summary>
		/// <param name="pack">The package to return a bundle from.</param>
		/// <returns>A bundle with all of the sprites registered under the given package.</returns>
		public static Bundle<Sprite> GetSpriteBundle(Package pack) { return sprites[pack]; }

		public static bool AddTexture(Package pack, string key, Texture texture) { return tilesprites[pack].Add(texture, key); }
		public static Texture GetTexture(Package pack, string key) {
			try { return tilesprites[pack][key]; }
			catch(Exception e) { throw new UnregisteredTextureException(pack, key, e); }
		}
		public static Bundle<Texture> GetTextureBundle(Package pack) {
			if(!_registered[pack])
				throw new UnregisteredPackageException(pack, null);
			return tilesprites[pack];
		}

	}

}

namespace Platformer.Render {
using Microsoft.Graphics.Canvas.Effects;
using Data.IO;

	public static class Effects {
		private static Transform2DEffect VerticalMirror = new Transform2DEffect() {
			TransformMatrix = new Matrix3x2(-1, 0, 0, 1, 0, 0)
		};

		public static void MirrorVertical(CanvasBitmap img) {
			throw new NotImplementedException();
			//VerticalMirror.Source = img;
		}

	}

	public class Texture {
		private CanvasBitmap image;

		public Texture(CanvasBitmap img) { image = img; }

		public static implicit operator CanvasBitmap(Texture s) { return s.image; }

	}

	public class Sprite {
		protected CanvasBitmap[] frame;
		/// <summary>
		/// The number of frames in this Sprite's animation.
		/// </summary>
		public byte Length { get; private set; }
		protected int index = 0;

		public Sprite(CanvasBitmap[] frames) {
			frame = frames;
			Length = (byte)frames.Length;
		}

		/// <summary>
		/// Gets the next frame in the sequence.
		/// </summary>
		/// <returns>The <code>CanvasBitmap</code> containing the next image in the sprite's sequence.</returns>
		public CanvasBitmap NextFrame() {
			int outf = index++;
			if(index >= Length)
				index = 0;
			return frame[outf];
		}
		/// <summary>
		/// Turns the frame counter back to zero, starting the animation over.
		/// </summary>
		public void Reset() { index = 0; }

		public bool HasNext() { return frame[index] != null; }

		/// <summary>
		/// Loads a sprite from a DataMap. This method is
		/// all but reserved for the sprite loading method
		/// in the ResourceManager. However, a sprite's
		/// DataMap should be suffixed as a <code>.sdf</code>
		/// file, and should contain the following properties:
		/// <code>byte length</code>: the length of the sprite in frames;
		/// <code>bool timed</code>: if the sprite has a delay between frames;
		/// <code>byte wait</code>: the delay, if applicable, between frames as a number of frames;
		/// <code>bool directional</code>: denotes if the sprite is reversible.
		/// If you have your own method of loading sprites, the frames should be stored
		/// in the values <code>frameX</code>, where X is the index of the frame
		/// stored there as a <code>CanvasBitmap</code>, and <code>rframeX</code>
		///	for the reverse frames for directional sprites.
		/// </summary>
		/// <param name="map">The DataMap to load from. This DataMap will be locked after the operation.</param>
		/// <returns>The sprite outlined in the given DataMap.</returns>
		public static Sprite FromDataMap(Data.IO.DataMap map) {
			if(map.IsLocked())
				throw new LockedMapException();
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
		/// <summary>
		/// Ensures that there's actually a sprite in the current index;
		/// if not, it fetches it from the Database class.
		/// </summary>
		public void VerifySprite() {
			if(sprite[_state] == null)
				sprite[_state] = backup[_state].ToSprite();
		}

		public static implicit operator CanvasBitmap(SpriteSheet s) { return s.NextFrame(); }

	}

	/// <summary>
	/// Represents a <code>Sprite</code> object as stored
	/// in the <code>Database</code> class.
	/// </summary>
	public struct SpriteReference {
		public readonly Data.Struct.Package Package;
		public readonly string Key;

		public SpriteReference(string p, string k) : this(Database.GetPackage(p), k) { }
		public SpriteReference(byte p, string k) : this(Database.GetPackage(p), k) { }
		public SpriteReference(Data.Struct.Package p, string k) {
			Package = p;
			Key = k;
		}

		public Sprite ToSprite() { return Database.GetSprite(Package, Key); }
		new public string ToString() { return (Package.Identifier + ":" + Key); }

	}

	/// <summary>
	/// Represents a <code>StaticSprite</code> object as stored
	/// in the <code>Database</code> class.
	/// </summary>
	public struct TextureReference {
		public readonly Data.Struct.Package Package;
		public readonly string Key;

		public TextureReference(Data.Struct.Package p, string k) {
			Package = p;
			Key = k;
		}

		public Texture ToTexture() { return Database.GetTexture(Package, Key); }
		new public string ToString() { return (Package.Identifier + ":" + Key); }

	}

}
