As anyone who has ever had to cloak a starship knows, one way to make something "invisible" is to paint its surface to look like what's behind it. To experiment with this, start with a main form that has a background image as a `Stretch` layout. Then overlay a `TableLayoutPanel` whose job it is to keep the button scaled correctly as the form resizes. Finally, place an instance of `TransparentButton` in one of the cells.

Here's an example of a class that responds to `SizeChanged` events by capturing a bitmap of the designated HostControl.

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
