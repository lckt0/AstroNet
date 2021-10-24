using System;

namespace Runtime {
	public class AstroNet {
		public static object Run(uint A_0, string A_1, RuntimeTypeHandle type, string id_, object[] args) {
			uint id = Convert.ToUInt32(id_.Length);
			var module = Type.GetTypeFromHandle(type).Module;
			return VMInstance.Instance(module).Run(id, args);
		}

		public static unsafe void Run(uint A_0, string A_1, RuntimeTypeHandle type, string id_, void*[] typedRefs, void* retTypedRef) {
			uint id = Convert.ToUInt32(id_.Length);
			var module = Type.GetTypeFromHandle(type).Module;
			VMInstance.Instance(module).Run(id, typedRefs, retTypedRef);
		}

		internal static object RunInternal(uint A_0, string A_1, int moduleId, ulong codeAddr, uint key, uint sigId, object[] args) {
			return VMInstance.Instance(moduleId).Run(codeAddr, key, sigId, args);
		}

		internal static unsafe void RunInternal(uint A_0, string A_1, int moduleId, ulong codeAddr, uint key, uint sigId, void*[] typedRefs,
			void* retTypedRef) {
			VMInstance.Instance(moduleId).Run(codeAddr, key, sigId, typedRefs, retTypedRef);
		}
	}
}