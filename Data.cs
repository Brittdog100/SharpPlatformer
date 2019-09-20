using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

using Windows.Storage;

using Platformer.Error;

using static System.Diagnostics.Debug;

namespace Platformer {
using Data;
using Data.Struct;

	public static class Initialization {

		public static event Event.SpriteCreationEvent CreatingSprites;
		private static bool _spritesmade = false;
		public static event Event.CreationEvent CreatingSheets;
		private static bool _sheetsmade = false;
		public static event Event.TextureCreationEvent CreatingTextures;
		private static bool _texturesmade = false;
		public static event Event.CreationEvent CreatingTiles;
		private static bool _tilesmade = false;

		static Initialization() {
			CreatingSprites += CreateCoreSprites;
			CreatingSheets += CreateCoreSpriteSheets;
			CreatingTextures += CreateCoreTextures;
			CreatingTiles += CreateCoreTiles;
		}

		public static async Task CreateGraphics(ICanvasAnimatedControl sender) {
			await CreateSprites(sender);
			await CreateTextures(sender);
		}

		public static async Task CreateSprites(ICanvasAnimatedControl sender) {
			if(_spritesmade)
				return;
			_spritesmade = true;
			await CreatingSprites(sender);
		}
		public static void CreateSheets() {
			if(_sheetsmade)
				return;
			_sheetsmade = true;
			CreatingSheets();
		}
		public static async Task CreateTextures(ICanvasAnimatedControl sender) {
			if(_texturesmade)
				return;
			_texturesmade = true;
			await CreatingTextures(sender);
		}
		public static void CreateTiles() {
			if(_tilesmade)
				return;
			_tilesmade = true;
			CreatingTiles();
		}

		private async static Task CreateCoreSprites(ICanvasAnimatedControl sender) {
			WriteLine("starting sprite prep");
			await ResourceManager.LoadSprite(sender, Core.MainPackage, @"asset\sprite\test\idle.sdf");
			await ResourceManager.LoadSprite(sender, Core.MainPackage, @"asset\sprite\test\walk.sdf");
			await ResourceManager.LoadSprite(sender, Core.MainPackage, @"asset\sprite\test\run.sdf");
			await ResourceManager.LoadSprite(sender, Core.MainPackage, @"asset\sprite\test\fall.sdf");
			WriteLine("finished sprite prep");
		}
		private static void CreateCoreSpriteSheets() {
			//TODO
		}

		private async static Task CreateCoreTextures(ICanvasAnimatedControl sender) {
			WriteLine("starting texture prep");
			await ResourceManager.LoadTexture(sender, Core.MainPackage, @"asset\texture\default\ground.png", "ground");
			await ResourceManager.LoadTexture(sender, Core.MainPackage, @"asset\texture\default\grass.png", "grass");
			WriteLine("finished texture prep");
		}

		private static void CreateCoreTiles() {
			WriteLine("starting tile creation");
			//Tile order is important, since the order they are declared affects their index in tileset binaries.
			Core.AddTile(Core.MainPackage, Geometry.Tile.FromDataMap(new Data.IO.DataMap(Core.MainPackage, new Data.IO.AppDataFile(@"asset\tile\default\ground.dat"))));
			Core.AddTile(Core.MainPackage, Geometry.Tile.FromDataMap(new Data.IO.DataMap(Core.MainPackage, new Data.IO.AppDataFile(@"asset\tile\default\grass.dat"))));
			WriteLine("finished tile creation");
		}

		public static void PreparePlayer() {
			//TODO
			/**	This is going to be a little odd, but I'm going to move the burden of loading the player onto
			 *	the Package struct somehow. I might accomplish this by adding a definition for the player
			 *	object's DataMap into the package definition, but I'm still not particularly sure for now.
			 */
			Core.Player = new Object.Player(new Render.SpriteSheet(new Data.IO.DataMap(Core.MainPackage, new Data.IO.AppDataFile(@"asset\sprite\testplayer.ssf"))));
		}

	}

	/// <summary>
	/// This class is used to store game objects and general data storage.
	/// </summary>
	public static partial class Core {
		private static IdentityMap map = new IdentityMap();
		private static byte _nextpackage = 1;
		private static Package[] _pack = new Package[0x100];
		private static bool[] _registered = new bool[0x100];
		private static Dictionary<string,Package> _package = new Dictionary<string, Package>();
		/// <summary>
		/// The Package containing all of the objects created by the main game.
		/// </summary>
		public static readonly Package MainPackage = new Package("core",0);
		private static SmallBatch<Geometry.Tile>[] tiles = new SmallBatch<Geometry.Tile>[0x100];
		public static readonly Random rng = new Random();
		/// <summary>
		/// Represents the player-controlled object. This will always be
		/// ID 0x000000, but this is an easier accessor.
		/// </summary>
		public static Object.Player Player {
			get { return (Object.Player)map[new IdentityNumber(0)]; }
			set { map[new IdentityNumber(0)] = value; }
		}
		/// <summary>
		/// Represents the Level currently loaded. This will always be
		/// ID 0x000001, but this is an easier accessor.
		/// </summary>
		public static Geometry.Level Level {
			get { return (Geometry.Level)map[new IdentityNumber(1)]; }
			set { map[new IdentityNumber(1)] = value; }
		}

		static Core() {
			_pack[0] = MainPackage;
			_registered[0] = true;
			sprites[0] = new Bundle<Render.Sprite>(MainPackage);
			textures[0] = new Bundle<Render.Texture>(MainPackage);
			tiles[0] = new SmallBatch<Geometry.Tile>(MainPackage);
			_package.Add("core", MainPackage);
		}

		/// <summary>
		/// Determines the cause of error for an unregistered ID.
		/// </summary>
		/// <param name="id">An ID</param>
		/// <returns>An <code>UnregisteredPackageException</code> if the package
		/// is not registered, or an <code>UnregisteredObjectException</code> otherwise.
		/// </returns>
		private static Exception GetNumError(IdentityNumber id) {
			if(!_registered[id.Package])
				return new UnregisteredPackageException(id.Package);
			else
				return new UnregisteredObjectException(id);
		}

		/// <summary>
		/// Returns the object associated with an ID.
		/// </summary>
		/// <param name="id">The ID number.</param>
		/// <returns>The object at that ID.</returns>
		/// <exception cref="UnregisteredObjectException">If no object is found
		/// at the given ID.
		/// </exception>
		/// <exception cref="UnregisteredPackageException">If the package specified
		/// by the ID is not registered.
		/// </exception>
		public static Identifiable Get(IdentityNumber id) {
			WriteLine("get id " + id.ToString());
			if (!map.Has(id))
				throw GetNumError(id);
			return map[id];
		}
		/// <summary>
		/// Adds an identifiable object to the active "scene." Please note that the object's
		/// identity may change based on which identities are available, and these new IDs are
		/// randomly generated.
		/// </summary>
		/// <param name="o">The object to add.</param>
		/// <returns>The IdentityNumber that the added object was registered with.</returns>
		public static IdentityNumber Add(Identifiable o) {
			IdentityNumber id = o.GetID();
			if(map.Has(id)) {
				o.SetID(map.Random((Package)id.Package));
				id = o.GetID();
			}
			WriteLine("assigning to id " + id);
			map[id] = o;
			return id;
		}
		public static void Set(Identifiable obj, IdentityNumber id) {
			if(id == 0 && !(obj is Object.Player))
				throw new IdentityOverrideException(id);
			Release(id);
			obj.SetID(id);
			map[id] = obj;
		}
		/// <summary>
		/// Releases an ID and removes its object from the Database.
		/// </summary>
		/// <param name="id">The ID to release.</param>
		/// <returns><code>True</code> if the ID was released successfully,
		/// <code>False</code> otherwise.
		/// </returns>
		/// <exception cref="UnregisteredPackageException">If the package specified
		/// by the ID is not registered.
		/// </exception>
		public static bool Release(IdentityNumber id) {
			WriteLine("release id " + id.ToString());
			if(!_registered[id.Package])
				throw new UnregisteredPackageException(id.Package);
			return map.Release(id);
		}
		public static bool Release(Identifiable o) { return Release(o.GetID()); }

		/// <summary>
		/// Creates and registers a new package with the given name.
		/// </summary>
		/// <param name="name">The desired package name.</param>
		/// <returns>A registered package with the provided name.</returns>
		/// <exception cref="PackageNameOverrideException">If the package name specified
		/// is already taken.
		/// </exception>>
		public static Package CreatePackage(string name) {
			if(_package.ContainsKey(name))
				throw new PackageNameOverrideException(name);
			var output = new Package(name);
			RegisterPackage(output);
			return output;
		}
		internal static Package GetPackage(byte id) {
			if(!_registered[id])
				throw new UnregisteredPackageException(id, null);
			return _pack[id];
		}
		internal static Package GetPackage(string name) {
			if(!_package.ContainsKey(name))
				throw new UnregisteredPackageNameException(name);
			return _package[name];
		}
		internal static byte GetPackageNumber() { return _nextpackage++; }
		internal static void RegisterPackage(Package p) {
			if(_registered[p])
				throw new PackageOverrideException(p.Identifier);
			_package[p.Name] = p;
			_registered[p] = true;
			sprites[p] = new Bundle<Render.Sprite>();
			tiles[p] = new SmallBatch<Geometry.Tile>();
			tiles[p].Add(new Geometry.Tile());
			textures[p] = new Bundle<Render.Texture>();
		}

		public static byte AddTile(Package pack, Geometry.Tile tile) {
			byte output = tiles[pack].Add(tile);
			ShortIdentity id = new ShortIdentity(pack, output);
			tiles[pack][output].SetID(id);
			WriteLine(tile.Identity);
			return output;
		}
		public static Geometry.Tile GetTile(Package pack, byte num) {
			if(num == 0)
				return tiles[0][0];
			return tiles[pack][num];
		}
		public static SmallBatch<Geometry.Tile> GetTileBatch(Package pack) { return tiles[pack]; }

	}

}

namespace Platformer.Data {

	public interface Identifiable {

		/// <summary>
		/// Gets the IdentityNumber for the given Identifiable.
		/// </summary>
		/// <returns>The IdentityNumber for this Identifiable object.</returns>
		IdentityNumber GetID();
		void SetID(IdentityNumber newIdentity);

	}

	public interface ShortIdentifiable {

		ShortIdentity GetID();
		
	}

	public enum Direction {
		LEFT,
		RIGHT,
		UP,
		DOWN
	}

	public interface Directional {

		Direction GetFacing();

	}

	/// <summary>
	/// Used as a more specific class for numbers used as IDs.
	/// They work in the format of 0xAABBBB, where AA is the hex
	/// code for the package identifier, and BBBB is the hex code
	/// for the object identifier. Can be casted to uint.
	/// </summary>
	public struct IdentityNumber {
		public readonly byte Package;
		public readonly uint Reference;
		private uint Full;

		public IdentityNumber(uint num) {
			Full = num;
			Package = (byte)(num / 0x10000);
			Reference = num % 0x10000;
		}
		public IdentityNumber(byte pnum,uint rnum) {
			Package = pnum;
			Reference = rnum;
			Full = (pnum * (uint)0x10000) + Reference;
		}

		public void Dispose() {
		}

		public static implicit operator uint(IdentityNumber n) { return n.Full; }

		new public string ToString() { return Full.ToString("X"); }

	}

	public struct ShortIdentity {
		public readonly byte Package;
		public readonly byte Reference;
		private ushort Full;

		public ShortIdentity(ushort fullnum) {
			Full = fullnum;
			Package = (byte)(fullnum / 0x100);
			Reference = (byte)(fullnum % 0x100);
		}
		public ShortIdentity(byte pack, byte key) {
			Package = pack;
			Reference = key;
			Full = (ushort)((Package * 0x100) + Reference);
		}

		public static implicit operator ushort(ShortIdentity s) { return s.Full; }

		new public string ToString() { return Full.ToString("X"); }

	}

	public static class ResourceManager {
		public static readonly StorageFolder AppFolderPath;
		private static string PackageName = "Platformer.";

		static ResourceManager() {
			AppFolderPath = ApplicationData.Current.LocalFolder;
			//Set up our assets and asset directories.
			CreateDirs(
				"asset",
				@"asset\texture",@"asset\sprite",
				@"asset\script",
				@"asset\tile",
				@"asset\sound",@"asset\sound\music"
			);
			foreach(string r in GetEmbeddedResourceNames()) {
				string ern = r.Remove(0,PackageName.Length);
				if(!ern.StartsWith("asset."))
					continue;
				string adp = ern.Remove(ern.LastIndexOf('.')).Replace('.','\\') + ern.Substring(ern.LastIndexOf('.'));
				using(Stream rout = GetAppDataWriteStream(adp))
						GetEmbeddedResource(ern).CopyTo(rout);
			}
		}

		public static string AppDataPath(string path) { return (AppFolderPath.Path + "\\" + path); }

		private static Assembly GetAssembly() { return Assembly.GetExecutingAssembly(); }
		public static Stream GetEmbeddedResource(string path) { return GetAssembly().GetManifestResourceStream(PackageName + path); }
		public static string[] GetEmbeddedResourceNames() { return GetAssembly().GetManifestResourceNames(); }

		public static Task<Stream> GetAppDataStream(string path) { return AppFolderPath.OpenStreamForReadAsync(path); }
		public static Stream GetAppDataWriteStream(string path) { return AppFolderPath.OpenStreamForWriteAsync(path, CreationCollisionOption.ReplaceExisting).Result; }

		public static T DeserializeAppDataBin<T>(string path) {
			using(Stream tmp = GetAppDataStream(path).Result)
				return (T)new BinaryFormatter().Deserialize(tmp);
		}

		public static void WriteBin<T>(T arg, string path) {
			using(Stream tmp = GetAppDataWriteStream(path)) {
				new BinaryFormatter().Serialize(tmp, arg);
			}
		}

		public static void CreateDir(string path) { var tmp = AppFolderPath.CreateFolderAsync(path); }

		public static void CreateDirs(params string[] paths) { foreach(string path in paths) CreateDir(path); }

		public static void SerializeBin<T>(T arg, Stream stream) { new BinaryFormatter().Serialize(stream, arg); }
		public static T DeserializeBin<T>(Stream stream) { return (T)new BinaryFormatter().Deserialize(stream); }

		/// <summary>
		/// Loads a <code>Sprite</code> from a file.
		/// </summary>
		/// <param name="sender">The Canvas Controller for the game canvas.</param>
		/// <param name="pack">The <code>Package</code> creating the <code>Sprite</code>.</param>
		/// <param name="path">The path to the <code>Sprite</code>'s <code>DataMap</code>.</param>
		/// <returns>The <code>Sprite</code> at the given path.</returns>
		/// <seealso cref="LoadSprite(ICanvasAnimatedControl, Struct.Package, DataMap)"/>
		/// <seealso cref="Render.Sprite"/>
		public static async Task<Render.Sprite> LoadSprite(ICanvasAnimatedControl sender, Struct.Package pack, string path) { return await LoadSprite(sender, pack, new IO.DataMap(pack, new IO.AppDataFile(path))); }
		/// <summary>
		/// Loads a <code>Sprite</code> from a file.
		/// </summary>
		/// <param name="sender">The Canvas Controller for the game canvas.</param>
		/// <param name="pack">The <code>Package</code> creating the <code>Sprite</code>.</param>
		/// <param name="map">The <code>DataMap</code> with the properties of the desired <code>Sprite</code>.</param>
		/// <returns>The <code>Sprite</code> outlined by the given <code>DataMap</code>.</returns>
		/// <seealso cref="LoadSprite(ICanvasAnimatedControl, Struct.Package, string)"/>
		/// <seealso cref="Render.Sprite"/>
		public static async Task<Render.Sprite> LoadSprite(ICanvasAnimatedControl sender, Struct.Package pack, IO.DataMap map) {
			bool d = map.Has("directional") && (bool)map["directional"].Data;
			string ft = (string)map["path"].Data;
			byte l = (byte)map["length"].Data;
			CanvasBitmap[] f;
			f = new CanvasBitmap[l];
			/* So, we start doing some weird stuff here; since there's so much
			 * potential variability in the way Sprite images can be loaded,
			 * we put the loaded images into the DataMap as properties. This is
			 * a void property in a DataMap so it won't be able to be converted
			 * into a plaintext Property, but that's not a big deal. That said,
			 * I don't know what the use case for such an operation might be for
			 * extensions in the game, but it is possible through this method to
			 * pass higher level data to an object through DataMaps via this method.
			 */
			for(int n = 0; n < l; n++)
				map.Add(new IO.RawProperty("frame" + n, typeof(void), await CanvasBitmap.LoadAsync(sender, AppDataPath(ft + n + ".png"))));
			if(d)
				for(int n = 0; n < l; n++)
					map.Add(new IO.RawProperty("rframe" + n, typeof(void), await CanvasBitmap.LoadAsync(sender, AppDataPath(ft + n + "L.png"))));
			string key = (string)map["key"].Data;
			Render.Sprite output = Render.Sprite.FromDataMap(map);
			if(Core.AddSprite(pack, key, output))
				return output;
			else return null;
		}
		public static async Task<Render.Texture> LoadTexture(ICanvasAnimatedControl sender, Struct.Package pack, string path, string key) {
			var img = await CanvasBitmap.LoadAsync(sender, AppDataPath(path));
			var output = new Render.Texture(img);
			if(Core.AddTexture(pack, key, output))
				return output;
			else return null;
		}

	}

}

namespace Platformer.Data.Struct {

	/// <summary>
	/// A Dictionary wrapper that uses IdentityNumbers
	/// for the Database class.
	/// </summary>
	internal class IdentityMap {
		private Dictionary<IdentityNumber, Identifiable> dict;

		internal IdentityMap() { dict = new Dictionary<IdentityNumber, Identifiable>(); }

		public Identifiable this[IdentityNumber n] {
			get { return dict[n]; }
			set { dict[n] = value; }
		}

		/// <summary>
		/// Basically the <code>HasKey</code> method.
		/// </summary>
		/// <param name="n">The <code>IdentityNumber</code> to query for.</param>
		/// <returns>
		/// <code>True</code> if the given <code>IdentityNumber</code> is
		/// already in this <code>IdentityMap</code>, <code>False</code>
		/// otherwise.
		/// </returns>
		internal bool Has(IdentityNumber n) { return dict.ContainsKey(n); }
		/// <summary>
		/// Removes the given <code>IdentityNumber</code>
		/// from this <code>IdentityMap</code>, freeing
		/// the number to prevent leakage.
		/// </summary>
		/// <param name="n">The <code>IdentityNumber</code> to release.</param>
		/// <returns><code>True</code> if the <code>IdentityNumber</code> was successfully removed, <code>False</code> otherwise.</returns>
		internal bool Release(IdentityNumber n) { return dict.Remove(n); }


		/// <summary>
		/// This is an internal method so it doesn't really do
		/// a lot for me to document, except that I'm trying to
		/// be diligent. Anyway, this hands out a random
		/// <code>IdentityNumber</code> that isn't used in this
		/// map.
		/// </summary>
		/// <param name="p">The Package whose extension is to be used.</param>
		/// <returns>An <code>IdentityNumber</code> not used in this <code>IdentityMap</code>.</returns>
		internal IdentityNumber Random(Package p) {
			uint output;
			do output = (uint)Core.rng.Next(0x10000);
			while(Has(new IdentityNumber(p, output)));
			return new IdentityNumber(p, output);
		}

	}

	/// <summary>
	/// Represents the extension that is responsible for an identifiable object.
	/// These are dispensed via <code cref="Core.CreatePackage(string)">CreatePackage</code>
	/// method in the <code>Database</code> class.
	/// </summary>
	/// <seealso cref="Core.GetPackage(byte)"/>
	/// <seealso cref="Core.GetPackage(string)"/>
	/// <seealso cref="IdentityNumber"/>
	public struct Package {
		/// <summary>
		/// The two-digit hex code that represents this Package.
		/// </summary>
		public readonly byte Identifier;
		/// <summary>
		/// The key used to reference this Package when its
		/// Identifier is unknown.
		/// </summary>
		public readonly string Name;

		internal Package(string name) {
			Identifier = Core.GetPackageNumber();
			Name = name;
		}
		internal Package(string name, byte num) {
			Name = name;
			Identifier = num;
		}

		public static implicit operator byte(Package p) { return p.Identifier; }
		public static explicit operator Package(byte b) { return Core.GetPackage(b); }

	}

	/// <summary>
	/// A mostly useless Dictionary-type data structure where
	/// keys and values are interchangeable.
	/// </summary>
	/// <typeparam name="T"></typeparam>
	/// <typeparam name="U"></typeparam>
	public class BiDictionary<T,U> {
		private List<T> tl;
		private List<U> ul;

		public BiDictionary() {
			tl = new List<T>();
			ul = new List<U>();
		}
		public BiDictionary(params (T,U)[] tuples) : this() {
			foreach((T,U) t in tuples) {
				tl.Add(t.Item1);
				ul.Add(t.Item2);
			}
		}

		public U this[T t] {
			get {
				if(tl.Contains(t))
					return ul[tl.IndexOf(t)];
				else
					return default(U);
				}
			set {
				if(!ul.Contains(value))
					ul[tl.IndexOf(t)] = value;
			}
		}
		public T this[U u] {
			get {
				WriteLine("getting covalue to " + u.ToString());
				if(ul.Contains(u))
					return tl[ul.IndexOf(u)];
				else
					return default(T);
			}
			set {
				if(!tl.Contains(value))
				tl[ul.IndexOf(u)] = value;
			}
		}

		public bool Contains(T t) { return tl.Contains(t); }
		public bool Contains(U u) { return ul.Contains(u); }

		public bool Remove(T t) {
			int i = tl.IndexOf(t);
			if(i == -1)
				return false;
			tl.RemoveAt(i);
			ul.RemoveAt(i);
			return true;
		}
		public bool Remove(U u) {
			int i = ul.IndexOf(u);
			if(i == -1)
				return false;
			ul.RemoveAt(i);
			tl.RemoveAt(i);
			return true;
		}

	}

	public struct Bundle<T> {
		private Dictionary<string,T> _dict;
		public readonly Package Source;

		public Bundle(Package src) {
			Source = src;
			_dict = new Dictionary<string,T>();
		}

		public T this[string k] {
			get {
				if(_dict.ContainsKey(k))
					return _dict[k];
				else return default(T);
			}
			private set { _dict[k] = value; }
		}

		public bool Add(T t,string k) {
			if(_dict.ContainsKey(k) || t == null)
				return false;
			this[k] = t;
			return true;
		}


	}

	public struct SmallBatch<T> where T : ShortIdentifiable {
		private T[] _val;
		public readonly Package Source;
		private int _index;

		public SmallBatch(Package p) {
			Source = p;
			_val = new T[0x100];
			_index = 0;
		}

		public T this[byte k] {
			get { return _val[k]; }
			private set { _val[k] = value; }
		}

		public byte Add(T t) {
			if(_index < 0x100)
				_val[_index] = t;
			else
				throw new SmallBatchOverflowException(Source, _val[0].GetType());
			return (byte)_index++;
		}

	}

	public struct Reference<T> where T : Identifiable {
		public readonly IdentityNumber Identity;

		public Reference(IdentityNumber id) { Identity = id; }

		public static implicit operator T(Reference<T> r) { return (T)Core.Get(r.Identity); }

	}

	public struct ShortReference<T> where T : ShortIdentifiable {
		public readonly ShortIdentity Identity;

		public ShortReference(ShortIdentity id) { Identity = id; }

	}
}

namespace Platformer.Data.IO {
using Geometry;
using Render;
using Struct;

	/// <summary>
	/// Represents a file in the AppData folder. Basically a glorified string
	/// with file loading methods.
	/// </summary>
	public struct AppDataFile {
		public readonly string Path;

		public AppDataFile(string path) { Path = path; }

		/// <summary>
		/// Retrieves a <code>Stream</code> to the file this <code>AppDataFile</code>
		/// points to. The stream is read-only.
		/// </summary>
		/// <returns>A read-only <code>Stream</code> for the file.</returns>
		/// <seealso cref="ResourceManager.GetAppDataStream(string)"/>
		/// <seealso cref="LoadForWrite()"/>
		public Stream Load() { return ResourceManager.GetAppDataStream(Path).Result; }
		/// <summary>
		/// Retrieves a <code>Stream</code> to the file this <code>AppDataFile</code>
		/// points to. The stream can be written to.
		/// </summary>
		/// <returns>A writeable <code>Stream</code> for the file.</returns>
		/// <seealso cref="ResourceManager.GetAppDataWriteStream(string)"/>
		/// <seealso cref="Load()"/>
		public Stream LoadForWrite() { return ResourceManager.GetAppDataWriteStream(Path); }

	}

	/// <summary>
	/// An interface for objects that can be serialized as a <code>DataMap</code>.
	/// Practically any data type can be deserialized, but serializing is a bit
	/// more difficult since often times since the data is not always preserved.
	/// A good example of this is the <code>Sprite</code> class; when deserialized,
	/// all of the images no longer have the appropriate file paths, just image objects.
	/// </summary>
	/// <seealso cref="DataMap"/>
	public interface DataMappable {

		/// <summary>
		/// Stores the state of this object as a DataMap to be exported to a file.
		/// </summary>
		/// <returns></returns>
		DataMap Package();

	}

	/// <summary>
	/// DataMaps are used as a rudimentary form of deserialization focused on rapid
	/// development; it is created with human readability in mind, and should be used
	/// with the <code>.dat</code> file extension except in cases where it shouldn't.
	/// </summary>
    public class DataMap {
		private static BiDictionary<string,Type> _types = new BiDictionary<string,Type>(
			("behavior", typeof(Reference<Logic.Behavior>)),
			("bool", typeof(bool)),
			("byte", typeof(byte)),
			("dat", typeof(DataMap)),
			("double", typeof(double)),
			("file", typeof(AppDataFile)),
			("float", typeof(float)),
			("int", typeof(int)),
			("point", typeof(Coordinate)),
			("rect", typeof(BoundingBox)),
			("sheet", typeof(SpriteSheet)),
			("sprite", typeof(SpriteReference)),
			("str", typeof(string)),
			("texture", typeof(TextureReference)),
			("vec", typeof(Vector2))
		);

		private Dictionary<string,RawProperty> props = new Dictionary<string,RawProperty>();
		private bool finalized = false;
		/// <summary>
		/// The <code>Package</code> responsible for this <code>DataMap</code>.
		/// </summary>
		public readonly Package Package;

		internal DataMap(AppDataFile f) : this(default, f) { }
		public DataMap(Package p) { Package = p; }
		public DataMap(Package p, AppDataFile f) : this(p){
			using(StreamReader s = new StreamReader(f.Load())) {
				while(!s.EndOfStream)
					Add(ParseLine(s.ReadLine()).ToRaw());
			}
		}

		public RawProperty this[string name] {
			get { return props[name]; }
			set {
				if(finalized)
					return;
				else props[name] = value;
			}
		}

		public void Add(RawProperty prop) { if(!finalized) this[prop.Name] = prop; }
		public void Add(Property prop) { Add(prop.ToRaw()); }

		public bool Has(string prop) { return props.ContainsKey(prop); }

		public void ToFile(AppDataFile path) {
			//TODO make this thing
			var stream = new StreamWriter(path.LoadForWrite());
			foreach(string k in props.Keys) {

			}
		}

		public static Type GetType(string t) {
			if(_types.Contains(t))
				return _types[t];
			else return null;
		}
		public static string GetTypeDeclarator(Type t) {
			if(_types.Contains(t))
				return _types[t];
			else return null;
		}

		public static Property ParseLine(string definition) {
			string[] parts = definition.Split(' ', 3);
			if(_types.Contains(parts[0]))
				return new Property(parts[1], parts[0], parts[2]);
			else
				return null;
		}

		public static object Deserialize(Property prop) {
			switch(prop.Type) {
			case "bool" : return bool.Parse(prop.Data);
			case "byte" : return byte.Parse(prop.Data);
			case "double" : return double.Parse(prop.Data);
			case "file" : return new AppDataFile(prop.Data);
			case "float" : return float.Parse(prop.Data);
			case "int" : return int.Parse(prop.Data);
			case "point" :
				string[] point = prop.Data.Split(',');
				return new Coordinate(double.Parse(point[0]), double.Parse(point[1]));
			case "rect" :
				string[] rect = prop.Data.Split(',');
				return new BoundingBox(0,0,double.Parse(rect[0]), double.Parse(rect[1]));
			case "sheet" : return new SpriteSheet(new DataMap(new AppDataFile(prop.Data)));
			case "sprite" :
				string[] sprite = prop.Data.Split(':');
				byte spritepacknum;
				if(byte.TryParse(sprite[0], out spritepacknum))
					return new SpriteReference(spritepacknum, sprite[1]);
				else return new SpriteReference(sprite[0], sprite[1]);
			case "str" : return prop.Data;
			case "texture" :
				string[] texture = prop.Data.Split(':');
				byte texpacknum;
				if(byte.TryParse(texture[0], out texpacknum))
					return new TextureReference(Core.GetPackage(texpacknum), texture[1]);
				else return new TextureReference(Core.GetPackage(texture[0]), texture[1]);
			case "vec" :
				string[] vec = prop.Data.Split(',');
				return new Vector2(float.Parse(vec[0]), float.Parse(vec[1]));
			default : return null;				
			}
		}

		public void Lock() { finalized = true; }
		public bool IsLocked() { return finalized; }

		public static string Prepare(object o) {
			string t = _types[o.GetType()];
			switch(t) {
			case "bool" : return ((bool)o).ToString().ToLower();
			case "byte" : return ((byte)o).ToString();
			case "double" : return ((double)o).ToString();
			case "file" : return ((AppDataFile)o).Path;
			case "float" : return ((float)o).ToString();
			case "int" : return ((int)o).ToString();
			case "point" :
				Coordinate point = (Coordinate)o;
				return (point.X + "," + point.Y);
			case "rect" :
				BoundingBox rect = (BoundingBox)o;
				return (rect.Width + "," + rect.Height);
			case "sprite" : return ((SpriteReference)o).ToString();
			case "str" : return (o as string);
			case "texture" : return ((TextureReference)o).ToString();
			case "vec" :
				Vector2 vector = (Vector2)o;
				return (vector.X + "," + vector.Y);
			}
			return null;
		}

		public static void CheckForSet(DataMap map, params string[] props) {
			List<string> missing = new List<string>();
			foreach(string p in props)
				if(!map.Has(p))
					missing.Add(p);
			if(missing.Count > 0)
				throw new MissingPropertyException(missing.ToArray());
		}

	}	

	public class Property {
		public readonly string Name;
		public readonly string Type;
		public readonly string Data;

		public Property(string n,string t,string d) {
			Name = n;
			Type = t;
			Data = d;
		}

		public RawProperty ToRaw() {
			return new RawProperty(Name,DataMap.GetType(Type),DataMap.Deserialize(this));
		}

	}

	/// <summary>
	/// Represents a fully deserialized property in a <code>DataMap</code>.
	/// Compared to the "refined" <code>Property</code> object, a <code>RawProperty</code>
	/// is made up of fully useable values rather than <code>string</code> representations.
	/// </summary>
	/// <seealso cref="DataMap"/>
	/// <seealso cref="Property"/>
	public class RawProperty {
		public readonly string Name;
		public readonly Type Type;
		public readonly object Data;

		public RawProperty(string n,Type t,object d) {
			Name = n;
			Type = t;
			Data = d;
		}

		public static explicit operator Property(RawProperty p) { return new Property(p.Name,DataMap.GetTypeDeclarator(p.Type),DataMap.Prepare(p.Data)); }

	}

}
