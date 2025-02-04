using Lineri.ESS.Core.Interfaces;
using Lineri.ESS.Core.Utils;

namespace Lineri.ESS.Core
{
    /// <summary>
    /// The audio object
    /// </summary>
    public class Audio : IAudio
    {
        public int AudioID { get; private set; }
        public AudioType Type { get; private set; }
        public bool IsPlaying { get; private set; }
        public bool Paused { get; private set; }
        public bool Stopping { get; private set; }
        public bool Activated { get; private set; }
        public bool PlayedStart { get; private set; }
        public bool Deleted { get; private set; }
        public float Volume
        {
            get => _volume;
            private set => _volume = MathUtil.Clamp01(value);
        }
        public IAudioSource AudioSource { get; private set; }
        public IAudioClip Clip
        {
            get => _clip;
            private set
            {
                _clip = value;
                AudioSource.Clip = _clip;
            }
        }
        
        public bool Loop
        {
            get => _loop;
            set
            {
                _loop = value;
                AudioSource.Loop = _loop;
            }
        }
        
        public bool Mute
        {
            get => _mute;
            set
            {
                _mute = value;
                AudioSource.Mute = _mute;
            }
        }

        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = MathUtil.Clamp(value, -3, 3);
                AudioSource.Pitch = _pitch;
            }
        }

        public bool Persist { get; set; }
        public float FadeInSeconds { get; set; }
        public float FadeOutSeconds { get; set; }


        private IAudioClip _clip;
        private float _volume;
        private bool _loop;
        private bool _mute;
        private float _pitch;

        private float _targetVolume;
        private float _initTargetVolume;
        private float _tempFadeSeconds = -1f;
        private float _fadeInterpolater = 0f;
        private float _onFadeStartVolume;
        
        
        public void Init( int audioID, AudioType audioType, IAudioClip clip, bool loop, bool persist, float volume, float fadeInValue,
            float fadeOutValue, IAudioSource audioSource)
        {
            // Set unique audio ID
            AudioID = audioID;

            // Initialize values
            // Use private fields for setting to prevent parameters from being applied to the AudioSource
            this.AudioSource = audioSource;
            this.Type = audioType;
            this._clip = clip;
            this._loop = loop;
            this.Persist = persist;
            this.FadeInSeconds = fadeInValue;
            this.FadeOutSeconds = fadeOutValue;
            this._targetVolume = volume;
            this._initTargetVolume = volume;
            
            // Initliaze states
            IsPlaying = false;
            Paused = false;
            Activated = false;
            Deleted = false;
            
            // Set important values
            audioSource.Clip = _clip;
            audioSource.Volume = Volume;
        }

        /// <summary>
        /// Initializes the audiosource component with the appropriate values
        /// </summary>
        private void SetValueAudioSource(bool hasData = false)
        {
            IAudioSource audioSource = AudioSource;
            audioSource.Loop = Loop;
            audioSource.Mute = Mute;
            audioSource.Pitch = Pitch;
        }

        
        public void Update()
        {
            if (!Activated)
            {
                _fadeInterpolater = -Application.DeltaTime;
                SetValueAudioSource();
                Activated = true;
            }

            // Increase/decrease volume to reach the current target
            if (Volume != _targetVolume)
            {
                float fadeValue;
                _fadeInterpolater += Application.DeltaTime;

                if (Volume > _targetVolume)
                {
                    fadeValue = _tempFadeSeconds == -1f ? FadeOutSeconds : _tempFadeSeconds;
                }
                else
                {
                    fadeValue = _tempFadeSeconds == -1f ? FadeInSeconds : _tempFadeSeconds;
                }

                // If fadeValue = 0, then the result (_fadeInterpolater / fadeValue) will be NaN.
                // Since both values are of the float type, an exception will not be created.
                // In turn, the Lerp method uses the Mathf method for the t argument.Clamp01() which in turn returns 1 if it takes a NaN.
                Volume = MathUtil.Lerp(_onFadeStartVolume, _targetVolume, _fadeInterpolater / fadeValue);
            }
            else if (_tempFadeSeconds != -1f)
            {
                _tempFadeSeconds = -1f;
            }

            // Set the volume, taking into account the global volumes as well.
            switch (Type)
            {
                case AudioType.Music:
                    {
                        AudioSource.Volume = Volume * EazySoundManager.Instance.GlobalMusicVolume * EazySoundManager.Instance.GlobalVolume;
                        break;
                    }
                case AudioType.Sound:
                    {
                        AudioSource.Volume = Volume * EazySoundManager.Instance.GlobalSoundsVolume * EazySoundManager.Instance.GlobalVolume;
                        break;
                    }
                case AudioType.UISound:
                    {
                        AudioSource.Volume = Volume * EazySoundManager.Instance.GlobalUISoundsVolume * EazySoundManager.Instance.GlobalVolume;
                        break;
                    }
            }

            // Completely stop audio if it finished the process of stopping
            if (Volume == 0f && Stopping)
            {
                AudioSource.Stop();
                Stopping = false;
                IsPlaying = false;
                Paused = false;
            }
            // Update playing status
            else
            {
                IsPlaying = AudioSource.IsPlaying;
            }
        }
        
        public void Play()
        {
            Play(_initTargetVolume);
        }
        
        public void Play(float volume)
        {
            PlayedStart = true;
            IsPlaying = true;
            AudioSource.Play();
            SetVolume(volume);
        }

        
        public void Stop()
        {
            if (Stopping) return;

            Stopping = true;
            SetVolume(0f);
        }
        
        public void StopInstantly()
        {
            if (Stopping) return;

            Stopping = true;
            SetVolume(0f, 0f);
        }
        
        public void Pause()
        {
            Paused = true;
            AudioSource.Pause();
        }
        
        public void UnPause()
        {
            AudioSource.UnPause();
            Paused = false;
        }
        
        public void SetVolume(float volume)
        {
            if (volume > _targetVolume)
            {
                SetVolume(volume, FadeOutSeconds);
            }
            else
            {
                SetVolume(volume, FadeInSeconds);
            }
        }
 
        public void SetVolume(float volume, float fadeSeconds)
        {
            SetVolume(volume, fadeSeconds, this.Volume);
        }

        public void SetVolume(float volume, float fadeSeconds, float startVolume)
        {
            _targetVolume = MathUtil.Clamp01(volume);
            _fadeInterpolater = 0f;
            _onFadeStartVolume = startVolume;
            _tempFadeSeconds = fadeSeconds;
        }

        public void Delete()
        {
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
}