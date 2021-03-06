using System;
using Runtime.Dynamic;
using Runtime.Execution;

namespace Runtime.OpCodes {
	internal class Nop : IOpCode {
		public byte Code {
			get { return Constants.OP_NOP; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			state = ExecutionState.Next;
		}
	}
}