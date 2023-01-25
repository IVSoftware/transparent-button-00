This edited `TransparentButton` improves on my previous answer and no longer overrides to `OnPaint`. It requires no custom drawing. Instead, it uses `Graphics.CopyFromScreen` to make a screenshot of the rectangle behind the button and sets its own `Button.BackgroundImage` to the snipped bitmap. This way it's effectively camouflaged and appears transparent while still drawing as a Standard styled button.

[![clickable-draggable-over-richtextbox][1]][1]

Requirements:
- Create a button with transparent background.
- Do it _without_ drawing the background manually in `OnPaint`.
- Keep the `Button.FlatStyle` as `FlatStyle.Standard`.
- Do not disturb the rounded edges of the standard button.

***

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

***
**Design Mode Example**

Main form `BackgroundImage` to main form with a `Stretch` layout. Then overlay a `TableLayoutPanel` whose job it is to keep the button scaled correctly as the form resizes. `TransparentButton` is now placed in one of the cells. 

[![designer][2]][2]

[![runtime][3]][3]

***
**Test**

Here's the code I used to test the basic transparent button as shown over a background image: 

    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            buttonTransparent.ForeColor = Color.White;
            buttonTransparent.Click += onClickTransparent;
        }
        private void onClickTransparent(object? sender, EventArgs e)
        {
            MessageBox.Show("Clicked!");
            buttonTransparent.RestartWDT();
        }
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

***
**Revision 2.0 Demo**

The Master commit now demonstrates a draggable, clickable transparent button over a RichTextBox control.


  [1]: https://i.stack.imgur.com/PrYWw.png
  [2]: https://i.stack.imgur.com/XYlWr.png
  [3]: https://i.stack.imgur.com/3BfNe.png