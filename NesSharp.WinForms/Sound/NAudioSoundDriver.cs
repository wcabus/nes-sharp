using NAudio.Wave;
using NesSharp.Core.Sound;

namespace NesSharp.WinForms.Sound;

public class NAudioSoundDriver : ISoundDriver
{
    private uint _sampleRate;
    private uint _channels;
    private uint _blocks;
    private uint _blockSamples;
    private uint _blockFree;
    private uint _blockPos;

    private byte[] _blockMemory = Array.Empty<byte>();

    private float _globalTime;

    private SoundOut? _soundOut;

    private WaveOut? _soundOutput;
    private QueuedBufferProvider? _waveProvider;
    private readonly CancellationTokenSource _soundCancellationTokenSource = new();
    private Thread? _soundThread;
    
    private readonly object _lock = new();

    public void InitializeAudio(uint sampleRate, uint channels, uint blocks, uint blockSamples)
    {
        _sampleRate = sampleRate;
        _channels = channels;
        _blocks = blocks;
        _blockSamples = blockSamples;
        _blockFree = _blocks;

        var callback = WaveCallbackInfo.FunctionCallback();
        _soundOutput = new WaveOut(callback);

        _waveProvider = new QueuedBufferProvider(new WaveFormat((int)_sampleRate, sizeof(short) * 8, (int)_channels), (int)(_blockSamples * sizeof(short)), (int)_blocks);
        _waveProvider.BufferExhausted += WhenBufferExhausted;

        _soundOutput.Init(_waveProvider);
        
        _blockMemory = new byte[_blocks * _blockSamples * sizeof(short)];

        _soundThread = new Thread(SoundThread);
        _soundThread.Start();

        lock (_lock)
        {
            Monitor.Pulse(_lock);
        }
    }
    
    public void SetSoundOutMethod(SoundOut soundOut)
    {
        _soundOut = soundOut;
    }

    public void DestroyAudio()
    {
        _soundCancellationTokenSource.Cancel();

        _soundOutput?.Dispose();
    }

    private void SoundThread()
    {
        var cancellationToken = _soundCancellationTokenSource.Token;
        
        _globalTime = 0.0f;
        var timeStep = 1.0f / _sampleRate;
        var maxSample = (float)short.MaxValue;

        while (!cancellationToken.IsCancellationRequested)
        {
            // Wait for block to become available
            if (_blockFree == 0)
            {
                lock (_lock)
                {
                    while (_blockFree == 0 && !cancellationToken.IsCancellationRequested)
                    {
                          Monitor.Wait(_lock, 10);
                    }
                }
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            _blockFree--;

            var currentBlock = _blockPos * _blockSamples;

            for (uint i = 0; i < _blockSamples; i += _channels) 
            {
                for (uint c = 0; c < _channels; c++)
                {
                    var newSample = (short)(Clip(GetMixerOutput(c, _globalTime + timeStep * i, timeStep, cancellationToken), 1.0f) * maxSample);
                    _blockMemory[currentBlock + ((i + c) * sizeof(short)) + 0] = (byte)(newSample & 0xFF);
                    _blockMemory[currentBlock + ((i + c) * sizeof(short)) + 1] = (byte)(newSample >>> 8);
                }
            }

            _globalTime += (timeStep * _blockSamples);

            // Send the block to the sound card
            _waveProvider!.AddSamples(_blockMemory, (int)currentBlock, (int)_blockSamples * sizeof(short));
            
            _blockPos++;
            _blockPos %= _blocks;
            _soundOutput!.Play();
        }
    }

    private void WhenBufferExhausted(object? sender, EventArgs e)
    {
        _blockFree++;
        lock (_lock)
        {
            Monitor.Pulse(_lock);
        }
    }

    private float GetMixerOutput(uint channel, float globalTime, float timeStep, CancellationToken cancellationToken = default)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            return _soundOut?.Invoke(channel, globalTime, timeStep) ?? 0.0f;
        }

        return 0.0f;
    }

    private static float Clip(float sample, float max)
    {
        return sample >= 0.0 ? Math.Min(sample, max) : Math.Max(sample, -max);
    }

    public void Dispose()
    {
        DestroyAudio();
        
        _soundCancellationTokenSource.Dispose();
    }
}