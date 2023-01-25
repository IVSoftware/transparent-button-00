This edited `TransparentButton` improves on my previous answer. It no longer requires an override to `OnPaint` and uses _no_ custom drawing whatsoever. What I missed the first time is to simply set the `Button.BackgroundImage` to the camouflage bitmap. 

[![runtime][1]][1]

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

        int _wdtCount = 0;
        internal void RestartWDT(TimeSpan? timeSpan = null)
        {
            var captureCount = ++_wdtCount;
            var delay = timeSpan ?? TimeSpan.FromMilliseconds(250);
            Task
                .Delay(delay)
                .GetAwaiter()
                .OnCompleted(() => 
                {
                    if(captureCount.Equals(_wdtCount))
                    {
                        Debug.WriteLine($"WDT {delay}");
                        Refresh();
                    }
                });
        }
    }

***
**Design Mode Example**

Main form `BackgroundImage` to main form with a `Stretch` layout. Then overlay a `TableLayoutPanel` whose job it is to keep the button scaled correctly as the form resizes. `TransparentButton` is now placed in one of the cells. 

[![designer][2]][2]

***
**Test**

Here's the code I used to test this answer: 

    public partial class MainForm : Form
    {
        public MainForm() => InitializeComponent();       
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            buttonTransparent.FlatStyle= FlatStyle.Standard;
            buttonTransparent.SetParentForm(this);
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


  [1]: https://i.stack.imgur.com/0w60t.png
  [2]: https://i.stack.imgur.com/XYlWr.png