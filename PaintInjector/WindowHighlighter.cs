using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenTitlebarButtons.Native;
using OpenTitlebarButtons.Utils;
using Timer = System.Timers.Timer;

namespace NetFramework
{
    public class WindowHighlighter : ElementHoster
    {
        private readonly Program _program;
        public bool Clicked;
        public bool Exit;
        private IntPtr _highlightingWindow = IntPtr.Zero;
        private readonly Dictionary<IntPtr, IntPtr> _windowCache = new Dictionary<IntPtr, IntPtr>();

        public WindowHighlighter(Program program, EventManager eventManager, NativeUnmanagedWindow parent, Program.SelectedPaint selectedPaint) : base(
            eventManager, parent)
        {
            _program = program;
            const int brushWidth = 2;
            var pen = new Pen(Color.Red, brushWidth);

            WindowState = FormWindowState.Maximized;
            Focus();
            Show();
            WindowState = FormWindowState.Normal;

            CalculateCoords += (sender, args) =>
            {
                if (_highlightingWindow == IntPtr.Zero) return;

                var width = ParentWindow.Bounds.Width;
                var height = ParentWindow.Bounds.Height;
                args.Y -= 1;
                const int x = 9;
                const int y = 0;

                var bitmap = new Bitmap(width, height);
                var graphics = Graphics.FromImage(bitmap);

                width -= 14;
                height -= 7 - 1;

                graphics.DrawRectangle(pen, x, y + 2, width - 4, height - 4);
                graphics.DrawLine(pen, x, 3, width + 5, 3);
                SetBitmap(bitmap);
            };

            var handle1 = Handle;

            Relocate();
            SetWindowPos(handle1, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);

            Cursor.Current = Cursors.Cross;

            var lastPos = Point.Empty;

            var t = new Timer {SynchronizingObject = this, Interval = 500, AutoReset = true};
            t.Elapsed += (sender, args) =>
            {

                if (Exit)
                {
                    t.Close();
                    Close();
                    return;
                }
                
                var currentPoint = Cursor.Position;

                if (lastPos != Point.Empty && lastPos == currentPoint) goto clicked;
                lastPos = currentPoint;

                var handle = WindowFromPoint(Cursor.Position);
                if (handle == handle1) goto clicked;
                var hWnd = GetRoot(handle, handle);
                if (hWnd == IntPtr.Zero) goto clicked;

                if (_highlightingWindow != IntPtr.Zero && hWnd == _highlightingWindow) goto clicked;
                _highlightingWindow = hWnd;
                var unmanagedWindow = new NativeUnmanagedWindow(hWnd);

                Attach(unmanagedWindow, false);
                SetWindowPos(handle1, HWND_TOPMOST, 0, 0, 0, 0, TOPMOST_FLAGS);
                Relocate();

                clicked:
                if (!Clicked) return;
                Clicked = false;

                GetWindowThreadProcessId(ParentWindow.Handle, out var processId);
                var process = Process.GetProcessById(processId);
                if (process.ProcessName != "mspaint") return;

                program.DisposeEverything();
                t.Close();
                Close();
                selectedPaint(true, processId);
            };
            t.Start();
        }

        private IntPtr GetRoot(IntPtr original, IntPtr handle)
        {
            if (_windowCache.ContainsKey(handle)) return _windowCache[handle];
            IntPtr result = GetParent(handle);
            if (result != IntPtr.Zero) return GetRoot(original, result);
            _windowCache[original] = handle;
            return handle;
        }

        [DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
        public static extern IntPtr GetParent(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern IntPtr WindowFromPoint(Point p);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);

        static readonly IntPtr HWND_NOTOPMOST = new IntPtr(-2);

        static readonly IntPtr HWND_TOP = new IntPtr(0);

        static readonly IntPtr HWND_BOTTOM = new IntPtr(1);

        const UInt32 SWP_NOSIZE = 0x0001;

        const UInt32 SWP_NOMOVE = 0x0002;

        const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy,
            uint uFlags);

        [DllImport("user32.dll", SetLastError = true)]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int processId);
    }
}