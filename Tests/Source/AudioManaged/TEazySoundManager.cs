using Lineri.ESS.Core;
using Lineri.ESS.Core.Interfaces;
using Lineri.ESS.Core.Utils;
using Moq;
using Tests.Source._Utils;
using Tests.Source._Utils.Mocks;


namespace Tests.Source.AudioManaged;

public class TEazySoundManager
{
    
    [Fact]
    public void FadeOutforStopped()
    {
        var m = ESMTestExtension.CreateManager(out var a, out var c1);

        a.FadeOutSeconds = 0f;
        m.StopAudio(AudioType.Music, 1f);
        Assert.True(1f == a.FadeOutSeconds);

        a.Paused = false;
        a.IsPlaying = true;
        a.TimeToEnd = 0f;
        a.VerifyDefaultArgs(a.Delete, Times.Never());
        Assert.False(a.Deleted);
        m.Update();
        a.VerifyDefaultArgs(a.Update, Times.Exactly(1));
        a.VerifyDefaultArgs(a.Delete, Times.Once());
    }

    [Fact]
    public void DeleteAfterEndPlaying()
    {
        var m = ESMTestExtension.CreateManager(out var a, out var c1);
        
        a.Paused = false;
        a.IsPlaying = true;
        a.TimeToEnd = 0f;
        a.VerifyDefaultArgs(a.Delete, Times.Never());
        Assert.False(a.Deleted);
        m.Update();
        a.VerifyDefaultArgs(a.Update, Times.Once());
        a.VerifyDefaultArgs(a.Delete, Times.Once());
    }


    [Fact]
    public void SingltonExceptionSecondInstance()
    {
        var m = ESMTestExtension.CreateManager(out var a, out var c1);
        Assert.ThrowsAny<Exception>(() => new EazySoundManager<AudioMock, AudioSourceMock>());
    }
    [Fact]
    public void ManageMethodsInvokeTest()
    {
        var m = ESMTestExtension.CreateManager(out var a, out var c1);
        var c2 = new Mock<IAudioClip>();
        
        Assert.Equal(m.GetAudio(AudioType.Music, a.AudioID), a);
        Assert.Equal(m.GetMusicAudio(a.AudioID), a);
        Assert.Contains(a, m.GetAudios(AudioType.Music, a.Clip));
        Assert.Null(m.GetMusicAudio(a.AudioID + 1));
        Assert.Null(m.GetMusicAudio(a.AudioID - 1));
        Assert.Null(m.GetSoundAudio(a.AudioID));
        Assert.Null(m.GetUISoundAudio(a.AudioID));
        Assert.Empty(m.GetMusicAudios(c2.Object));
        Assert.Empty(m.GetSoundAudios(c2.Object));
        Assert.Empty(m.GetUISoundAudios(c2.Object));
        Assert.Empty(m.GetSoundAudios(c1.Object));
        Assert.Empty(m.GetUISoundAudios(c1.Object));
        
        a.VerifyDefaultArgs(nameof(a.Init), Times.Once());
        a.VerifyDefaultArgs((a.Play, Times.Once()));
        a.VerifyDefaultArgs(a.Stop, a.StopInstantly, a.Update, a.Pause, a.UnPause, a.Delete);
    }

    [Fact]
    public void PauseStopMethods()
    {
        var m = ESMTestExtension.CreateManager(out var a, out var c1);

        a.VerifyDefaultArgs(a.Pause, a.Stop, a.UnPause, a.Delete);
        m.PauseAllAudio();
        a.VerifyDefaultArgs(a.Stop, a.UnPause, a.Delete);
        a.VerifyDefaultArgs(a.Pause, Times.Once());
        m.UnPauseAllAudio();
        a.VerifyDefaultArgs(a.Stop, a.Delete);
        a.VerifyDefaultArgs(a.UnPause, Times.Once());
        a.VerifyDefaultArgs(a.Delete);
        m.StopAll();
        a.VerifyDefaultArgs(a.Stop, Times.Once());
        
        for (int i = 0; i < 2; i++)
        {
            m.PauseAllAudio();
            m.UnPauseAllAudio();
            m.StopAll();
        }
        
        a.VerifyDefaultArgs(a.Delete);
        Assert.False(a.Deleted);
    }

    [Fact]
    public void LostFocus()
    {
        var m = ESMTestExtension.CreateManager(out var a, out var c1);

        Assert.True(m.CanPlay);
        m.Update();
        Assert.True(m.CanPlay);
        a.VerifyDefaultArgs(a.Pause);
        a.VerifyDefaultArgs(a.UnPause);
        Application.IsFocused = false;
        m.CanPlayInBackground = false;
        a.VerifyDefaultArgs(a.Pause);
        a.VerifyDefaultArgs(a.UnPause);
        m.Update();
        a.VerifyDefaultArgs(a.Pause, Times.Once());
        a.VerifyDefaultArgs(a.UnPause);
        Application.IsFocused = true;
        m.Update();
        a.VerifyDefaultArgs(a.Pause, Times.Once());
        a.VerifyDefaultArgs(a.UnPause, Times.Once());
    }

    [Fact]
    public void Volumes()
    {
        var m = ESMTestExtension.CreateManager(out var a, out var c1);

        float[] vs = new float[] { 2f, float.MaxValue, float.PositiveInfinity, float.NaN, float.NegativeZero, -1f, float.NegativeInfinity, float.NaN, 0.5f, float.NaN };
        float[] es = new float[] { 1f, 1f, 1f, 1f, 0f, 0f, 0f, 0f, 0.5f, 0.5f };

        for (var i = 0; i < vs.Length; i++)
        {
            m.GlobalVolume = vs[i];
            Assert.True(m.GlobalVolume == es[i], $"Expected '{es[i]}', got '{m.GlobalVolume}', from '{vs[i]}'");
            m.GlobalMusicVolume = vs[i];
            Assert.True(m.GlobalMusicVolume == es[i], $"Expected '{es[i]}', got '{m.GlobalMusicVolume}', from '{vs[i]}'");
            m.GlobalSoundVolume = vs[i];
            Assert.True(m.GlobalSoundVolume == es[i], $"Expected '{es[i]}', got '{m.GlobalSoundVolume}', from '{vs[i]}'");
            m.GlobalUISoundVolume = vs[i];
            Assert.True(m.GlobalUISoundVolume == es[i], $"Expected '{es[i]}', got '{m.GlobalUISoundVolume}', from '{vs[i]}'");
        }
    }

    [Fact]
    public void RemoveNonPersistAudioAudioCache()
    {
        var m = ESMTestExtension.CreateManager(out var a, out var c1);

        a.Persist = false;
        m.Update();
        Assert.True(a.IsPlaying);
        a.VerifyDefaultArgs(a.Stop);
        Assert.True(m.GetPrivateFieldByType<Queue<IAudioSource>>().Count == 0);
        a.TimeToEnd = 0f;
        a.VerifyDefaultArgs(a.Delete);
        m.PlayMusic(c1.Object);
        m.Update();
        a.VerifyDefaultArgs(a.Delete, Times.Once());
        Assert.True(m.GetPrivateFieldByType<Queue<IAudioSource>>().Count == 1);
        
        m.InvokePrivateMethod<object>("OnSceneLoaded");
        a.VerifyDefaultArgs(a.Delete, Times.Once());
        
        Assert.True(m.GetPrivateFieldByType<Queue<IAudioSource>>().Count == 0);
        a.VerifyDefaultArgs(a.Play, Times.Once());
        m.PlayAudio(AudioType.Sound, c1.Object);
        a.VerifyDefaultArgs(a.Play, Times.Exactly(2));
        Assert.True(a.IsPlaying);
    }
    
    [Fact]
    public void Overloads()
    {
        var m = ESMTestExtension.CreateManager(out var a, out var c1);
        
        Assert.Equal(AudioType.Music, m.PlayMusic(c1.Object).Type);
        Assert.Equal(AudioType.Sound, m.PlaySound(c1.Object).Type);
        Assert.Equal(AudioType.UISound, m.PlayUISound(c1.Object).Type);
        Assert.Equal(AudioType.Music, m.PrepareMusic(c1.Object).Type);
        Assert.Equal(AudioType.Sound, m.PrepareSound(c1.Object).Type);
        Assert.Equal(AudioType.UISound, m.PrepareUISound(c1.Object).Type);
    }
}