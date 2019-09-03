using System;

using Platformer.Data;
using Platformer.Data.Struct;

namespace Platformer.Error {

	public class UnregisteredObjectException : Exception {
		public readonly IdentityNumber Reference;

		public UnregisteredObjectException(IdentityNumber key, Exception inner)
		: base("There is no object registered to " + key.ToString(), inner) { Reference = key; }

	}

	public class UnregisteredPackageException : Exception {
		public readonly byte Identifier;

		public UnregisteredPackageException(byte num, Exception inner)
		: base("There is no package registered to " + num, inner) { Identifier = num; }

	}

	public class UnregisteredPackageNameException : Exception {
		public readonly string Name;

		public UnregisteredPackageNameException(string name, Exception inner)
		: base("No package is registered under the name \"" + name + "\"", inner) { Name = name; }

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

	public class UnregisteredScriptException : UnregisteredItemException {

		public UnregisteredScriptException(Package pack, string key, Exception inner) : base("script", pack, key, inner) { }

	}

	public class ScriptException : Exception {

		public ScriptException(Package pack, string key, int line, Exception inner)
		: base("Encountered an exception in script " + pack.Identifier + ":" + key + " at line " + line, inner) { }

	}

}
