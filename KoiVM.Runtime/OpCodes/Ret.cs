using System;
using Runtime.Dynamic;
using Runtime.Execution;

namespace Runtime.OpCodes {
	internal class Ret : IOpCode {
		public byte Code {
			get { return Constants.OPX_RET; }
		}

		public void Run(VMContext ctx, out ExecutionState state) {
			var sp = ctx.Registers[Constants.REG_SP].U4;
			var slot = ctx.Stack[sp];
			ctx.Stack.SetTopPosition(--sp);
			ctx.Registers[Constants.REG_SP].U4 = sp;

			ctx.Registers[Constants.REG_IP].U8 = slot.U8;
			state = ExecutionState.Next;
		}
	}
}