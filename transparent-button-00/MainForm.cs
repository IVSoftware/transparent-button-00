using System.ComponentModel;
using System.Diagnostics;

namespace transparent_button_00
{
    enum HostForTesting
    {
        MainForm,
        TableLayoutPanel,
    }
    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();
       
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            buttonTransparent.ForeColor= Color.White;
            buttonTransparent.Click += onClickTransparent;
        }
        private void onClickTransparent(object? sender, EventArgs e) =>
            MessageBox.Show("Clicked!");
        protected override CreateParams CreateParams
        {
            get
            {
                const int WS_EX_COMPOSITED = 0x02000000;
                // https://stackoverflow.com/a/36352503/5438626
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= WS_EX_COMPOSITED;
                return cp;
            }
        }
    }
    class TransparentButton : Button
    {
        public TransparentButton() => Paint += (sender, e) =>
        {
            // Detect size/location changes
            if ((Location != _prevLocation) || (Size != _prevSize))
            {
                Refresh();
            }
            _prevLocation = Location;
            _prevSize = Size;
        };

        Point _prevLocation = new Point(int.MaxValue, int.MaxValue);
        Size _prevSize = new Size(int.MaxValue, int.MaxValue);

        public new void Refresh()
        {
            if (!DesignMode)
            {
                bool isInitial = false;
                if ((BackgroundImage == null) || !BackgroundImage.Size.Equals(Size))
                {
                    isInitial = true;
                    BackgroundImage = new Bitmap(Width, Height);
                }
                if (MouseButtons.Equals(MouseButtons.None))
                {
                    // Hide button, take screenshot, show button again
                    Visible = false;
                    BeginInvoke(async () =>
                    {
                        Parent?.Refresh();
                        if (isInitial) await Task.Delay(100);
                        using (var graphics = Graphics.FromImage(BackgroundImage))
                        {
                            graphics.CopyFromScreen(PointToScreen(new Point()), new Point(), Size);
                        }
                        Visible = true;
                    });
                }
                else
                {
                    using (var graphics = Graphics.FromImage(BackgroundImage))
                        graphics.FillRectangle(Brushes.LightGray, graphics.ClipBounds);
                }
            }
            else base.Refresh();
        }
        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            Refresh();
        }
    }
}