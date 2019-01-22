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
        public static void initializeButtons() => new Program().GenerateButtons();
    }
}