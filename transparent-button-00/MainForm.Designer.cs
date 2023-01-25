namespace transparent_button_00
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonTransparent = new transparent_button_00.TransparentButton();
            this.richTextBox = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // buttonTransparent
            // 
            this.buttonTransparent.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(22)))), ((int)(((byte)(22)))), ((int)(((byte)(22)))));
            this.buttonTransparent.ForeColor = System.Drawing.Color.SeaGreen;
            this.buttonTransparent.Location = new System.Drawing.Point(12, 60);
            this.buttonTransparent.Name = "buttonTransparent";
            this.buttonTransparent.Size = new System.Drawing.Size(175, 80);
            this.buttonTransparent.TabIndex = 7;
            this.buttonTransparent.Text = "Transparent Button";
            this.buttonTransparent.UseVisualStyleBackColor = true;
            // 
            // richTextBox
            // 
            this.richTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.richTextBox.Location = new System.Drawing.Point(0, 0);
            this.richTextBox.Name = "richTextBox";
            this.richTextBox.Size = new System.Drawing.Size(978, 644);
            this.richTextBox.TabIndex = 8;
            this.richTextBox.Text = "";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(10F, 25F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.ClientSize = new System.Drawing.Size(978, 644);
            this.Controls.Add(this.buttonTransparent);
            this.Controls.Add(this.richTextBox);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Main Form";
            this.ResumeLayout(false);

        }

        #endregion
        private TransparentButton buttonTransparent;
        private RichTextBox richTextBox;
    }
}