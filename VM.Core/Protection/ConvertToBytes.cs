using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace Core.Protection
{
    internal class ConvertToBytes
    {
        private BinaryWriter _binWriter;
        public byte[] ConvertedBytes;
        private readonly MethodDef _methods;
        public bool Successful;

        public ConvertToBytes(MethodDef methodDef)
        {
            _methods = methodDef;
            ConvertedBytes = null;
            Successful = false;
        }
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        public void ConversionMethod(ModuleDefMD moduleDefMD)
        {
            _binWriter = new BinaryWriter(new MemoryStream()); //create a binary writer 
            ExceptionStarter(moduleDefMD);//Process Method Exception handlers.
            _binWriter.Write(_methods.Body.Instructions
             .Count); //write the amount of instructions so the conversion method knows how many instructions to look for
            foreach (var ins in _methods.Body.Instructions) //loop all instructions in method
            {
                OpcodeWriter(ins.OpCode); //write the byte of the opcode to resolve to later
                var opType = ins.OpCode.OperandType; //get the opcode operand type
                                                     //148	021A	call	class [mscorlib]System.Reflection.MethodBase [mscorlib]System.Reflection.MethodBase::GetMethodFromHandle(valuetype [mscorlib]System.RuntimeMethodHandle)
                if (ins.Operand != null && (ins.Operand.ToString().Contains("StackTrace") || ins.Operand.ToString().Contains("Assembly") || ins.Operand.ToString().Contains("GetMethodFromHandle")))
                {
                    throw new Exception();
                }
                switch (opType)
                {
                    case OperandType.InlineNone:
                        InlineNone(ins);
                        continue;
                    case OperandType.InlineMethod:
                        InlineMethod(ins);
                        continue;
                    case OperandType.InlineString:
                        InlineString(ins);
                        continue;
                    case OperandType.InlineI:
                        InlineI(ins);
                        continue;
                    case OperandType.ShortInlineVar:
                        ShortInlineVar(ins);
                        continue;
                    case OperandType.InlineField:
                        InlineField(ins);
                        continue;
                    case OperandType.InlineType:
                        InlineType(ins);
                        continue;
                    case OperandType.ShortInlineBrTarget:
                        ShortInlineBrTarget(_methods.Body.Instructions, ins);
                        continue;
                    case OperandType.ShortInlineI:
                        ShortInlineI(ins);
                        continue;
                    case OperandType.InlineSwitch:
                        InlineSwitch(_methods.Body.Instructions, ins);
                        continue;
                    case OperandType.InlineBrTarget:
                        InlineBrTarget(_methods.Body.Instructions, ins);
                        continue;
                    case OperandType.InlineTok:
                        InlineTok(ins);
                        continue;
                    case OperandType.InlineVar:
                        InlineVar(ins);
                        continue;
                    case OperandType.ShortInlineR:
                        ShortInlineR(ins);
                        break;
                    case OperandType.InlineR:
                        InlineR(ins);
                        break;
                    case OperandType.InlineI8:
                        InlineI8(ins);
                        break;
                    default:
                        throw new Exception(string.Format("OperandType {0} Not Supported", opType));
                }
            }
            Successful = true;
            var buffer = new byte[_binWriter.BaseStream.Length];
            _binWriter.BaseStream.Position = 0;
            _binWriter.BaseStream.Read(buffer, 0, buffer.Length);
            ConvertedBytes = buffer;
        }

        private void OpcodeWriter(OpCode opcode)
        {
            _binWriter.Write((short)opcode.Value); //write its code in byte format
        }
        #region ExceptionHandlerCode
        [Obfuscation(Feature = "virtualization", Exclude = false)]
        private void ExceptionStarter(ModuleDefMD moduleDefMD)
        {
            if (_methods.MDToken.ToInt32() == 100663324)
            {

            }
            var allInstructions = _methods.Body.Instructions;
            _binWriter.Write(_methods.Body.ExceptionHandlers.Count);//we write the amount of exception handlers in the method
            foreach (ExceptionHandler bodyExceptionHandler in _methods.Body.ExceptionHandlers)
            {
                //just pass all the fields to a variable for ease of access
                var catchType = bodyExceptionHandler.CatchType;
                var filterStart = bodyExceptionHandler.FilterStart;
                var handlerEnd = bodyExceptionHandler.HandlerEnd;
                var handlerStart = bodyExceptionHandler.HandlerStart;
                var handlerType = bodyExceptionHandler.HandlerType;
                var tryEnd = bodyExceptionHandler.TryEnd;
                var tryStart = bodyExceptionHandler.TryStart;
                //catchType
                var catchResolveTypeDef = catchType.ResolveTypeDef();
                var imported = moduleDefMD.Import(catchResolveTypeDef).ResolveTypeDef();
                //catchType may be null so we need to account for this
                if (catchType == null)
                {
                    //-1 is appropriate since we can use this to detect a empty catch type
                    _binWriter.Write(-1);
                }

                else
                {
                    if (imported.Module != _methods.Module)
                        _binWriter.Write(catchType.MDToken.ToInt32());
                    else
                        _binWriter.Write(imported.MDToken.ToInt32());
                }
                //we get the index of the exceptionHandler information and write its instruction index so we can use this to start and end exception handlers using dynamic methods
                //filterStart
                var filterStartIndex = allInstructions.IndexOf(filterStart);
                if (filterStartIndex == -1)
                    _binWriter.Write(-1);
                else
                    _binWriter.Write(filterStartIndex);
                //handlerEnd
                var handlerendIndex = allInstructions.IndexOf(handlerEnd);
                if (handlerendIndex == -1)
                    _binWriter.Write(-1);
                else
                    _binWriter.Write(handlerendIndex);
                //handlerStart
                var handlerStartIndex = allInstructions.IndexOf(handlerStart);
                if (handlerStartIndex == -1)
                    _binWriter.Write(-1);
                else
                    _binWriter.Write(handlerStartIndex);
                //handlerType
                //there are many different handler types however catch and finally are the most common the others you will rarely see
                switch (handlerType)
                {
                    case ExceptionHandlerType.Catch:
                        _binWriter.Write((byte)1);
                        break;
                    case ExceptionHandlerType.Duplicated:
                        _binWriter.Write((byte)2);
                        break;
                    case ExceptionHandlerType.Fault:
                        _binWriter.Write((byte)3);
                        break;
                    case ExceptionHandlerType.Filter:
                        _binWriter.Write((byte)4);
                        break;
                    case ExceptionHandlerType.Finally:
                        _binWriter.Write((byte)5);
                        break;
                }
                //tryEnd
                var tryEndIndex = allInstructions.IndexOf(tryEnd);
                if (tryEndIndex == -1)
                    _binWriter.Write(-1);
                else
                    _binWriter.Write(tryEndIndex);
                //tryStart
                var tryStartIndex = allInstructions.IndexOf(tryStart);
                if (tryStartIndex == -1)
                    _binWriter.Write(-1);
                else
                    _binWriter.Write(tryStartIndex);
            }
        }
        #endregion
        #region operandTypeConversions

        private void InlineNone(Instruction instruction)
        {
            _binWriter.Write((byte)0); //write 0 so when converting back i know that this refers to a inline none operand type
        }

        private void InlineMethod(Instruction instruction)
        {
            _binWriter.Write((byte)1); //write 1 so when converting back i know its a inline method
            if (instruction.Operand is MethodSpec)
            {
                var methodSpec = instruction.Operand as MethodSpec; //cast the operand to a methoddef(if included in current module) or methodref (if in an external module)
                if (methodSpec == null)
                    throw new Exception("Check the instruction. This should not happen");
                var mdToken = methodSpec.MDToken.ToInt32();
                _binWriter.Write(mdToken);
            }
            else
            {
                var methodDeforRef = instruction.Operand as IMethodDefOrRef; //cast the operand to a methoddef(if included in current module) or methodref (if in an external module)
                if (methodDeforRef == null)
                    throw new Exception("Check the instruction. This should not happen");
                var mdToken = methodDeforRef.MDToken.ToInt32();
                _binWriter.Write(mdToken);
            }

        }

        private void InlineString(Instruction instruction)
        {
            _binWriter.Write((byte)2); //write 2 so when converting back i know its a inline string
            var operand = instruction.Operand.ToString(); //get the instructions operand
            _binWriter.Write(operand); //write string into binaryWriter.
        }

        private void InlineI(Instruction instruction)
        {
            _binWriter.Write((byte)3); //write 3 so when converting back i know its a inlineI
            var operand = instruction.GetLdcI4Value(); //get the instructions operand as an integer since inlineI is ints
            _binWriter.Write(operand); //write string into binaryWriter.
        }

        private void ShortInlineVar(Instruction instruction)
        {
            _binWriter.Write((byte)4); //write 4 so when converting back i know its a shortInlineVar
            if (instruction.Operand is Local)
            {
                var loc = instruction.Operand as Local; //cast it to a local(variable)
                _binWriter.Write(loc.Index); //write its index so can refer to it in conversion back
                _binWriter.Write((byte)0);
            }
            else if (instruction.Operand is Parameter)
            {
                var par = instruction.Operand as Parameter; //cast it to a local(variable)
                _binWriter.Write(par.Index); //write its index so can refer to it in conversion back
                _binWriter.Write((byte)1);
            }
        }

        private void InlineField(Instruction instruction)
        {
            _binWriter.Write((byte)5); //write 5 so when converting back i know its a inline field
            if (instruction.Operand is MemberRef)
            {
                var memberRef = instruction.Operand as MemberRef; //cast the operand to a fielddef
                if (memberRef == null)
                    throw new Exception("Check the instruction. This should not happen");
                var mdToken = memberRef.MDToken.ToInt32();
                _binWriter.Write(mdToken);
            }
            else
            {
                var fieldDef = instruction.Operand as FieldDef; //cast the operand to a fielddef
                if (fieldDef == null)
                    throw new Exception("Check the instruction. This should not happen");
                var mdToken = fieldDef.MDToken.ToInt32();
                _binWriter.Write(mdToken);
            }

        }

        private void InlineType(Instruction instruction)
        {
            _binWriter.Write((byte)6); //write 6 so when converting back i know its a inline type
            var typeDeforRef = instruction.Operand as ITypeDefOrRef; //cast the operand to a typeDef or typeRef
            if (typeDeforRef == null)
                throw new Exception("Check the instruction. This should not happen");
            var mdToken = typeDeforRef.MDToken.ToInt32();
            _binWriter.Write(mdToken);
        }

        private void ShortInlineBrTarget(IList<Instruction> allInstructions, Instruction instruction)
        {
            _binWriter.Write((byte)7); //write 7 so i know its shortinlinebrtarger when converting back
            var index = allInstructions
             .IndexOf((Instruction)instruction.Operand); //get the index of the instruction in all instructions so i can use this to mark the instruction as a branch location
            _binWriter.Write(index);
        }

        private void ShortInlineI(Instruction instruction)
        {
            _binWriter.Write((byte)8); //write 8 so when converting back i know its a shortInlineI
            var operand = instruction.GetLdcI4Value(); //get the instructions operand as an integer since inlineI is ints
            _binWriter.Write((byte)operand); //write string into binaryWriter.
        }

        private void InlineSwitch(IList<Instruction> allInstructions, Instruction instruction)
        {
            _binWriter.Write((byte)9); //write 9 so i know its inlineSwitch when converting back
            var allLocations = instruction.Operand as Instruction[];
            _binWriter.Write(allLocations.Count());
            foreach (var switchLocation in allLocations)
            {
                var index = allInstructions
                .IndexOf(switchLocation); //get the index of the instruction in all instructions so i can use this to mark the instruction as a branch location
                _binWriter.Write(index);
            }
        }

        private void InlineBrTarget(IList<Instruction> allInstructions, Instruction instruction)
        {
            _binWriter.Write((byte)10); //write 10 so i know its inlinebrTarget when converting back
            var index = allInstructions
                .IndexOf((Instruction)instruction.Operand); //get the index of the instruction in all instructions so i can use this to mark the instruction as a branch location
            _binWriter.Write(index);
        }

        private void InlineTok(Instruction instruction)
        {
            _binWriter.Write((byte)11); //write 11 so when converting back i know its a inlineTok
                                        //inlineTok can be a field method or a type so we need to check which first
            if (instruction.Operand is FieldDef)
            {

                var fieldDef = instruction.Operand as FieldDef; //cast the operand to a typeDef or typeRef
                var mdToken = fieldDef.MDToken.ToInt32();
                _binWriter.Write(mdToken);
                _binWriter.Write((byte)0);//says its fielddef
            }
            else if (instruction.Operand is ITypeDefOrRef)
            {

                var typeDeforRef = instruction.Operand as ITypeDefOrRef; //cast the operand to a typeDef or typeRef
                var mdToken = typeDeforRef.MDToken.ToInt32();
                _binWriter.Write(mdToken);
                _binWriter.Write((byte)1);//says its type
            }
            else if (instruction.Operand is IMethodDefOrRef)
            {

                var methoDefOrRef = instruction.Operand as IMethodDefOrRef; //cast the operand to a typeDef or typeRef
                var mdToken = methoDefOrRef.MDToken.ToInt32();
                _binWriter.Write(mdToken);
                _binWriter.Write((byte)2);//says its method
            }
            else
            {
                throw new Exception("Check the instruction. This should not happen");
            }
        }
        private void InlineVar(Instruction instruction)
        {
            _binWriter.Write((byte)12); //write 12 so when converting back i know its a InlineVar
            if (instruction.Operand is Local)
            {
                var loc = instruction.Operand as Local; //cast it to a local(variable)
                _binWriter.Write(loc.Index); //write its index so can refer to it in conversion back
                _binWriter.Write((byte)0);
            }
            else if (instruction.Operand is Parameter)
            {
                var par = instruction.Operand as Parameter; //cast it to a local(variable)
                _binWriter.Write(par.Index); //write its index so can refer to it in conversion back
                _binWriter.Write((byte)1);
            }
            else
            {
                _binWriter.Write(0); //write its index so can refer to it in conversion back
                _binWriter.Write((byte)0);
            }

        }
        private void ShortInlineR(Instruction instruction)
        {
            _binWriter.Write((byte)13); //write 8 so when converting back i know its a shortInlineI
            var operand = instruction.Operand; //get the instructions operand as an integer since inlineI is ints
            _binWriter.Write((float)operand); //write string into binaryWriter.
        }
        private void InlineR(Instruction instruction)
        {
            _binWriter.Write((byte)14); //write 8 so when converting back i know its a shortInlineI
            var operand = instruction.Operand; //get the instructions operand as an integer since inlineI is ints
            _binWriter.Write((double)operand); //write string into binaryWriter.
        }
        private void InlineI8(Instruction instruction)
        {
            _binWriter.Write((byte)15); //write 8 so when converting back i know its a shortInlineI
            var operand = instruction.Operand; //get the instructions operand as an integer since inlineI is ints
            _binWriter.Write((long)operand); //write string into binaryWriter.
        }

        #endregion
    }
}