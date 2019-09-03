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
using Windows.Foundation;

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

		static Initialization() {
			CreatingSprites += CreateCoreSprites;
			CreatingSheets += CreateCoreSpriteSheets;
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

		private async static Task CreateCoreSprites(ICanvasAnimatedControl sender) {
			WriteLine("starting sprite prep");
			await ResourceManager.LoadSprite(sender, Database.MainPackage, @"asset\sprite\test\idle.sdf");
			await ResourceManager.LoadSprite(sender, Database.MainPackage, @"asset\sprite\test\walk.sdf");
			await ResourceManager.LoadSprite(sender, Database.MainPackage, @"asset\sprite\test\run.sdf");
			await ResourceManager.LoadSprite(sender, Database.MainPackage, @"asset\sprite\test\fall.sdf");
			WriteLine("finished sprite prep");
		}
		private static void CreateCoreSpriteSheets() {
			
		}

		public static void PreparePlayer() {
			//TODO
			Database.Player = new Object.Player(new Render.SpriteSheet(new Data.IO.DataMap(Database.MainPackage, new Data.IO.AppDataFile(@"asset\sprite\testplayer.ssf"))));
		}

	}

	public static class Database {
		private static IdentityMap map = new IdentityMap();
		private static byte _nextpackage = 0;
		/// <summary>
		/// Package for all game objects created by the main game.
		/// </summary>
		private static Package[] _pack = new Package[0x100];
		private static bool[] _registered = new bool[0x100];
		private static Dictionary<string,Package> _package = new Dictionary<string, Package>();
		public static readonly Package MainPackage = CreatePackage("core");
		private static Bundle<Render.Sprite>[] sprites = new Bundle<Render.Sprite>[0x100];
		public static readonly Random rng = new Random();
		public static Object.Player Player {
			get { return (Object.Player)map[new IdentityNumber(0)]; }
			set { map[new IdentityNumber(0)] = value; }
		}

		static Database() {
			sprites[0] = new Bundle<Render.Sprite>(MainPackage);
		}

		public static Identifiable Get(IdentityNumber id) {
			if(!map.Has(id))
				throw new UnregisteredObjectException(id, null);
			WriteLine("get id " + id.ToString()); return map[id];
		}
		public static bool Release(IdentityNumber id) {
			WriteLine("release id " + id.ToString());
			if(!map.Has(id))
				throw new UnregisteredObjectException(id, null);
			return map.Release(id);
		}

		public static Package CreatePackage(string name) {
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
				throw new UnregisteredPackageNameException(name, null);
			return _package[name];
		}
		internal static byte GetPackageNumber() { return _nextpackage++; }
		internal static void RegisterPackage(Package p) {
			_package[p.Name] = p;
			_registered[p.Identifier] = true;
		}

		internal static bool AddSprite(Package pack, string key, Render.Sprite sprite) { return sprites[pack.Identifier].Add(sprite, key); }
		public static Render.Sprite GetSprite(Package pack, string key) {
			try {
				return sprites[pack.Identifier][key];
			} catch(Exception e) { throw new UnregisteredSpriteException(pack, key, e); }
		}
		public static Bundle<Render.Sprite> GetSpriteBundle(Package pack) { return sprites[pack.Identifier]; }

	}

}

/// <summary>
/// Contains classes used in the Database, such as the
/// Entity and IdentityNumber classes, the Identifiable
/// interface, and the ResourceManager. For data structures,
/// see Platformer.Data.Struct.
/// </summary>
namespace Platformer.Data {

	/// <summary>
	/// Outlines an entity in the game.
	/// </summary>
	public abstract class Entity : Identifiable {
		public readonly IdentityNumber Identity;

		public Entity(IdentityNumber id) { Identity = id; }

		public IdentityNumber GetID() { return Identity; }

	}

	public interface Identifiable {

		/// <summary>
		/// Gets the IdentityNumber for the given Identifiable.
		/// </summary>
		/// <returns>The IdentityNumber for this Identifiable object.</returns>
		IdentityNumber GetID();

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

		public static async Task<Render.Sprite> LoadSprite(ICanvasAnimatedControl sender, Struct.Package pack, string path) { WriteLine("started loading " + path);  return await LoadSprite(sender, pack, new IO.DataMap(pack, new IO.AppDataFile(path))); }
		public static async Task<Render.Sprite> LoadSprite(ICanvasAnimatedControl sender, Struct.Package pack, IO.DataMap map) {
			bool d = map.Has("directional") && (bool)map["directional"].Data;
			string ft = (string)map["path"].Data;
			byte l = (byte)map["length"].Data;
			CanvasBitmap[] f;
			f = new CanvasBitmap[l];
			for(int n = 0; n < l; n++)
				map.Add(new IO.RawProperty("frame" + n, typeof(void), await CanvasBitmap.LoadAsync(sender, AppDataPath(ft + n + ".png"))));
			if(d)
				for(int n = 0; n < l; n++)
					map.Add(new IO.RawProperty("rframe" + n, typeof(void), await CanvasBitmap.LoadAsync(sender, AppDataPath(ft + n + "L.png"))));
			string key = (string)map["key"].Data;
			Render.Sprite output = Render.Sprite.FromDataMap(map);
			if(Database.AddSprite(pack, key, output))
				return output;
			else return null;
		}

	}

}

/// <summary>
/// Contains data structures used in the game.
/// </summary>
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

		internal bool Has(IdentityNumber n) { return dict.ContainsKey(n); }
		internal bool Release(IdentityNumber n) { return dict.Remove(n); }

		internal IdentityNumber Random(Package p) {
			uint output;
			do
				output = (uint)Database.rng.Next(0xFFFF);
			while(Has(new IdentityNumber(p, output)));
			return new IdentityNumber(p.Identifier, output);
		}

	}

	public struct Package {
		public readonly byte Identifier;
		public readonly string Name;

		internal Package(string name) {
			Identifier = Database.GetPackageNumber();
			Name = name;
		}

		public static implicit operator byte(Package p) { return p.Identifier; }

	}

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

}

namespace Platformer.Data.IO {
using Render;
using Struct;

	public struct AppDataFile {
		public readonly string Path;

		public AppDataFile(string path) { Path = path; }

		public Stream Load() { return ResourceManager.GetAppDataStream(Path).Result; }
		public Stream LoadForWrite() { return ResourceManager.GetAppDataWriteStream(Path); }

	}

	public interface DataMappable {

		DataMap Package();

	}

    public class DataMap {
		private static BiDictionary<string,Type> _types = new BiDictionary<string,Type>(
			("bool",typeof(bool)),
			("byte",typeof(byte)),
			("dat",typeof(DataMap)),
			("double",typeof(double)),
			("file",typeof(AppDataFile)),
			("float",typeof(float)),
			("int",typeof(int)),
			("point",typeof(Point)),
			("rect",typeof(Rect)),
			("sheet",typeof(SpriteSheet)),
			("sprite",typeof(SpriteReference)),
			("str",typeof(string)),
			("vec",typeof(Vector2))
		);

		private Dictionary<string,RawProperty> props = new Dictionary<string,RawProperty>();
		private bool finalized = false;
		public readonly Package Package;

		public DataMap(Package p) { Package = p; }
		public DataMap(Package p, AppDataFile f) : this(p){
			using(StreamReader s = new StreamReader(f.Load())) {
				while(!s.EndOfStream)
					Add(ParseLine(s.ReadLine()).ToRaw());
			}
		}

		public RawProperty this[string name] {
			get { WriteLine("getting property named " + name); return props[name]; }
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
				return new Point(double.Parse(point[0]), double.Parse(point[1]));
			case "rect" :
				string[] rect = prop.Data.Split(',');
				return new Rect(0,0,double.Parse(rect[0]), double.Parse(rect[1]));
			//case "sheet" : return new SpriteSheet(new DataMap(new AppDataFile(prop.Data)));
			case "sprite" :
				string[] sprite = prop.Data.Split(':');
				return new SpriteReference(Database.GetPackage(byte.Parse(sprite[0])), sprite[1]);
			case "str" : return prop.Data;
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
				Point point = (Point)o;
				return (point.X + "," + point.Y);
			case "rect" :
				Rect rect = (Rect)o;
				return (rect.Width + "," + rect.Height);
			case "sprite" : return ((SpriteReference)o).ToString();
			case "str" : return (o as string);
			case "vec" :
				Vector2 vector = (Vector2)o;
				return (vector.X + "," + vector.Y);
			}
			return null;
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
