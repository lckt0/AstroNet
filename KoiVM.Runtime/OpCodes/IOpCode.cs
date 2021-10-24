using System;
using Runtime.Execution;

namespace Runtime.OpCodes {
	internal interface IOpCode {
		byte Code { get; }
		void Run(VMContext ctx, out ExecutionState state);
	}
}