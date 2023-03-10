using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

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
            #region G L Y P H
            var path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Fonts", "glyphs.ttf");
            privateFontCollection.AddFontFile(path);
            var fontFamily = privateFontCollection.Families[0];
            buttonTransparent.UseCompatibleTextRendering = true;
            buttonTransparent.Font = new Font(fontFamily, 24F);
            buttonTransparent.Text = "\uE801";
            #endregion G L Y P H

            buttonTransparent.FlatStyle= FlatStyle.Standard;

            buttonTransparent.MouseDown += onMoveableMouseDown;
            buttonTransparent.MouseMove += onMoveableMouseMove;
            buttonTransparent.Click += (sender, e) =>
            {
                if (!_isControlMoved)
                {
                    MessageBox.Show("Clicked!");
                    buttonTransparent.RestartWDT();
                }
            };

            path = Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Documents", "Document.rtf");
            richTextBox.Rtf = File.ReadAllText(path);
            richTextBox.MouseMove += (sender, e) =>
            {
                if(!MousePosition.Equals(_mousePrev))
                {
                    buttonTransparent.RestartWDT();
                }
                _mousePrev = MousePosition;
            };
            richTextBox.VScroll += (sender, e) =>
            {
                buttonTransparent.RestartWDT();
            };
        }
        Point _mousePrev = new Point(int.MaxValue, int.MaxValue);
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
        PrivateFontCollection privateFontCollection = new PrivateFontCollection();

        Point
            _mouseDownScreen = new Point(),
            _mouseDelta = new Point(),
            _controlDownPoint = new Point();
        bool _isControlMoved = false;

        private void onMoveableMouseDown(object? sender, MouseEventArgs e)
        {
            _isControlMoved = false;
            if (sender is Control control)
            {
                _mouseDownScreen = control.PointToScreen(e.Location);
                Text = $"{_mouseDownScreen}";
                _controlDownPoint = control.Location;
            }
        }

        private void onMoveableMouseMove(object? sender, MouseEventArgs e)
        {
            if (MouseButtons.Equals(MouseButtons.Left))
            {
                if (sender is Control control)
                {
                    var screen = control.PointToScreen(e.Location);
                    _mouseDelta = new Point(screen.X - _mouseDownScreen.X, screen.Y - _mouseDownScreen.Y);
                    Text = $"{_mouseDownScreen} {screen} {_mouseDelta}";
                    var newControlLocation = new Point(_controlDownPoint.X + _mouseDelta.X, _controlDownPoint.Y + _mouseDelta.Y);
                    if (!control.Location.Equals(newControlLocation))
                    {
                        control.Location = newControlLocation;
                        _isControlMoved = true;
                    }
                }
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
                        if (isInitial) await Task.Delay(250);
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
        /// <summary>
        /// Refresh after a watchdog time delay. For example
        /// in response to parent control mouse moves.
        /// </summary>  
        internal void RestartWDT(TimeSpan? timeSpan = null)
        {
            var captureCount = ++_wdtCount;
            var delay = timeSpan ?? TimeSpan.FromMilliseconds(250);
            Task.Delay(delay).GetAwaiter().OnCompleted(() =>
            {
                if (captureCount.Equals(_wdtCount))
                {
                    Debug.WriteLine($"WDT {delay}");
                    Refresh();
                }
            });
        }
        int _wdtCount = 0;
    }
}