using NAudio.Wave;

namespace NesSharp.WinForms.Sound;

public class QueuedBufferProvider : IWaveProvider
{
    private readonly Queue<byte[]> _queue = new();
    private byte[]? _currentReadBuffer = null;
    private int _currentReadPos = 0;

    private int _queuePos = 0;

    public QueuedBufferProvider(WaveFormat waveFormat, int bufferLength, int bufferCount)
    {
        WaveFormat = waveFormat;
        BufferLength = bufferLength;
        BufferCount = bufferCount;

        for (var i = 0; i < bufferCount; i++)
        {
            _queue.Enqueue(new byte[bufferLength]);
        }
    }

    public WaveFormat WaveFormat { get; }

    /// <summary>Buffer length in bytes</summary>
    public int BufferLength { get; }
    
    /// <summary>Number of buffers</summary>
    public int BufferCount { get; }

    public void AddSamples(byte[] buffer, int offset, int count)
    {
        if (count > BufferLength)
        {
            throw new ArgumentException(@"count must be less than or equal to BufferLength", nameof(count));
        }

        if (_queuePos >= BufferCount)
        {
            return; // ignore samples if queue is full
        }

        Array.Copy(buffer, offset, _queue.ElementAt(_queuePos), 0, count);
        _queuePos++;
    }

    public int Read(byte[] buffer, int offset, int count)
    {
        if (_queuePos == 0)
        {
            return 0;
        }

        var bytesRead = 0;
        if (_currentReadBuffer != null)
        {
            var bytesToCopy = Math.Min(count, _currentReadBuffer.Length - _currentReadPos);
            Array.Copy(_currentReadBuffer, _currentReadPos, buffer, offset, bytesToCopy);

            bytesRead += bytesToCopy;
            _currentReadPos += bytesToCopy;
            
            ClearCurrentReadBufferWhenExhausted();
        }
        
        while (_currentReadBuffer == null && _queue.Count != 0 && bytesRead < count && _queuePos > 0)
        {
            _currentReadBuffer = _queue.Dequeue();
            _queuePos--;

            _currentReadPos = 0;

            var bytesToCopy = Math.Min(count - bytesRead, _currentReadBuffer.Length);
            Array.Copy(_currentReadBuffer, _currentReadPos, buffer, offset + bytesRead, bytesToCopy);
            
            bytesRead += bytesToCopy;
            _currentReadPos += bytesToCopy;

            ClearCurrentReadBufferWhenExhausted();
        }

        void ClearCurrentReadBufferWhenExhausted()
        {
            if (_currentReadBuffer != null && _currentReadPos >= _currentReadBuffer.Length)
            {
                Array.Clear(_currentReadBuffer);
                _queue.Enqueue(_currentReadBuffer);

                _currentReadBuffer = null;
                _currentReadPos = 0;

                BufferExhausted?.Invoke(this, EventArgs.Empty);
            }
        }

        return bytesRead;
    }

    public event EventHandler? BufferExhausted;
}