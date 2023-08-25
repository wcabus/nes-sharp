using NesSharp.Core;

namespace NesSharp.WinForms.Debugging
{
    public partial class PaletteControl : UserControl, IDebugOutput
    {
        private Ppu? _ppu;

        public PaletteControl()
        {
            InitializeComponent();
        }

        public void SetPpu(Ppu ppu)
        {
            _ppu = ppu;
            GetPalettes();
        }

        public void DebugUpdate()
        {
            GetPalettes();
        }

        private void GetPalettes()
        {
            for (var p = 0; p < 8; p++)
            {
                for (var c = 0; c < 4; c++)
                {
                    var color = _ppu!.GetColorFromPalette((byte)p, (byte)c);
                    var control = Controls.Find($"palette{p}{c}", true).FirstOrDefault() as PictureBox;

                    if (control is null)
                        continue;

                    if (control is { InvokeRequired: true, IsDisposed: false })
                    {
                        control.Invoke(() => control.BackColor = Color.FromArgb(color.R, color.G, color.B));
                    }
                    else if (!control.IsDisposed)
                    {
                        control.BackColor = Color.FromArgb(color.R, color.G, color.B);
                    }
                }
            }
        }
    }
}
