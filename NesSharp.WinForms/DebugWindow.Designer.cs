namespace NesSharp.WinForms
{
    partial class DebugWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }

            if (disposing)
            {
                _buffer[0][0].Dispose();
                _buffer[0][1].Dispose();
                _buffer[1][0].Dispose();
                _buffer[1][1].Dispose();
            }

            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            patternTable0View = new PictureBox();
            patternTable1View = new PictureBox();
            label1 = new Label();
            label2 = new Label();
            paletteControl1 = new Debugging.PaletteControl();
            ((System.ComponentModel.ISupportInitialize)patternTable0View).BeginInit();
            ((System.ComponentModel.ISupportInitialize)patternTable1View).BeginInit();
            SuspendLayout();
            // 
            // patternTable0View
            // 
            patternTable0View.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            patternTable0View.Location = new Point(12, 85);
            patternTable0View.Name = "patternTable0View";
            patternTable0View.Size = new Size(256, 256);
            patternTable0View.SizeMode = PictureBoxSizeMode.Zoom;
            patternTable0View.TabIndex = 0;
            patternTable0View.TabStop = false;
            // 
            // patternTable1View
            // 
            patternTable1View.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            patternTable1View.Location = new Point(274, 85);
            patternTable1View.Name = "patternTable1View";
            patternTable1View.Size = new Size(256, 256);
            patternTable1View.SizeMode = PictureBoxSizeMode.Zoom;
            patternTable1View.TabIndex = 1;
            patternTable1View.TabStop = false;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label1.AutoSize = true;
            label1.Location = new Point(12, 67);
            label1.Name = "label1";
            label1.Size = new Size(83, 15);
            label1.TabIndex = 2;
            label1.Text = "Pattern table 0";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            label2.AutoSize = true;
            label2.Location = new Point(274, 67);
            label2.Name = "label2";
            label2.Size = new Size(83, 15);
            label2.TabIndex = 3;
            label2.Text = "Pattern table 1";
            // 
            // paletteControl1
            // 
            paletteControl1.Location = new Point(10, 12);
            paletteControl1.MaximumSize = new Size(1011, 52);
            paletteControl1.MinimumSize = new Size(1011, 52);
            paletteControl1.Name = "paletteControl1";
            paletteControl1.Size = new Size(1011, 52);
            paletteControl1.TabIndex = 4;
            // 
            // DebugWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1030, 353);
            Controls.Add(paletteControl1);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(patternTable1View);
            Controls.Add(patternTable0View);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "DebugWindow";
            Text = "NES# - Debug";
            ((System.ComponentModel.ISupportInitialize)patternTable0View).EndInit();
            ((System.ComponentModel.ISupportInitialize)patternTable1View).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox patternTable0View;
        private PictureBox patternTable1View;
        private Label label1;
        private Label label2;
        private Debugging.PaletteControl paletteControl1;
    }
}