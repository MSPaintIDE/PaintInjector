using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows.Automation;
using OpenTitlebarButtons.Native;
using OpenTitlebarButtons.Utils;

namespace NetFramework
{
    public class Program
    {   
        static void Main(string[] args) => new Program().GenerateButtons();
        
        static Program() => AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
            => Assembly.LoadFrom($@"{new FileInfo(args.RequestingAssembly.Location).DirectoryName}\{args.Name.Split(',')[0]}.dll");

        private static EventManager _eventManager;
        private static Bitmap _hoverLayer;
        private static AutomationElement window;
        private static AutomationElement statusBar;
        private static AutomationElement statusText;

        private static TextHoster _textHoster;
        

        internal void GenerateButtons(int processId = -1)
        {
           var thread = new Thread(() =>
            {
                _eventManager = new EventManager();
                _hoverLayer = (Bitmap) Resources.ResourceManager.GetObject("Hover_Layer");

                var process = processId != -1 ? Process.GetProcessById(processId) : Process.GetProcessesByName("mspaint").FirstOrDefault();
                if (process == null)
                {
                    Console.WriteLine("Cannot find any Paint process" + (processId == -1 ? "." : " With the process ID of " + processId + "."));
                    return;
                }

                window = AutomationElement.FromHandle(process.MainWindowHandle);
                statusBar = window.FindFirst(TreeScope.Children,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.StatusBar));

                statusText = statusBar.FindAll(TreeScope.Subtree,
                    new PropertyCondition(AutomationElement.ControlTypeProperty, ControlType.Edit))[4];

                var build = AddButton(process, "Build");
                build.Click += (sender, args) => JavaInterface.RunCallback(CallbackType.Build);

                var run = AddButton(process, "Run", tooltip: "Runs the current file");
                run.Click += (sender, args) => JavaInterface.RunCallback(CallbackType.Run);

                var stop = AddButton(process, "Stop", tooltip: "Stops the execution of the current program");
                stop.Click += (sender, args) => JavaInterface.RunCallback(CallbackType.Stop);

                AddButton(process, "Spacer", space: true);

                var commit = AddButton(process, "Commit", tooltip: "Commits all files in the project");
                commit.Click += (sender, args) => JavaInterface.RunCallback(CallbackType.Commit);

                var push = AddButton(process, "Push", tooltip: "Pushes all files in the project");
                push.Click += (sender, args) => JavaInterface.RunCallback(CallbackType.Push);

                var pull = AddButton(process, "Pull", tooltip: "Updates the project from git");
                pull.Click += (sender, args) => JavaInterface.RunCallback(CallbackType.Pull);

                _textHoster = AddText(process);

                WaitForEverything();
            });
           
           thread.SetApartmentState(ApartmentState.STA);
           thread.Start();
            
        }

        private static int _lastX = 178;

        private TextHoster AddText(Process process, string text = null)
        {
            var textHost = new TextHoster(_eventManager, new NativeUnmanagedWindow(process.MainWindowHandle));

            var prevWidth = -1;
            textHost.CalculateCoords += (sender, args) =>
            {   
                var statusRect = statusText.Current.BoundingRectangle;
                var windowRect = window.Current.BoundingRectangle;

                args.X += (int) (statusRect.X - windowRect.X);
                args.Y += (int) (statusRect.Y - windowRect.Y + statusRect.Height * 0.75 - textHost.Height);
                
                var nextWidth = (int) statusText.Current.BoundingRectangle.Width;
                if (nextWidth == prevWidth) return;
                
                textHost.Width = prevWidth = nextWidth;
                textHost.ChangeBounds(textHost.Left, textHost.Top, nextWidth, textHost.Height);

                textHost.Redraw();
            };

            textHost.BackgroundColor = Color.FromArgb(240, 240, 240);
            textHost.FontSize = 11;
            textHost.SetText(text, false);

            textHost.Relocate();

            return textHost;
        }

        private ButtonHoster AddButton(Process process, string iconName, bool hasHover = true, bool space = false, string tooltip = null)
        {
            var buttonHost = new ButtonHoster(_eventManager, new NativeUnmanagedWindow(process.MainWindowHandle));

            var icon = (Bitmap) Resources.ResourceManager.GetObject(iconName);
            buttonHost.icon = AddBackground(icon, !space);
            if (!space && hasHover) buttonHost.hoverIcon = GenerateHover(icon);
            buttonHost.XOffset = _lastX;
            buttonHost.YOffset = 30;
            _lastX += buttonHost.Width;

            if (tooltip != null) buttonHost.Hover += (sender, args) => { _textHoster.SetText(tooltip, false); };

            return buttonHost;
        }

        // This is needed because WindowFromPoint sees through transparency. It just emulates the normal background behind it
        private static Bitmap AddBackground(Bitmap input, bool normal = true)
        {
            var width = normal ? 25 : input.Width;
            var height = normal ? 25 : input.Height;
            var coloredLayer = new Bitmap(width, height);
            var graphics = Graphics.FromImage(coloredLayer);
            graphics.Clear(Color.White);
            graphics.DrawLine(new Pen(Color.FromArgb(218, 219, 220)), 0, height - 1, width,
                height - 1);
            var offset = normal ? 2 : 0;
            graphics.DrawImage((Image) input.Clone(), new Rectangle(offset, offset, input.Width, input.Height));
            return coloredLayer;
        }

        private Bitmap GenerateHover(Bitmap input)
        {
            var hoverLayer = (Bitmap) _hoverLayer.Clone();
            Graphics.FromImage(hoverLayer).DrawImage(input, new Rectangle(2, 2, 21, 21));
            return hoverLayer;
        }

        private static void WaitForEverything()
        {
            // Thanks SO Ɛ> https://stackoverflow.com/a/2586635/3929546

            ManualResetEvent quitEvent = new ManualResetEvent(false);

            Console.CancelKeyPress += (sender, eArgs) =>
            {
                quitEvent.Set();
                eArgs.Cancel = true;
            };

            quitEvent.WaitOne();
        }
    }
}