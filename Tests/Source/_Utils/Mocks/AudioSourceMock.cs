using Lineri.ESS.Core.Base;
using Lineri.ESS.Core.Interfaces;
using Moq;


namespace Tests.Source._Utils.Mocks;

public class AudioSourceMock : Mock<IAudioSource>, IAudioSource
{
    public IAudioClip Clip
    {
        get => Object.Clip;
        set => Object.Clip = value;
    }

    public float Volume
    {
        get => Object.Volume;
        set => Object.Volume = value;
    }

    public float Pitch
    {
        get => Object.Pitch;
        set => Object.Pitch = value;
    }

    public bool Loop
    {
        get => Object.Loop;
        set => Object.Loop = value;
    }

    public bool Mute
    {
        get => Object.Mute;
        set => Object.Mute = value;
    }

    public bool IsPlaying
    {
        get => Object.IsPlaying;
        set => Object.IsPlaying = value;
    }

    public void Play()
    {
        Object.Play();
    }

    public void Stop()
    {
        Object.Stop();
    }

    public void Pause()
    {
        Object.Pause();
    }

    public void UnPause()
    {
        Object.UnPause();
    }
}