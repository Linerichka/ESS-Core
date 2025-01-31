namespace Lineri.ESS.Core.Interfaces
{

    public interface IAudioSource
    { 
        public IAudioClip Clip { get; set; }
        
        public float Volume { get; set; }
        public float Pitch { get; set; }
        public bool Loop {get;set; }
        public bool Mute { get; set; }
        
        public bool IsPlaying { get; set; }
        
        
        public void Play();
        public void Stop();
        public void Pause();
        public void UnPause();
    }
}