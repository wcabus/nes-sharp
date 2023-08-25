using System.Drawing.Imaging;
using NesSharp.Core;
using NesSharp.Core.Sound;
using NesSharp.WinForms.Sound;

namespace NesSharp.WinForms
{
    public partial class EmulatorWindow : Form
    {
        private Thread? _emulationThread;
        private CancellationTokenSource _emulatorCancellationTokenSource = new();
        private CancellationToken _cancellationToken = default;

        private DateTime _time1;
        private DateTime _time2;

        private float _residualTime;
        private Bus? _nesSystem;

        private readonly Dictionary<Keys, bool> _playerOne = new();

        private readonly Bitmap[] _buffer = new Bitmap[2];
        private int _bufferIndex;
        private static readonly Rectangle Rect = new(0, 0, 256, 240);

        private readonly ISoundDriver _soundDriver = new NAudioSoundDriver();

        private DebugWindow? _debugWindow;
        
        public EmulatorWindow()
        {
            InitializeComponent();

            _buffer[0] = new Bitmap(256, 240, PixelFormat.Format32bppArgb);
            _buffer[1] = new Bitmap(256, 240, PixelFormat.Format32bppArgb);
        }

        private void EmulatorWindow_Load(object sender, EventArgs e)
        {
            _nesSystem = new Bus();

            InitializeInput();
            InitializeSound();           
        }

        private void EmulatorWindow_Closing(object sender, FormClosingEventArgs e)
        {
            _emulatorCancellationTokenSource.Cancel();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void showDebugWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_debugWindow is not null)
            {
                _debugWindow.Focus();
                return;
            }

            _debugWindow = new DebugWindow();
            _debugWindow.FormClosed += OnDebugWindowClosed;
            _debugWindow.Show();
            _debugWindow.SetPpu(_nesSystem!.Ppu);
        }

        private void OnDebugWindowClosed(object? sender, FormClosedEventArgs e)
        {
            _debugWindow?.Dispose();
            _debugWindow = null;
        }

        #region Emulation

        private async void OnLoadRomClicked(object sender, EventArgs e)
        {
            StopEmulator();

            using var openFileDialog = new OpenFileDialog();

            openFileDialog.Filter = @"NES ROM files (*.nes)|*.nes|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;
            openFileDialog.RestoreDirectory = true;
            openFileDialog.AutoUpgradeEnabled = true;
            openFileDialog.CheckFileExists = true;

            if (openFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            var fileName = openFileDialog.FileName;
            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            var cartridge = await Cartridge.FromFile(fileName);
            StartEmulator(cartridge);
        }

        private void StartEmulator(Cartridge cartridge)
        {
            _nesSystem!.InsertCartridge(cartridge);

            _nesSystem.Reset();
            _emulatorCancellationTokenSource = new();

            var cancelToken = _emulatorCancellationTokenSource.Token;

            _emulationThread = new Thread(() =>
            {
                PrepareEmulator();
                while (!cancelToken.IsCancellationRequested)
                {
                    UpdateGame(cancelToken);
                }
            });
            _emulationThread.Start();

            _cancellationToken = cancelToken;
        }

        private void PrepareEmulator()
        {
            _time1 = DateTime.UtcNow;
            _time2 = DateTime.UtcNow;
        }

        private void UpdateGame(CancellationToken cancellationToken)
        {
            _time2 = DateTime.UtcNow;
            var elapsed = _time2 - _time1;
            _time1 = _time2;

            var elapsedTime = (float)elapsed.Ticks;
                        
            RunEmulator(elapsedTime, cancellationToken);
        }

        private void RunEmulator(float elapsedTime, CancellationToken cancellationToken)
        {
            _nesSystem!.SetControllerState(0, _playerOne[P1KeyUp], _playerOne[P1KeyDown], _playerOne[P1KeyLeft], _playerOne[P1KeyRight], _playerOne[P1KeyStart], _playerOne[P1KeySelect], _playerOne[P1KeyA], _playerOne[P1KeyB]);

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _nesSystem.Ppu.FrameComplete = false;

            UpdateEmulatorOutputAndDebugWindow();
        }

        private void UpdateEmulatorOutputAndDebugWindow()
        {
            // Draw the screen
            BitmapData? bitmapData = null;
            var bitmap = _buffer[_bufferIndex];

            try
            {
                bitmapData = bitmap.LockBits(Rect, ImageLockMode.WriteOnly, bitmap.PixelFormat);
                unsafe
                {
                    var dstPointer = (byte*)bitmapData.Scan0.ToPointer();
                    var src = _nesSystem!.Ppu.Screen;

                    for (var y = 0; y < 240; y++)
                    {
                        for (var x = 0; x < 256; x++)
                        {
                            var pixel = src[y * 256 + x];
                            var offset = (y * bitmapData.Stride) + (x * 4);

                            dstPointer[offset + 0] = pixel.B;
                            dstPointer[offset + 1] = pixel.G;
                            dstPointer[offset + 2] = pixel.R;
                            dstPointer[offset + 3] = 255;
                        }
                    }
                }
            }
            finally
            {
                if (bitmapData is not null)
                {
                    bitmap.UnlockBits(bitmapData);
                }

                try
                {
                    emulatorOutput.Invoke(() => { emulatorOutput.Image = bitmap; });
                }
                catch
                {
                    // ignored
                }

                // Swap buffers
                _bufferIndex++;
                _bufferIndex %= 2;
            }

            _debugWindow?.DebugUpdate();
        }

        private void StopEmulator()
        {
            _nesSystem!.Stop();
            _emulatorCancellationTokenSource.Cancel();
        }

        #endregion

        #region Input

        // Player 1 Inputs
        public Keys P1KeyUp { get; set; }
        public Keys P1KeyDown { get; set; }
        public Keys P1KeyLeft { get; set; }
        public Keys P1KeyRight { get; set; }
        public Keys P1KeyStart { get; set; }
        public Keys P1KeySelect { get; set; }
        public Keys P1KeyA { get; set; }
        public Keys P1KeyB { get; set; }

        private void InitializeInput()
        {
            P1KeyUp = Keys.W;
            P1KeyDown = Keys.S;
            P1KeyLeft = Keys.A;
            P1KeyRight = Keys.D;
            P1KeyStart = Keys.Enter;
            P1KeySelect = Keys.Back;
            P1KeyA = Keys.O;
            P1KeyB = Keys.K;

            _playerOne.Add(P1KeyUp, false);
            _playerOne.Add(P1KeyDown, false);
            _playerOne.Add(P1KeyLeft, false);
            _playerOne.Add(P1KeyRight, false);
            _playerOne.Add(P1KeyStart, false);
            _playerOne.Add(P1KeySelect, false);
            _playerOne.Add(P1KeyA, false);
            _playerOne.Add(P1KeyB, false);
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (_playerOne.ContainsKey(e.KeyCode))
            {
                _playerOne[e.KeyCode] = true;
            }
        }

        private void OnKeyUp(object sender, KeyEventArgs e)
        {
            if (_playerOne.ContainsKey(e.KeyCode))
            {
                _playerOne[e.KeyCode] = false;
            }
        }

        #endregion

        #region Sound

        private void InitializeSound()
        {
            _nesSystem!.SetSampleFrequency(44100);

            _soundDriver.InitializeAudio(44100, 1, 16, 512);
            _soundDriver.SetSoundOutMethod(SoundOut);
        }

        private float SoundOut(uint channel, float globalTime, float timeStep)
        {
            if (_nesSystem is null || !_nesSystem.IsRunning)
            {
                return 0f;
            }

            if (channel == 0)
            {
                while (!_nesSystem.Clock() && !_cancellationToken.IsCancellationRequested)
                {
                    // keep emulating until we have a sample to play. This is to keep the timing of the sound accurate.
                }

                return (float)_nesSystem.AudioSample;
            }

            return 0f;
        }

        private void DisposeSoundOutput()
        {
            _soundDriver.Dispose();
        }

        #endregion

        private void DisposeBuffers()
        {
            StopEmulator();

            _buffer[0].Dispose();
            _buffer[1].Dispose();
        }
    }
}