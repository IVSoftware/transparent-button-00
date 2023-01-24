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

        // Determine which control to use for the background.
        HostForTesting HostForTesting => HostForTesting.MainForm;
       
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            switch (HostForTesting)
            {
                case HostForTesting.MainForm:
                    buttonTransparent.HostContainer = this;
                    break;
                case HostForTesting.TableLayoutPanel:
                    tableLayoutPanel.BackgroundImage = BackgroundImage;
                    tableLayoutPanel.BackgroundImageLayout = BackgroundImageLayout;
                    buttonTransparent.HostContainer = tableLayoutPanel;
                    break;
            }
            buttonTransparent.Click += onClickTransparent;
        }
        private void onClickTransparent(object? sender, EventArgs e)
        {
            MessageBox.Show("Clicked!");
        }
        protected override CreateParams CreateParams
        {
            get
            {
                // https://stackoverflow.com/a/36352503/5438626
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }
    }
    class TransparentButton : Button
    {
        int _wdtCount = 0;
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            if (!(DesignMode || _hostContainer == null)) 
            { 
                captureBackground();
            }
        }

        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Control? HostContainer
        {
            get => _hostContainer;
            set
            {
                if (!Equals(_hostContainer, value))
                {
                    _hostContainer = value;
                    if (_hostContainer != null)
                    {
                        captureBackground();
                    }
                }
            }
        }
        Control? _hostContainer = null;
        private void captureBackground()
        {
            if (HostContainer != null)
            {
                // Hide this button before drawing
                Visible = false;

                // Draw the full container
                var tmp = (Bitmap)new Bitmap(HostContainer.Width, HostContainer.Height);
                HostContainer.DrawToBitmap(tmp, new Rectangle(0, 0, HostContainer.Width, HostContainer.Height));

                // S A V E    f o r    D E B U G
                // tmp.Save("tmp.bmp");
                // Process.Start("explorer.exe", "tmp.bmp");

                if(HostContainer is Form form)
                {
                    var ptScreen = HostContainer.PointToScreen(HostContainer.ClientRectangle.Location);
                    var ptOffset = new Point(
                        ptScreen.X - HostContainer.Location.X,
                        ptScreen.Y - HostContainer.Location.Y);

                    var clipBounds = new Rectangle(Location.X + ptOffset.X, Location.Y + ptOffset.Y, Width, Height);
                    _chameleon = tmp.Clone(
                        clipBounds, 
                        System.Drawing.Imaging.PixelFormat.DontCare);
                }
                else
                {
                    _chameleon = tmp.Clone(
                        Bounds,
                        System.Drawing.Imaging.PixelFormat.DontCare);
                }

                // S A V E    f o r    D E B U G
                //_chameleon.Save("chm.bmp");
                //Process.Start("explorer.exe", "chm.bmp");

                // Show this button.
                Visible = true;
            }
        }
        private Bitmap? _chameleon = null;
        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
            {
                base.OnPaint(e);
            }
            else
            {
                // Important to check this when there are mutliple controls
                if (HostContainer != null) 
                {
                    if (_chameleon == null)
                    {
                        Debug.WriteLine(false, "Expecting background image");
                        base.OnPaint(e);
                    }
                    else
                    {
                        e.Graphics.DrawImage(_chameleon, new Point());
                    }
                }
                using (var brush = new SolidBrush(Color.White))
                {
                    e.Graphics.DrawString(Text, Font, brush, new PointF(10, 10));
                }
            }
        }
        protected override void OnMouseHover(EventArgs e)
        {
            base.OnMouseHover(e);
            var client = PointToClient(MousePosition);
            if (!IsDisposed)
            {
                ToolTip.Show(
                    "Mouse is over an invisible button!",
                    this,
                    new Point(client.X + 10, client.Y - 25),
                    1000
                );
            }
        }
        private static ToolTip ToolTip { get; } = new ToolTip();
    }
}