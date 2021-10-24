using Astro_Renewed.Services;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace Astro_Renewed
{
    class Program
    {
        public static List<string> VirtualizeMethods = new List<string>();

        [DllImport("kernel32.dll", ExactSpelling = true)]
        private static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private static string Load(string[] Arguments)
        {
            IntPtr ThisConsole = GetConsoleWindow();
            string lgo = "\r\n\r\n                   ___       __           ____  __   ___                  __          \r\n                  / _ | ___ / /________  / __ \\/ /  / _/_ _____ _______ _/ /____  ____\r\n                 / __ |(_-</ __/ __/ _ \\/ /_/ / _ \\/ _/ // (_-</ __/ _ `/ __/ _ \\/ __/\r\n                /_/ |_/___/\\__/_/  \\___/\\____/_.__/_/ \\_,_/___/\\__/\\_,_/\\__/\\___/_/   \r\n\r\n";
            string ModulePath = "";
            ShowWindow(ThisConsole, 9);
            Console.SetWindowSize(102, 22);
            Console.SetBufferSize(102, 9001);
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine(lgo);
            Console.Title = "AstroNet - Waiting for file";
            ShowWindow(ThisConsole, 9);
            Console.SetWindowSize(102, 22);
            Console.SetBufferSize(102, 9001);
            if (Arguments.Length == 1)
            {
                ModulePath = Arguments[0];
            }
            if (Arguments.Length == 0)
            {
                ConsoleColor dsx = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write("[+] ");
                Console.ForegroundColor = ConsoleColor.Blue;
                Console.Write("Path: ");
                Console.ForegroundColor = ConsoleColor.Yellow;
                ModulePath = Console.ReadLine().Replace("\"", "");
                Console.ForegroundColor = dsx;
                Console.Clear();
                Console.WriteLine(lgo);
                ShowWindow(ThisConsole, 9);
                Console.SetWindowSize(102, 22);
                Console.SetBufferSize(102, 9001);
            }
            if (Arguments.Length != 0 && Arguments.Length != 1)
            {
                ConsoleLogger.Log("An error occured while loading module.", ConsoleLogger.PrintMode.Error);
                Console.ReadKey();
                Environment.Exit(0);
            }
            return ModulePath;
        }

        private static string Save(ModuleDefMD module, string file)
        {
            Console.Title = "AstroNet - Saving file";
            string filepathmessage = "";
            if (file.ToLower().EndsWith(".exe") || file.ToLower().EndsWith(".dll"))
            {
                var path = file.Substring(0, file.Length - 4) + "-astro" + file.Substring(file.Length - 4, 4);
                filepathmessage = path;

                // Writing
                ModuleWriterOptions writerOptions = new ModuleWriterOptions(module);
                writerOptions.MetaDataOptions.Flags =
                 MetaDataFlags
                  .PreserveAll;
                writerOptions.MetaDataLogger =
                 DummyLogger
                  .NoThrowInstance;
                /*
                MemoryStream memory = new MemoryStream();
                module.Write(memory, writerOptions);

                File.WriteAllBytes(path, memory.ToArray());
                memory.Close();*/
            }
            try
            {
                Protections.Virtualization.Execute(module, filepathmessage, VirtualizeMethods);
                //Saving on KoiVM
                ConsoleLogger.Log("Your file is saved successfully.", ConsoleLogger.PrintMode.Success);
                Console.Title = "AstroNet - Done";
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log("An error occured while saving module.", ConsoleLogger.PrintMode.Error);
                ConsoleLogger.Log(ex.Message.ToString(), ConsoleLogger.PrintMode.Error);
                Console.ReadKey();
                Environment.Exit(0);
            }
            return filepathmessage;
        }

        public static void Main(string[] args)
        {
            string loadPath = Load(args);
            ModuleDefMD module = null;
            try
            {
                module = ModuleDefMD.Load(loadPath);
            }
            catch (Exception ex)
            {
                ConsoleLogger.Log("An error occured while loading module.", ConsoleLogger.PrintMode.Error);
                ConsoleLogger.Log(ex.Message.ToString(), ConsoleLogger.PrintMode.Error);
                Console.ReadKey();
                Environment.Exit(0);
            }
            ConsoleLogger.Log("Protection execution process is started.", ConsoleLogger.PrintMode.Success);
            Console.Title = "AstroNet - Processing..";

            //PROTECTION

            Protections.Anti.AntiDump.Execute(module);
            Protections.Attribute.Execute(module);
            //Protections.ArithmeticPr.Execute(module);
            //Protections.CtrlFlow.Execute(module);
            Protections.Rename.Execute(module);
            //Protections.Base64.Execute(module);
            //Protections.ProxyInt.Execute(module);
            //Protections.LocalToField.Execute(module);
            Protections.Base64.Execute(module);
            Protections.StringEnc.Execute(module);
            Protections.Base64.Execute(module);

            // Virtualization
            // VirtualizeMethods.Add("Main");

            // Saving and virtualizing methods

            string savePath = Save(module, loadPath);
            ConsoleLogger.Log(savePath, ConsoleLogger.PrintMode.Info);
            ConsoleLogger.Log("Press any key for exit application.", ConsoleLogger.PrintMode.Info);
            Console.ReadKey();
        }
        public static void afterVM(ModuleDefMD md)
        {
            Protections.ArithmeticPr.Execute(md);
            Protections.Base64.Execute(md);
            Protections.CtrlFlow.Execute(md);
            Protections.Other.StackUnfConfusion.Execute(md);
            Protections.Other.Watermark.Execute(md);
            Protections.ConstantMelting.Execute(md);
        }
    }
}