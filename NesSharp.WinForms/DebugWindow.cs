using NesSharp.Core;
using NesSharp.WinForms.Debugging;
using System.Drawing.Imaging;

namespace NesSharp.WinForms
{
    public partial class DebugWindow : Form, IDebugOutput
    {
        private Ppu? _ppu;

        private readonly RgbColor[][] _patternTables = new RgbColor[2][];
        private readonly Bitmap[][] _buffer = new Bitmap[2][];
        private int _bufferIndex;
        private static readonly Rectangle PatternTableRect = new(0, 0, 128, 128);

        public DebugWindow()
        {
            InitializeComponent();

            _patternTables[0] = new RgbColor[128 * 128];
            _patternTables[1] = new RgbColor[128 * 128];

            _buffer[0] = new Bitmap[2];
            _buffer[1] = new Bitmap[2];

            _buffer[0][0] = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
            _buffer[0][1] = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
            _buffer[1][0] = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
            _buffer[1][1] = new Bitmap(128, 128, PixelFormat.Format32bppArgb);
        }

        public void SetPpu(Ppu ppu)
        {
            _ppu = ppu;

            CreatePatternTableImage(0, patternTable0View);
            CreatePatternTableImage(1, patternTable1View);
            IncreaseBufferIndex();

            paletteControl1.SetPpu(ppu);
        }

        public void DebugUpdate()
        {
            CreatePatternTableImage(0, patternTable0View);
            CreatePatternTableImage(1, patternTable1View);
            IncreaseBufferIndex();

            paletteControl1.DebugUpdate();
        }

        private void IncreaseBufferIndex()
        {
            _bufferIndex = (_bufferIndex + 1) % 2;
        }

        private void CreatePatternTableImage(byte index, PictureBox target)
        {
            BitmapData? bitmapData = null;
            var bitmap = _buffer[index][_bufferIndex];

            try
            {
                bitmapData = bitmap.LockBits(PatternTableRect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                unsafe
                {
                    var dstPointer = (byte*)bitmapData.Scan0.ToPointer();
                    var src = _ppu!.GetPatternTable(_patternTables[index], index);

                    for (var y = 0; y < 128; y++)
                    {
                        for (var x = 0; x < 128; x++)
                        {
                            var pixel = src[y * 128 + x];
                            var offset = (y * bitmapData.Stride) + (x * 4);

                            dstPointer[offset + 0] = pixel.B;
                            dstPointer[offset + 1] = pixel.G;
                            dstPointer[offset + 2] = pixel.R;
                            dstPointer[offset + 3] = 255;
                        }
                    }
                }
            }
            catch
            {
                // unused
            }
            finally
            {
                if (bitmapData is not null)
                {
                    bitmap.UnlockBits(bitmapData);
                }

                try
                {
                    if (target is { InvokeRequired: true, IsDisposed: false })
                    {
                        target.Invoke(() => target.Image = bitmap);
                    }
                    else if (!target.IsDisposed)
                    {
                        target.Image = bitmap;
                    }
                }
                catch
                {
                    // unused
                }
            }
        }
    }
}
