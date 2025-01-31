using System;
using Lineri.ESS.Core.Interfaces;


namespace Lineri.ESS.Core.Base
{
    public class AudioSource : IAudioSource
    {
        public IAudioClip Clip { get; set; }
        
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public bool Loop { get; set; }
        public bool Mute { get; set; }
        
        public bool IsPlaying { get; set; }
        
        public void Play()
        {
            IsPlaying = true;
        }

        public void Stop()
        {
            IsPlaying = false;
        }

        public void Pause()
        {
        }

        public void UnPause()
        {
        }
    }
}