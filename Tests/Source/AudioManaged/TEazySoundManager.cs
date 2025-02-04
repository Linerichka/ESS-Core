using Lineri.ESS.Core;
using Tests.Source._Utils.Mocks;


namespace Tests.Source.AudioManaged;

public class TEazySoundManager
{


    public void CreateGetSet()
    {
        var m = new EazySoundManager<AudioMock, AudioSourceMock>();
    }
}