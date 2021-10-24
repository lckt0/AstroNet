using dnlib.DotNet.Writer;
using System;

namespace KVM
{
    public class ModuleWriterListenerEventArgs : EventArgs
    {
        public ModuleWriterListenerEventArgs(ModuleWriterEvent evt)
        {
            this.WriterEvent = evt;
        }

        public ModuleWriterEvent WriterEvent { get; private set; }
    }
}
