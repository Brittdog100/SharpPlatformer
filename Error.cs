using System;

using Platformer.Data;
using Platformer.Data.Struct;

namespace Platformer.Error {

	/// <summary>
	/// Thrown when attempting to get an object from an
	/// ID that does not have any object assigned to it.
	/// </summary>
	public class UnregisteredObjectException : Exception {
		public readonly IdentityNumber Reference;

		public UnregisteredObjectException(IdentityNumber key) : this(key, null) { }
		public UnregisteredObjectException(IdentityNumber key, Exception inner)
		: base("There is no object registered to " + key.ToString(), inner) { Reference = key; }

	}

	/// <summary>
	/// Thrown when attempting to call an ID number from
	///	a package that is not registered.
	/// </summary>
	public class UnregisteredPackageException : Exception {
		public readonly byte Identifier;

		public UnregisteredPackageException(byte num) : this(num, null) { }
		public UnregisteredPackageException(byte num, Exception inner)
		: base("There is no package registered to " + num, inner) { Identifier = num; }

	}

	/// <summary>
	/// Thrown when attempting to reference a package with a name
	/// that is not registered.
	/// </summary>
	public class UnregisteredPackageNameException : Exception {
		public readonly string Name;

		public UnregisteredPackageNameException(string name) : this(name, null) { }
		public UnregisteredPackageNameException(string name, Exception inner)
		: base("No package is registered under the name \"" + name + "\"", inner) { Name = name; }

	}

	/// <summary>
	/// Thrown when attempting to assign a package to an occupied slot.
	/// </summary>
	public class PackageOverrideException : Exception {
		public readonly byte Identifier;

		public PackageOverrideException(byte num) : this(pack, null) { }
		public PackageOverrideException(byte num, Exception inner)
		: base("A package is already registered with the ID " + num, inner) { Identifier = num; }

	}

	/// <summary>
	/// Thrown when attempting to register a package with a name that is already taken.
	/// </summary>
	public class PackageNameOverrideException : Exception {
		public readonly string Name;

		public PackageNameOverrideException(string name) : this(name, null) { }
		public PackageNameOverrideException(string name, Exception inner)
		: base("A package is already registered with the name \"" + name + "\"", inner) { Name = name; }

	}

	public abstract class UnregisteredItemException : Exception {
		public readonly Package Package;
		public readonly string Key;

		public UnregisteredItemException(string type, Package pack, string key, Exception inner)
		: base("Attempted to load unregistered " + type + " " + pack.Identifier + ":" + key, inner) {
			Package = pack;
			Key = key;
		}

	}

	public class UnregisteredSpriteException : UnregisteredItemException {
	
		public UnregisteredSpriteException(Package pack, string key, Exception inner) : base("sprite", pack, key, inner) { }
	
	}

	public class UnregisteredTileSpriteException : UnregisteredItemException {

		public UnregisteredTileSpriteException(Package pack, string key, Exception inner) : base ("tile sprite", pack, key, inner) { }

	}

	public class UnregisteredScriptException : UnregisteredItemException {

		public UnregisteredScriptException(Package pack, string key, Exception inner) : base("script", pack, key, inner) { }

	}

	public class LockedMapException : Exception {

		public LockedMapException() : this(null) { }
		public LockedMapException(Exception inner) : base("The given DataMap is locked", inner) { }

	}

	public class ScriptException : Exception {

		public ScriptException(Package pack, string key, int line, Exception inner)
		: base("Encountered an exception in script " + pack.Identifier + ":" + key + " at line " + line, inner) { }

	}

}
