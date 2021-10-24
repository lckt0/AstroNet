using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;

namespace KVM
{
    public class ModuleWriterListener : IModuleWriterListener
    {
        void IModuleWriterListener.OnWriterEvent(ModuleWriterBase writer, ModuleWriterEvent evt)
        {
            //bool flag = evt == ModuleWriterEvent.PESectionsCreated;
            //if (flag)
            //{
            //    //  NativeEraserNew.Erase(writer as NativeModuleWriter, writer.Module as ModuleDefMD); // Don't use
            //    NativeEraser.Erase(writer as NativeModuleWriter, writer.Module as ModuleDefMD);
            //}
            bool flag2 = this.OnWriterEvent != null;
            if (flag2)
            {
                this.OnWriterEvent(writer, new ModuleWriterListenerEventArgs(evt));
            }
        }

        public event EventHandler<ModuleWriterListenerEventArgs> OnWriterEvent;
    }
}
