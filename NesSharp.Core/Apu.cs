namespace NesSharp.Core;

/// <summary>
/// Audio Processing Unit
/// </summary>
public sealed class Apu
{
    private uint _clockCounter;
    private uint _frameClockCounter;
    private double _globalTime;

    private bool _useRawMode = false;

    private bool _pulse1Enable;
    private bool _pulse1Halt;
    private double _pulse1Sample;
    private double _pulse1Output;
    private Sequencer _pulse1Sequencer = new();
    private OscillatorPulse _pulse1Oscillator = new();
    private Envelope _pulse1Envelope = new();
    private LengthCounter _pulse1LengthCounter = new();
    private Sweeper _pulse1Sweep = new();

    private bool _pulse2Enable;
    private bool _pulse2Halt;
    private double _pulse2Sample;
    private double _pulse2Output;
    private Sequencer _pulse2Sequencer = new();
    private OscillatorPulse _pulse2Oscillator = new();
    private Envelope _pulse2Envelope = new();
    private LengthCounter _pulse2LengthCounter = new();
    private Sweeper _pulse2Sweep = new();

    private bool _noiseEnable;
    private bool _noiseHalt;
    private double _noiseSample;
    private double _noiseOutput;
    private Sequencer _noiseSequencer = new()
    {
        Sequence = 0xDBDB
    };
    private Envelope _noiseEnvelope = new();
    private LengthCounter _noiseLengthCounter = new();

    private static readonly byte[] LengthTable = 
    {
        10, 254, 20, 2, 40, 4, 80, 6,
        160, 8, 60, 10, 14, 12, 26, 14,
        12, 16, 24, 18, 48, 20, 96, 22,
        192, 24, 72, 26, 16, 28, 32, 30
    };

    public void CpuWrite(ushort address, byte data)
    {
        switch (address)
        {
            // Pulse Channel 1
            case 0x4000:
                switch ((data & 0xC0) >>> 6)
                {
                    case 0x00:
                        _pulse1Sequencer.NewSequence = 0b00000001;
                        _pulse1Oscillator.DutyCycle = 0.125;
                        break;

                    case 0x01:
                        _pulse1Sequencer.NewSequence = 0b00000011;
                        _pulse1Oscillator.DutyCycle = 0.25;
                        break;

                    case 0x02:
                        _pulse1Sequencer.NewSequence = 0b00001111;
                        _pulse1Oscillator.DutyCycle = 0.5;
                        break;

                    case 0x03:
                        _pulse1Sequencer.NewSequence = 0b11111100;
                        _pulse1Oscillator.DutyCycle = 0.75;
                        break;
                }
                _pulse1Sequencer.Sequence = _pulse1Sequencer.NewSequence;
                _pulse1Halt = (data & 0x20) != 0;
                _pulse1Envelope.Volume = (ushort)(data & 0x0F);
                _pulse1Envelope.Disable = (data & 0x10) != 0;
                break;

            case 0x4001:
                _pulse1Sweep.Enabled = (data & 0x80) != 0;
                _pulse1Sweep.Period = (byte)((data & 0x70) >>> 4);
                _pulse1Sweep.Down = (data & 0x08) != 0;
                _pulse1Sweep.Shift = (byte)(data & 0x07);
                _pulse1Sweep.Reload = true;
                break;

            case 0x4002:
                _pulse1Sequencer.Reload = (ushort)((_pulse1Sequencer.Reload & 0xFF00) | data);
                break;

            case 0x4003:
                _pulse1Sequencer.Reload = (ushort)((_pulse1Sequencer.Reload & 0x00FF) | ((data & 0x07) << 8));
                _pulse1Sequencer.Timer = _pulse1Sequencer.Reload;
                _pulse1Sequencer.Sequence = _pulse1Sequencer.NewSequence;
                _pulse1LengthCounter.Counter = LengthTable[(data & 0xF8) >>> 3];
                _pulse1Envelope.Start = true;
                break;

            // Pulse Channel 2
            case 0x4004:
                switch ((data & 0xC0) >>> 6)
                {
                    case 0x00:
                        _pulse2Sequencer.NewSequence = 0b00000001;
                        _pulse2Oscillator.DutyCycle = 0.125;
                        break;

                    case 0x01:
                        _pulse2Sequencer.NewSequence = 0b00000011;
                        _pulse2Oscillator.DutyCycle = 0.25;
                        break;

                    case 0x02:
                        _pulse2Sequencer.NewSequence = 0b00001111;
                        _pulse2Oscillator.DutyCycle = 0.5;
                        break;

                    case 0x03:
                        _pulse2Sequencer.NewSequence = 0b11111100;
                        _pulse2Oscillator.DutyCycle = 0.75;
                        break;
                }
                _pulse2Sequencer.Sequence = _pulse2Sequencer.NewSequence;
                _pulse2Halt = (data & 0x20) != 0;
                _pulse2Envelope.Volume = (ushort)(data & 0x0F);
                _pulse2Envelope.Disable = (data & 0x10) != 0;
                break;

            case 0x4005:
                _pulse2Sweep.Enabled = (data & 0x80) != 0;
                _pulse2Sweep.Period = (byte)((data & 0x70) >>> 4);
                _pulse2Sweep.Down = (data & 0x08) != 0;
                _pulse2Sweep.Shift = (byte)(data & 0x07);
                _pulse2Sweep.Reload = true;
                break;

            case 0x4006:
                _pulse2Sequencer.Reload = (ushort)((_pulse2Sequencer.Reload & 0xFF00) | data);
                break;

            case 0x4007:
                _pulse2Sequencer.Reload = (ushort)((_pulse2Sequencer.Reload & 0x00FF) | ((data & 0x07) << 8));
                _pulse2Sequencer.Timer = _pulse2Sequencer.Reload;
                _pulse2Sequencer.Sequence = _pulse2Sequencer.NewSequence;
                _pulse2LengthCounter.Counter = LengthTable[(data & 0xF8) >>> 3];
                _pulse2Envelope.Start = true;
                break;

            // Triangle Channel
            case 0x4008:
                break;

            case 0x4009:
                break;

            case 0x400A:
                break;

            case 0x400B:
                break;

            // Noise Channel
            case 0x400C:
                _noiseEnvelope.Volume = (ushort)(data & 0x0F);
                _noiseEnvelope.Disable = (data & 0x10) != 0;
                _noiseHalt = (data & 0x20) != 0;
                break;

            case 0x400D:
                break;

            case 0x400E:
                switch (data & 0x0F)
                {
                    case 0x00: _noiseSequencer.Reload = 0; break;
                    case 0x01: _noiseSequencer.Reload = 4; break;
                    case 0x02: _noiseSequencer.Reload = 8; break;
                    case 0x03: _noiseSequencer.Reload = 16; break;
                    case 0x04: _noiseSequencer.Reload = 32; break;
                    case 0x05: _noiseSequencer.Reload = 64; break;
                    case 0x06: _noiseSequencer.Reload = 96; break;
                    case 0x07: _noiseSequencer.Reload = 128; break;
                    case 0x08: _noiseSequencer.Reload = 160; break;
                    case 0x09: _noiseSequencer.Reload = 202; break;
                    case 0x0A: _noiseSequencer.Reload = 254; break;
                    case 0x0B: _noiseSequencer.Reload = 380; break;
                    case 0x0C: _noiseSequencer.Reload = 508; break;
                    case 0x0D: _noiseSequencer.Reload = 1016; break;
                    case 0x0E: _noiseSequencer.Reload = 2034; break;
                    case 0x0F: _noiseSequencer.Reload = 4068; break;
                }
                break;

            case 0x400F:
                _pulse1Envelope.Start = true;
                _pulse2Envelope.Start = true;
                _noiseEnvelope.Start = true;
                _noiseLengthCounter.Counter = LengthTable[(data & 0xF8) >>> 3];
                break;

            // DMC Channel
            case 0x4010:
                break;

            case 0x4011:
                break;

            case 0x4012:
                break;

            case 0x4013:
                break;

            // Control
            case 0x4015:
                _pulse1Enable = (data & 0x01) != 0;
                _pulse2Enable = (data & 0x02) != 0;
                _noiseEnable = (data & 0x04) != 0;
                break;

            // Frame Counter
            case 0x4017:
                break;
        }
    }

    public byte CpuRead(ushort address)
    {
        return 0;
    }

    public void Clock()
    {
        var quarterFrameClock = false;
        var halfFrameClock = false;

        _globalTime += (0.3333333333 / 1789773);

        if (_clockCounter % 6 == 0)
        {
            _frameClockCounter++;

            if (_frameClockCounter == 3729)
            {
                quarterFrameClock = true;
            }

            if (_frameClockCounter == 7457)
            {
                quarterFrameClock = true;
                halfFrameClock = true;
            }

            if (_frameClockCounter == 11186)
            {
                quarterFrameClock = true;
            }

            if (_frameClockCounter == 14916)
            {
                quarterFrameClock = true;
                halfFrameClock = true;
                _frameClockCounter = 0;
            }

            if (quarterFrameClock)
            {
                // volume envelope
                _pulse1Envelope.Clock(_pulse1Halt);
                _pulse2Envelope.Clock(_pulse2Halt);
                _noiseEnvelope.Clock(_noiseHalt);
            }

            if (halfFrameClock)
            {
                // frequency sweep and note length
                _pulse1LengthCounter.Clock(_pulse1Enable, _pulse1Halt);
                _pulse2LengthCounter.Clock(_pulse2Enable, _pulse2Halt);
                _noiseLengthCounter.Clock(_noiseEnable, _noiseHalt);
                _pulse1Sweep.Clock(ref _pulse1Sequencer.Reload, 0);
                _pulse2Sweep.Clock(ref _pulse2Sequencer.Reload, 1);
            }

            //	if (bUseRawMode)
            {
                // Update Pulse1 Channel ================================
                _pulse1Sequencer.Clock(_pulse1Enable, s => ((s & 0x0001) << 7) | ((s & 0x00FE) >>> 1));
                //	_pulse1Sample = (double)_pulse1Sequencer.output;
            }
            //else
            {
                _pulse1Oscillator.Frequency = 1789773.0 / (16.0 * (double)(_pulse1Sequencer.Reload + 1));
                _pulse1Oscillator.Amplitude = (double)(_pulse1Envelope.Output - 1) / 16.0;
                _pulse1Sample = _pulse1Oscillator.Sample(_globalTime);

                if (_pulse1LengthCounter.Counter > 0 && _pulse1Sequencer.Timer >= 8 && !_pulse1Sweep.Mute && _pulse1Envelope.Output > 2)
                    _pulse1Output += (_pulse1Sample - _pulse1Output) * 0.5;
                else
                    _pulse1Output = 0;
            }

            //if (bUseRawMode)
            {
                // Update Pulse1 Channel ================================
                _pulse2Sequencer.Clock(_pulse2Enable, s => ((s & 0x0001) << 7) | ((s & 0x00FE) >> 1));
                //	_pulse2Sample = (double)_pulse2Sequencer.output;

            }
            //	else
            {
                _pulse2Oscillator.Frequency = 1789773.0 / (16.0 * (double)(_pulse2Sequencer.Reload + 1));
                _pulse2Oscillator.Amplitude = (double)(_pulse2Envelope.Output - 1) / 16.0;
                _pulse2Sample = _pulse2Oscillator.Sample(_globalTime);

                if (_pulse2LengthCounter.Counter > 0 && _pulse2Sequencer.Timer >= 8 && !_pulse2Sweep.Mute && _pulse2Envelope.Output > 2)
                    _pulse2Output += (_pulse2Sample - _pulse2Output) * 0.5;
                else
                    _pulse2Output = 0;
            }


            _noiseSequencer.Clock(_noiseEnable, s => (((s & 0x0001) ^ ((s & 0x0002) >> 1)) << 14) | ((s & 0x7FFF) >>> 1));

            if (_noiseLengthCounter.Counter > 0 && _noiseSequencer.Timer >= 8)
            {
                _noiseOutput = (double)_noiseSequencer.Output * ((double)(_noiseEnvelope.Output - 1) / 16.0);
            }

            if (!_pulse1Enable) 
            {
                _pulse1Output = 0; 
            }

            if (!_pulse2Enable)
            {
                _pulse2Output = 0;
            }

            if (!_noiseEnable)
            {
                _noiseOutput = 0;
            }
        }

        // Frequency sweepers change at high frequency
        _pulse1Sweep.Track(_pulse1Sequencer.Reload);
        _pulse2Sweep.Track(_pulse2Sequencer.Reload);

        _clockCounter++;
    }

    public void Reset()
    {

    }

    public double GetOutputSample()
    {
        if (_useRawMode)
        {
            return (_pulse1Sample - 0.5) * 0.5
                   + (_pulse2Sample - 0.5) * 0.5;
        }
        else
        {
            return ((1.0 * _pulse1Output) - 0.8) * 0.1 +
                   ((1.0 * _pulse2Output) - 0.8) * 0.1 +
                   ((2.0 * (_noiseOutput - 0.5))) * 0.1;
        }
    }

    private struct Sequencer
    {
        public uint Sequence { get; set; }
        public uint NewSequence { get; set; }
        public ushort Timer { get; set; }
        public ushort Reload;
        
        public byte Output { get; private set; }

        public byte Clock(bool enable, Func<uint, uint> manipulator)
        {
            if (enable)
            {
                Timer--;
                if (Timer == 0xFFFF)
                {
                    Timer = (ushort)(Reload + 1);
                    Sequence = manipulator(Sequence);
                    Output = (byte)(Sequence & 1);
                }
            }

            return Output;
        }
    }

    private struct LengthCounter
    {
        public byte Counter { get; set; }
        
        public byte Clock(bool enable, bool halt)
        {
            if (!enable)
            {
                Counter = 0;
            }
            else if (Counter > 0 && !halt)
            {
                Counter--;
            }

            return Counter;
        }
    }

    private struct Envelope
    {
        public Envelope()
        {
            
        }

        public bool Start { get; set; } = false;
        public bool Disable { get; set; } = false;
        public ushort DividerCount { get; set; } = 0;
        public ushort Volume { get; set; } = 0;
        public ushort Output { get; set; } = 0;
        public ushort DecayCount { get; set; } = 0;

        public void Clock(bool loop)
        {
            if (!Start)
            {
                if (DividerCount == 0)
                {
                    DividerCount = Volume;
                    if (DecayCount == 0)
                    {
                        if (loop)
                        {
                            DecayCount = 15;
                        }
                    }
                    else
                    {
                        DecayCount--;
                    }
                }
                else
                {
                    DividerCount--;
                }
            }
            else
            {
                Start = false;
                DecayCount = 15;
                DividerCount = Volume;
            }

            Output = Disable ? Volume : DecayCount;
        }
    }

    private struct OscillatorPulse
    {
        public OscillatorPulse()
        {
            
        }

        public double Frequency { get; set; } = 0;
        public double DutyCycle { get; set; } = 0;
        public double Amplitude { get; set; } = 1;
        public double Pi { get; set; } = 3.14159;
        public double Harmonics { get; set; } = 20;

        public double Sample(double t)
        {
            var a = 0.0;
            var b = 0.0;
            var p = DutyCycle * 2.0 * Pi;

            for (double n = 1; n < Harmonics; n++)
            {
                var c = n * Frequency * 2.0 * Pi * t;
                a += -ApproxSin(c) / n;
                b += -ApproxSin(c - p * n) / n;
            }

            return (a - b) * (2.0 * Amplitude / Pi);
        }

        private static double ApproxSin(double x)
        {
            var j = x * 0.15915f;
            j -= (int)j;
            return 20.785 * j * (j - 0.5) * (j - 1.0);
        }
    }

    private struct Sweeper
    {
        public Sweeper()
        {
            
        }

        public bool Enabled { get; set; } = false;
        public bool Down { get; set; } = false;
        public bool Reload { get; set; } = false;
        public byte Shift { get; set; } = 0x00;
        public byte Timer { get; set; } = 0x00;
        public byte Period { get; set; } = 0x00;
        public ushort Change { get; set; } = 0;
        public bool Mute { get; set; } = false;

        public void Track(ushort target) 
        {
            if (Enabled)
            {
                Change = (ushort)(target >>> Shift);
                Mute = target is < 8 or > 0x7FF;
            }
        }

        public bool Clock(ref ushort target, byte channel)
        {
            var changed = false;
            if (Timer == 0 && Enabled && Shift > 0 && !Mute)
            {
                if (target >= 8 && Change < 0x7FF)
                {
                    if (Down)
                    {
                        target -= (ushort)(Change - channel);
                    }
                    else
                    {
                        target += Change;
                    }
                    changed = true;
                }
            }

            if (Reload || Timer == 0)
            {
                Timer = Period;
                Reload = false;
            }
            else
            {
                Timer--;
            }

            Mute = target is < 8 or > 0x7FF;

            return changed;
        }
    }
}