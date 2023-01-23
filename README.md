One way to make something "invisible" is to paint its surface to look like what's behind it. To experiment with this, start with a main form that has a background image as a `Stretch` layout. Then overlay a `TableLayoutPanel` whose job it is to keep the button scaled correctly as the form resizes and could for the basis for some image-mapping.  Finally, place an instance of `TransparentButton` in one of the cells. Here are the steps in the form designer:

![designer]()

The `TransparentButton` class responds to `SizeChanged` events by capturing a bitmap of the designated HostControl. At runtime, the `OnPaint` method draws the background where the button is supposed to be. But even though it's "invisible", clicking on the camoflauged button still raises the click. The code below is a proof of concept only. One would definitely want to do more rigorous testing than I have.



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
            Task.Delay(5000).GetAwaiter().OnCompleted(() => OnMouseHover(EventArgs.Empty));
        }

This property provides some flexibility for where the background is clipped.

        Control? _hostContainer = null;
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

Take a snapshot of the background.

        private Bitmap? _chameleon = null;
        private void captureBackground()
        {
            if (HostContainer != null)
            {
                // Hide this button before drawing
                Visible = false;

                // Draw the full container
                var tmp = (Bitmap)new Bitmap(HostContainer.Width, HostContainer.Height);
                HostContainer.DrawToBitmap(tmp, new Rectangle(0, 0, HostContainer.Width, HostContainer.Height));
                
                if(HostContainer is Form form)
                {
                    // Metrics are a little trickier because ClientRectangle
                    // is offset from the location and smaller than width/height.
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
                // Show this button.
                Visible = true;
            }
        }

Do not call the base class Paint. Draw the snipped image instead.

        protected override void OnPaint(PaintEventArgs e)
        {
            if (DesignMode)
            {
                base.OnPaint(e);
            }
            else
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
        }

Tool tip

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

    ***
    **Test**

    Here's the code I used to test this answer. 

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