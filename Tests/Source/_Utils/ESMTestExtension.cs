using Lineri.ESS.Core;
using Lineri.ESS.Core.Interfaces;
using Moq;
using Tests.Source._Utils.Mocks;


namespace Tests.Source._Utils;

public class ESMTestExtension
{
    public static EazySoundManager<AudioMock, AudioSourceMock> CreateManager(out AudioMock a, out Mock<IAudioClip> c)
    {
        var m = CreateManager<AudioMock, AudioSourceMock>(out var a2, out var c2);
        a = a2;
        c = c2;
        return m;
    }
    public static EazySoundManager<TAudio, TSource> CreateManager<TAudio, TSource>(out TAudio a, out Mock<IAudioClip> c) 
        where TAudio : class, IAudio, new() 
        where TSource : class, IAudioSource, new()
    {
        EazySoundManager<TAudio, TSource>.Instance?.Dispose();
        var m = new EazySoundManager<TAudio, TSource>();
        c = new Mock<IAudioClip>();
        a = m.PlayAudio(AudioType.Music, c.Object, 1f, false, false, 1f, 1f);
        return m;
    }
}