using System;

using Platformer.Data.Struct;

namespace Platformer.Error {

	public abstract class UnregisteredItemException : Exception {
		public readonly Package Package;
		public readonly string Key;

		public UnregisteredItemException(string type, Package pack, string key, Exception inner)
		: base ("Attempted to load unregistered " + type + " " + pack.Identifier + ":" + key, inner) {
			Package = pack;
			Key = key;
		}

	}

	public class UnregisteredSpriteException : UnregisteredItemException {
	
		public UnregisteredSpriteException(Package pack, string key, Exception inner) : base ("sprite", pack, key, inner) { }
	
	}

	public class UnregisteredScriptException : UnregisteredItemException {

		public UnregisteredScriptException(Package pack, string key, Exception inner) : base ("script", pack, key, inner) { }

	}

	public class ScriptException : Exception {

		public ScriptException(Package pack, string key, int line, Exception inner)
		: base("Encountered an exception in script " + pack.Identifier + ":" + key + " at line " + line, inner) { }

	}

}
