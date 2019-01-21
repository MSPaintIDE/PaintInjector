using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NetFramework
{
    // Credit to https://stackoverflow.com/a/13536715/3929546
    public class ButtonTooltip : Form
    {
        public int Duration { get; set; }

        public ButtonTooltip(int x, int y, int width, int height, string message, int duration)
        {
            FormBorderStyle = FormBorderStyle.None;
            ShowInTaskbar = false;
            Width = width;
            Height = height;
            Duration = duration;
            Location = new Point(x, y);
            StartPosition = FormStartPosition.Manual;
            base.BackColor = Color.LightYellow;

            Label label = new Label();
            label.Text = message;
            label.TextAlign = ContentAlignment.MiddleLeft;
            label.Dock = DockStyle.Fill;

            Padding = new Padding(5);
            Controls.Add(label);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            TaskScheduler ui = TaskScheduler.FromCurrentSynchronizationContext();

            Task.Factory.StartNew(() => CloseAfter(Duration, ui));
        }

        private void CloseAfter(int duration, TaskScheduler ui)
        {
            Thread.Sleep(duration * 1000);

            Form form = this;

            Task.Factory.StartNew(
                () => form.Close(),
                CancellationToken.None,
                TaskCreationOptions.None,
                ui);
        }
    }
}