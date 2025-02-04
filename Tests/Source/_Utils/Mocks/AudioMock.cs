using Lineri.ESS.Core;
using Lineri.ESS.Core.Interfaces;
using Moq;


namespace Tests.Source._Utils.Mocks;

public class AudioMock : IAudio
{
    public int AudioID { get; }
    public AudioType Type { get; }
    public bool IsPlaying { get; }
    public bool Paused { get; }
    public bool Stopping { get; }
    public bool Activated { get; }
    public bool PlayedStart { get; }
    public bool Deleted { get; }
    public float Volume { get; }
    public IAudioSource AudioSource { get; }
    public IAudioClip Clip { get; }
    public bool Loop { get; set; }
    public bool Mute { get; set; }
    public float Pitch { get; set; }
    public bool Persist { get; set; }
    public float FadeInSeconds { get; set; }
    public float FadeOutSeconds { get; set; }

    public void Init(int audioID, AudioType audioType, IAudioClip clip, bool loop, bool persist, float volume, float fadeInValue,
        float fadeOutValue, IAudioSource audioSource)
    {
        throw new NotImplementedException();
    }

    public void Update()
    {
        throw new NotImplementedException();
    }

    public void Play()
    {
        throw new NotImplementedException();
    }

    public void Play(float volume)
    {
        throw new NotImplementedException();
    }

    public void Stop()
    {
        throw new NotImplementedException();
    }

    public void StopInstantly()
    {
        throw new NotImplementedException();
    }

    public void Pause()
    {
        throw new NotImplementedException();
    }

    public void UnPause()
    {
        throw new NotImplementedException();
    }

    public void SetVolume(float volume)
    {
        throw new NotImplementedException();
    }

    public void SetVolume(float volume, float fadeSeconds)
    {
        throw new NotImplementedException();
    }

    public void SetVolume(float volume, float fadeSeconds, float startVolume)
    {
        throw new NotImplementedException();
    }

    public void Delete()
    {
        throw new NotImplementedException();
    }
}