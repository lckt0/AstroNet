using System;
using Runtime.Execution;

namespace Runtime.VCalls {
	internal interface IVCall {
		byte Code { get; }
		void Run(VMContext ctx, out ExecutionState state);
	}
}