This `TransparentButton` improves on my previous answer. It no longer requires an override to `OnPaint` and uses _no_ custom drawing whatsoever. What I missed the first time is to simply set the `Button.BackgroundImage` to the camouflage bitmap. 

[![runtime][1]][1]

Requirements:
- Create a button with transparent background.
- Do it _without_ drawing the background manually.
- Keep the `Button.FlatStyle` as `FlatStyle.Standard`.
- Do not disturb the rounded edges of the standard button.

***

    class TransparentButton : Button
    {
        public void SetParentForm(Form form)
        {
            _parentForm= form;
            captureBackground();
        }
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            captureBackground();
        }
        Form? _parentForm = null;
        private void captureBackground()
        {
            if (!(DesignMode || _parentForm == null))
            {
                // Hide this button before drawing
                Visible = false;

                // Draw the full container
                var tmp = (Bitmap)new Bitmap(_parentForm.Width, _parentForm.Height);
                _parentForm.DrawToBitmap(tmp, new Rectangle(0, 0, _parentForm.Width, _parentForm.Height));
                var ptScreen = _parentForm.PointToScreen(_parentForm.ClientRectangle.Location);
                var ptOffset = new Point(
                    ptScreen.X - _parentForm.Location.X,
                    ptScreen.Y - _parentForm.Location.Y);

                var clipBounds = new Rectangle(Location.X + ptOffset.X, Location.Y + ptOffset.Y, Width, Height);
                BackgroundImage = tmp.Clone(
                    clipBounds, 
                    System.Drawing.Imaging.PixelFormat.DontCare);

                // Show this button.
                Visible = true;
            }
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