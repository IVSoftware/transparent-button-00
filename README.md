As anyone who has ever had to cloak a starship knows, one way to make something look invisible is to paint its surface to look like what's behind it.


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
            if (_hostContainer != null)
            {
                // Hide this button before drawing
                Visible = false;

                // Draw the full container
                var tmp = (Bitmap)new Bitmap(_hostContainer.Width, _hostContainer.Height);
                _hostContainer.DrawToBitmap(tmp, new Rectangle(0, 0, _hostContainer.Width, _hostContainer.Height));

                // S A V E    f o r    D E B U G
                // tmp.Save("tmp.bmp");
                // Process.Start("explorer.exe", "tmp.bmp");

                if(_hostContainer is Form form)
                {
                    var ptScreen = _hostContainer.PointToScreen(_hostContainer.ClientRectangle.Location);
                    var ptOffset = new Point(
                        ptScreen.X - _hostContainer.Location.X,
                        ptScreen.Y - _hostContainer.Location.Y);

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
            ToolTip.Show(
                "Mouse is over an invisible button!",
                this,
                new Point(client.X + 10, client.Y - 25),
                1000
            );
        }
        private static ToolTip ToolTip { get; } = new ToolTip();
    }
