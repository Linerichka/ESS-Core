using Lineri.ESS.Core;
using Lineri.ESS.Core.Interfaces;
using Lineri.ESS.Core.Utils;
using Moq;


namespace Tests.Source._Utils.Mocks;

public class AudioMock : Mock<IAudio>, IAudio
{
    public int AudioID
    {
        get { return Object.AudioID; }
        set => this.SetupGet(x => x.AudioID).Returns(value);
    }

    public AudioType Type
    {
        get { return Object.Type; }
        set => this.SetupGet(x => x.Type).Returns(value);
    }

    public bool IsPlaying
    {
        get { return Object.IsPlaying; }
        set => this.SetupGet(x => x.IsPlaying).Returns(value);
    }

    public bool Paused
    {
        get { return Object.Paused; }
        set => this.SetupGet(x => x.Paused).Returns(value);
    }

    public bool Stopping
    {
        get { return Object.Stopping; }
        set => this.SetupGet(x => x.Stopping).Returns(value);
    }

    public bool Activated
    {
        get { return Object.Activated; }
        set => this.SetupGet(x => x.Activated).Returns(value);
    }

    public bool PlayedStart
    {
        get { return Object.PlayedStart; }
        set => this.SetupGet(x => x.PlayedStart).Returns(value);
    }

    public bool Deleted
    {
        get { return Object.Deleted; }
        set => this.SetupGet(x => x.Deleted).Returns(value);
    }

    public float Volume
    {
        get { return Object.Volume; }
        set => this.SetupGet(x => x.Volume).Returns(value);
    }

    public IAudioSource AudioSource
    {
        get => Object.AudioSource;
        set => this.SetupGet(x => x.AudioSource).Returns(value);
    }

    public IAudioClip Clip
    {
        get => Object.Clip;
        set => this.SetupGet(x => x.Clip).Returns(value);
    }


    public bool Loop
    {
        get => Object.Loop;
        set => this.SetupGet(x => x.Loop).Returns(value);
    }
    public bool Mute {
        get => Object.Loop;
        set => this.SetupGet(x => x.Mute).Returns(value);
    }
    public float Pitch {
        get => Object.Pitch;
        set => this.SetupGet(x => x.Pitch).Returns(value);
    }
    public bool Persist {
        get => Object.Persist;
        set => this.SetupGet(x => x.Persist).Returns(value);
    }
    public float FadeInSeconds {
        get => Object.FadeInSeconds;
        set => this.SetupGet(x => x.FadeInSeconds).Returns(value);
    }
    public float FadeOutSeconds {
        get => Object.FadeOutSeconds;
        set => this.SetupGet(x => x.FadeOutSeconds).Returns(value);
    }

    public float TimeToEnd = 2f;

    public void Init(int audioID, AudioType audioType, IAudioClip clip, bool loop, bool persist, float volume, float fadeInValue,
        float fadeOutValue, IAudioSource audioSource)
    {
        Object.Init(audioID, audioType, clip, loop, persist, volume, fadeInValue, fadeOutValue, audioSource);
        
        AudioID = audioID;
        this.AudioSource = audioSource;
        this.Type = audioType;
        this.Clip = clip;
        this.Loop = loop;
        this.Persist = persist;
        this.FadeInSeconds = fadeInValue;
        this.FadeOutSeconds = fadeOutValue;
        this.Volume = volume;
    }

    public void Update()
    {
        Object.Update();

        TimeToEnd -= Application.DeltaTime;
        if (TimeToEnd <= 0f)
        {
            Stop();
        }
        
        if (Stopping)
        {
            Stopping = false;
            IsPlaying = false;
            Paused = false;
        }
        else
        {
            IsPlaying = true;
        }
    }

    public void Play()
    {
        Object.Play();
        PlayedStart = true;
        IsPlaying = true;
    }

    public void Play(float volume)
    {
        Play();
    }

    public void Stop()
    {
        Object.Stop();
        
        Stopping = true;
    }

    public void StopInstantly()
    {
        Stop();
    }

    public void Pause()
    {
        Object.Pause();
        Paused = true;
    }

    public void UnPause()
    {
        Object.UnPause();
        Paused = false;
    }

    public void SetVolume(float volume)
    {
        Object.SetVolume(volume);
        
        Volume = volume;
    }

    public void SetVolume(float volume, float fadeSeconds)
    {
        SetVolume(volume);
    }

    public void SetVolume(float volume, float fadeSeconds, float startVolume)
    {
        SetVolume(volume);
    }

    public void Delete()
    {
        Object.Delete();
        
        if (IsPlaying)
        {
            AudioSource.Stop();
        }
            
        Stopping = false;
        IsPlaying = false;
        Paused = false;
        AudioSource = null;
        Deleted = true;
        Activated = false;
        PlayedStart = false;
    }
}