using System;


namespace Lineri.ESS.Core.Utils
{

    public static class Application
    {
        public static bool IsFocused = true;
        public static bool IsPlaying = true;

        public static Action SceneLoaded;

        public static float DeltaTime => 0.016f;
    }

}