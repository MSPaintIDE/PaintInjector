using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using OpenTitlebarButtons.Native;
using OpenTitlebarButtons.Utils;

namespace NetFramework
{
    internal class Program
    {

        [STAThread]
        static void Main(string[] args) => new Program().Stuff();
        
        private EventManager _eventManager = new EventManager();

        public void Stuff()
        {
            Process process = Process.GetProcessesByName("mspaint").FirstOrDefault();
            if (process == null)
            {
                Console.WriteLine("Cannot find any Paint process.");
                return;
            }

            new Thread(() => {
                while (true)
                {
                }
            }).Start();

            var build = AddButton(process, "Build", "Build_Hover");
            build.Click += (sender, args) => Console.WriteLine("Build");
            
            var run = AddButton(process, "Run", "Run_Hover");
            run.Click += (sender, args) => Console.WriteLine("Run");
            
            var stop = AddButton(process, "Stop", "Stop_Hover");
            stop.Click += (sender, args) => Console.WriteLine("Stop");
        }

        private int lastX = 178;

        private TitlebarButtonHosterForm AddButton(Process process, string icon, string hoverIcon)
        {
            var buttonHost = new TitlebarButtonHosterForm(_eventManager, new NativeUnmanagedWindow(process.MainWindowHandle));
            buttonHost.icon = (Bitmap) Resources.ResourceManager.GetObject(icon);
            buttonHost.hoverIcon = (Bitmap) Resources.ResourceManager.GetObject(hoverIcon);
            buttonHost.XOffset = lastX;
            buttonHost.YOffset = 30;
            lastX += 25;

            return buttonHost;
        }
    }
}