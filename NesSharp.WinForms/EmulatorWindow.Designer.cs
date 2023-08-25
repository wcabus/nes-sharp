namespace NesSharp.WinForms
{
    partial class EmulatorWindow
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
            if (disposing)
            {
                DisposeBuffers();
                DisposeSoundOutput();
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
            emulatorOutput = new PictureBox();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            loadROMToolStripMenuItem = new ToolStripMenuItem();
            toolStripMenuItem1 = new ToolStripSeparator();
            exitToolStripMenuItem = new ToolStripMenuItem();
            debugToolStripMenuItem = new ToolStripMenuItem();
            showDebugWindowToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)emulatorOutput).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // emulatorOutput
            // 
            emulatorOutput.Dock = DockStyle.Fill;
            emulatorOutput.Location = new Point(0, 24);
            emulatorOutput.Name = "emulatorOutput";
            emulatorOutput.Size = new Size(752, 657);
            emulatorOutput.SizeMode = PictureBoxSizeMode.Zoom;
            emulatorOutput.TabIndex = 0;
            emulatorOutput.TabStop = false;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, debugToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(752, 24);
            menuStrip1.TabIndex = 1;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { loadROMToolStripMenuItem, toolStripMenuItem1, exitToolStripMenuItem });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // loadROMToolStripMenuItem
            // 
            loadROMToolStripMenuItem.Name = "loadROMToolStripMenuItem";
            loadROMToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
            loadROMToolStripMenuItem.Size = new Size(182, 22);
            loadROMToolStripMenuItem.Text = "&Load ROM...";
            loadROMToolStripMenuItem.Click += OnLoadRomClicked;
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(179, 6);
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
            exitToolStripMenuItem.Size = new Size(182, 22);
            exitToolStripMenuItem.Text = "&Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // debugToolStripMenuItem
            // 
            debugToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { showDebugWindowToolStripMenuItem });
            debugToolStripMenuItem.Name = "debugToolStripMenuItem";
            debugToolStripMenuItem.Size = new Size(54, 20);
            debugToolStripMenuItem.Text = "&Debug";
            // 
            // showDebugWindowToolStripMenuItem
            // 
            showDebugWindowToolStripMenuItem.Name = "showDebugWindowToolStripMenuItem";
            showDebugWindowToolStripMenuItem.Size = new Size(197, 22);
            showDebugWindowToolStripMenuItem.Text = "&Show Debug Window...";
            showDebugWindowToolStripMenuItem.Click += showDebugWindowToolStripMenuItem_Click;
            // 
            // EmulatorWindow
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(752, 681);
            Controls.Add(emulatorOutput);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "EmulatorWindow";
            Text = "NES#";
            FormClosing += EmulatorWindow_Closing;
            Load += EmulatorWindow_Load;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            ((System.ComponentModel.ISupportInitialize)emulatorOutput).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox emulatorOutput;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem debugToolStripMenuItem;
        private ToolStripMenuItem showDebugWindowToolStripMenuItem;
        private ToolStripMenuItem loadROMToolStripMenuItem;
        private ToolStripSeparator toolStripMenuItem1;
    }
}