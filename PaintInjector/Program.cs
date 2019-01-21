using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Automation;
using System.Windows.Forms.VisualStyles;
using OpenTitlebarButtons.Native;
using OpenTitlebarButtons.Utils;

namespace NetFramework
{
    internal class Program
    {

        [STAThread]
        static void Main(string[] args) => new Program().Stuff();
        
        private EventManager _eventManager = new EventManager();
        private Bitmap _hoverLayer = (Bitmap) Resources.ResourceManager.GetObject("Hover_Layer");

        public void Stuff()
        {
            Process process = Process.GetProcessesByName("mspaint").FirstOrDefault();
            if (process == null)
            {
                Console.WriteLine("Cannot find any Paint process.");
                return;
            }

            var build = AddButton(process, "Build");
            build.Click += (sender, args) => Console.WriteLine("Build");
            
            var run = AddButton(process, "Run");
            run.Click += (sender, args) => Console.WriteLine("Run");
            
            var stop = AddButton(process, "Stop");
            stop.Click += (sender, args) => Console.WriteLine("Stop");
            
            var spacer = AddButton(process, "Spacer", hasHover: false);
            
            var commit = AddButton(process, "Commit");
            commit.Click += (sender, args) => Console.WriteLine("Commit");
            
            var push = AddButton(process, "Push");
            push.Click += (sender, args) => Console.WriteLine("Push");
            
            var pull = AddButton(process, "Pull");
            pull.Click += (sender, args) => Console.WriteLine("Pull");

            WaitForEverything();
        }

        private int lastX = 178;

        private TitlebarButtonHosterForm AddButton(Process process, string icon, bool hasHover = true, string tooltip = null)
        {
            var buttonHost = new TitlebarButtonHosterForm(_eventManager, new NativeUnmanagedWindow(process.MainWindowHandle));
            buttonHost.icon = (Bitmap) Resources.ResourceManager.GetObject(icon);
            if (hasHover) buttonHost.hoverIcon = GenerateHover(buttonHost.icon);
            buttonHost.XOffset = lastX;
            buttonHost.YOffset = 30;
            lastX += buttonHost.Width;

            return buttonHost;
        }

        private Bitmap GenerateHover(Bitmap input)
        {
            Bitmap hoverLayer = (Bitmap) _hoverLayer.Clone();
            Graphics.FromImage(hoverLayer).DrawImage(input, new Rectangle(0, 0, hoverLayer.Width, hoverLayer.Height));            
            return hoverLayer;
        }

        private void WaitForEverything()
        {
            // Thanks SO Ɛ> https://stackoverflow.com/a/2586635/3929546
            
            ManualResetEvent quitEvent = new ManualResetEvent(false);
        
            Console.CancelKeyPress += (sender, eArgs) => {
                quitEvent.Set();
                eArgs.Cancel = true;
            };

            quitEvent.WaitOne();
        }
    }
}