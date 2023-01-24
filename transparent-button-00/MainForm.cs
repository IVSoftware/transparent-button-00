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
    class TransparentButton : Button
    {
        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            Refresh();
        }
        public void SetParentForm(Form form)
        {
            _parentForm= form;
            Refresh();
        }
        Form? _parentForm = null;
        public new void Refresh()
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
            base.Refresh();
        }
    }
}