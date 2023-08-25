namespace NesSharp.Core.Sound;

public interface ISoundDriver : IDisposable
{
    void InitializeAudio(uint sampleRate, uint channels, uint blocks, uint blockSamples);
    void SetSoundOutMethod(SoundOut soundOut);
    void DestroyAudio();
}

public delegate float SoundOut(uint channel, float globalTime, float timeStep);