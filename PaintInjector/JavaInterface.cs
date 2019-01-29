using System;
using System.Collections.Generic;

namespace NetFramework
{
    public class JavaInterface
    {   
        public delegate void Callback();

        private static readonly Dictionary<CallbackType, Callback> _callbacks = new Dictionary<CallbackType, Callback>();

        private static void AddCallback(CallbackType callbackType, Callback callback) => _callbacks.Add(callbackType, callback);

        public static void RunCallback(CallbackType callbackType)
        {
            if (_callbacks.TryGetValue(callbackType, out var callback)) callback.Invoke();
        }

        [DllExport]
        public static void clickBuild(Callback callback) => AddCallback(CallbackType.Build, callback);
        
        [DllExport]
        public static void clickRun(Callback callback) => AddCallback(CallbackType.Run, callback);
        
        [DllExport]
        public static void clickStop(Callback callback) => AddCallback(CallbackType.Stop, callback);
        
        [DllExport]
        public static void clickCommit(Callback callback) => AddCallback(CallbackType.Commit, callback);
        
        [DllExport]
        public static void clickPush(Callback callback) => AddCallback(CallbackType.Push, callback);
        
        [DllExport]
        public static void clickPull(Callback callback) => AddCallback(CallbackType.Pull, callback);

        [DllExport]
        public static void initializeButtons()
        {            
//            new Thread(() =>
//            {
//                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
//
//                Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
//                {
//                    Console.WriteLine("HEEEEEEEEEEEEEEE " + args);
//                    Console.WriteLine("HEEEEEEEEEEEEEEE " + args.Name);
//                    Console.WriteLine("HEEEEEEEEEEEEEEE " + args.RequestingAssembly);
//                    Console.WriteLine("HEEEEEEEEEEEEEEE " + args.RequestingAssembly.Location);
//                    Console.WriteLine(args.RequestingAssembly.Location);
////                    Console.WriteLine($@"{new FileInfo(args.RequestingAssembly.Location).DirectoryName}\{args.Name.Split(',')[0]}.dll");
////                    return Assembly.LoadFrom(
////                        $@"{new FileInfo(args.RequestingAssembly.Location).DirectoryName}\{args.Name.Split(',')[0]}.dll");
//return Assembly.LoadFrom("OpenTitlebarButtons.dll");
//                }

//                Thread.Sleep(1000);

//            PerPixelAlphaWindow t = null;
                var program = new Program();
//                program.ChoosePaint((success, id) =>
//                {
//                    Console.WriteLine(success);
//                });
//            }).Start();
//            Console.WriteLine("111");
//            
            program.ChoosePaint((success, id) =>
            {
                if (success)
                {
                    Console.WriteLine("Success!");
                    program.GenerateButtons(id);
                }
                else
                {
                    Console.WriteLine("Error!");
                }             
            });
        }

//        [DllExport]
//        public static void initializeButtonsByID(int processId) => new Program().GenerateButtons(processId);
    }
}