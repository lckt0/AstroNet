using System;

namespace Runtime.Execution {
	internal enum ExecutionState {
		Next,
		Exit,
		Throw,
		Rethrow
	}
}