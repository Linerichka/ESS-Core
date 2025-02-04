using System;
using System.Collections.Generic;
using Lineri.ESS.Core.Base;
using Lineri.ESS.Core.Interfaces;
using Lineri.ESS.Core.Utils;

namespace Lineri.ESS.Core
{
    public class SoundPocket : BaseClass
    {
        // Here Sound - all sounds \ audio clips
        private float _soundVolume = 1f;
        public float SoundVolume
        {
            get => _soundVolume;
            set
            {
                _soundVolume = MathUtil.Clamp(value, 0f, 1);
                UpdateParametersAllPlayingClips(_typeUpdate.Volume);
            }
        }

        private float _pitch = 1f;
        public float Pitch
        {
            get => _pitch;
            set
            {
                _pitch = MathUtil.Clamp(value, -3f, 3f);
                UpdateParametersAllPlayingClips(_typeUpdate.Pitch);
            }
        }

        private float _randomPitch = 0f;
        public float RandomPitch
        {
            get => _randomPitch;
            set => _randomPitch = MathUtil.Clamp(value, 0, 3f);
        }

        // Use the plugin documentation for these 3 variables
        private float _fadeInSecond = 0f;
        public float FadeInSecond
        {
            get => _fadeInSecond;
            set
            {
                if (value < 0f) _fadeInSecond = 0f;
                else _fadeInSecond = value;
            }
        } 
        
        private float _fadeOutSecond = 0f;
        public float FadeOutSecond
        {
            get => _fadeOutSecond;
            set
            {
                if (value < 0f) _fadeOutSecond = 0f;
                else _fadeOutSecond = value;
            }
        }

        
        private float _currentMusicFadeOutSeconds = -1f;
        public float CurrentMusicFadeOut
        {
            get => _currentMusicFadeOutSeconds;
            set
            {
                if (MathUtil.Approximately(-1f, value)) _currentMusicFadeOutSeconds = -1f;
                else if (value < 0f) _currentMusicFadeOutSeconds = 0f;
                else _currentMusicFadeOutSeconds = value;
            }
        }
        
        //
        // If true, then fade in applies only to the first clip in the queue
        public bool FadeInFirstClipInQueue = false;
        
        /// If true, fade in applies only to the clip that is played after calling the Play method from outside. 
        /// Also, if Loop is set to True, then fade in will not be applied to the first clip when the queue is repeated.
        public bool FadeInClipAfterCallPlay = false;

        
        public bool LoopClips = false;
        
        // If true, plays clips in the lists in random order. ONLY FOR CLIPS ONE AFTER THE OTHER
        public bool RandomPlayClip = false;
        
        /// <summary>
        /// Keep sound on scene change
        /// </summary>
        /// continues the sound that is played at the moment of the scene change,
        /// audio queue won't play if the current class is not in DontDestroyOnLoad
        public bool PersistSoundOnSceneLoad = false;
        
        // When called, Play plays one clip instead of the entire chain. ignores LoopClips
        public bool PlayOneClip = false;
        
        // Play all clips at the start
        public bool PlaySoundOnAwake = false;
        
        // Plays all the assigned clips at the same time. This may not work with EazySoundManager.IgnoreDuplicateMusic/Sound/UiSound == true
        public bool PlayAllClipsTogether = false;
        
        // After the end of the current clip, the new ones will no longer be played
        public bool PlayClips = true;
        
        
        public List<IAudioClip> MusicClips = new List<IAudioClip>();
        public List<IAudioClip> SoundClips = new List<IAudioClip>();
        public List<IAudioClip> SoundUiClips = new List<IAudioClip>();

        #region CallBacks
        public event Action OnClipsQueuePlayedStart;
        // Called only if PlayAllClipsTogether = false
        public event Action OnClipPlayedStart;
        // It can be called repeatedly
        public event Action ClipsQueueEnded;
        public event Action ClipsQueueReset;
        #endregion
        
        #region Private variables
        //if Start() was called earlier - true, if not - false
        private bool _startInvoked = false;
        //true if the clips were called to play together
        private bool _clipsTogetherPlayCalled;
        private int _numberClipPlayedInList = 0;
        protected bool _playWasCalled = false;
        private List<Audio> _audioFromClips = new List<Audio>(4);
        private bool _audioFromClipsChanged = false;
        //use it to force new clips to start, even if the clips are already being played
        private bool _ignorePlayingCurrentClips = false;
        private bool _callPlay = true;
        #endregion

        protected virtual void Start()
        {
            SetVariables();
            PlayOnAwakeningIfConditionIsMet();
        }

        private void Update()
        {
            _audioFromClipsChanged = true;
            PlayAudioFromList(false);
        }

        #region Public void
        public virtual void Play()
        {
            if (!CheckInstalledClipsAndDisableIfNotInstalled()) return;

            _playWasCalled = true;
            PlayClips = true;

            PlayAudioFromList(true);
        }

        public void ResetClipQueue()
        {
            ClipsQueueReset?.Invoke();
            _numberClipPlayedInList = 0;
            _clipsTogetherPlayCalled = false;
        }

        public void StopClipsPlayning()
        {
            foreach (Audio audio in GetAudioFromClips()) audio.Stop();

            PlayClips = false;

            ResetClipQueue();
        }

        public void PauseClipsPlayning()
        {
            foreach (Audio audio in GetAudioFromClips()) audio.Pause();
        }

        public void UnPauseClipsPlayning()
        {
            foreach (Audio audio in GetAudioFromClips()) audio.UnPause();
        }

        public void ResetTimeClipsPlayed()
        {
            _ignorePlayingCurrentClips = true;
        }

        /// <summary>
        /// Sets the volume instantly, ignoring the Fade In and Fade Out values.
        /// </summary>
        public void SetVolumeInstantly()
        {
            foreach (Audio audio in GetAudioFromClips()) audio.SetVolume(SoundVolume, 0f);
        }
        #endregion

        private enum _typeUpdate
        {
            All,
            Volume,
            Pitch
        }

        /// <summary>
        /// Update the parameters of the clips that are currently playing (pitch, volume).
        /// </summary>
        private void UpdateParametersAllPlayingClips(_typeUpdate typeUpdate)
        {
            if (!(Application.IsPlaying && _startInvoked)) return;

            foreach (Audio audio in GetAudioFromClips())
            {
                if (typeUpdate == _typeUpdate.All)
                {
                    audio.SetVolume(SoundVolume);
                    audio.Pitch = Pitch;
                    continue;
                }
                else if (typeUpdate == _typeUpdate.Volume) audio.SetVolume(SoundVolume);
                else if (typeUpdate == _typeUpdate.Pitch) audio.Pitch = Pitch;
            }
        }

        #region PlayClip
        protected virtual void PlayAudioFromList(bool callPlay)
        {
            if (!_playWasCalled || !PlayClips || !EazySoundManager.Instance.CanPlay) return;

            if (PlayAllClipsTogether) PlayAllClipsInListTogether(callPlay);        
            else PlayClipsOneAfterTheOther(callPlay);          
        }

        #region Play Together
        private void PlayAllClipsInListTogether(bool callPlay)
        {
            if (!((_clipsTogetherPlayCalled && LoopClips) || callPlay)) return;
            if (!PlaybackOfClipsIsComplete()) return;
            else if (callPlay) OnClipsQueuePlayedStart?.Invoke();

            _callPlay = callPlay;

            PlayAllClipsInList(MusicClips, AudioType.Music);
            PlayAllClipsInList(SoundClips, AudioType.Sound);
            PlayAllClipsInList(SoundUiClips, AudioType.UISound);

            _clipsTogetherPlayCalled = true;
        }

        private void PlayAllClipsInList(List<IAudioClip> listClips, AudioType AudioType)
        {
            foreach (IAudioClip clip in listClips)
            {
                PlayClipInListTogether(clip, AudioType);
            }
        }

        private Audio PlayClipInListTogether(IAudioClip clip, AudioType AudioType)
        {
            Audio audio = PlayAudio(clip, AudioType);
            float clipPitch = Pitch + MathUtil.RandomRange(-RandomPitch, RandomPitch);
            audio.Pitch = clipPitch;

            return audio;
        }
        #endregion

        #region Play One After The Other
        protected void PlayClipsOneAfterTheOther(bool callPlay)
        {
            if (!(MusicClips.Count > _numberClipPlayedInList ||
                SoundClips.Count > _numberClipPlayedInList ||
                SoundUiClips.Count > _numberClipPlayedInList))
            {
                if (LoopClips || callPlay) ResetClipQueue();
            }

            if (!PlaybackOfClipsIsComplete() || (!callPlay && PlayOneClip))
            {
                return;
            }
            else if (callPlay) OnClipsQueuePlayedStart?.Invoke();

            OnClipPlayedStart?.Invoke();
            _callPlay = callPlay;

            if (MusicClips.Count > _numberClipPlayedInList)
            {
                PlayClipInList(MusicClips, AudioType.Music);
            }

            if (SoundClips.Count > _numberClipPlayedInList)
            {
                PlayClipInList(SoundClips, AudioType.Sound);
            }

            if (SoundUiClips.Count > _numberClipPlayedInList)
            {
                PlayClipInList(SoundUiClips, AudioType.UISound);
            }

            _numberClipPlayedInList++;
        }

        private Audio PlayClipInList(List<IAudioClip> clips, AudioType AudioType)
        {
            int clipIndex;

            if (RandomPlayClip) clipIndex = MathUtil.RandomRange(0, clips.Count);         
            else clipIndex = _numberClipPlayedInList;           

            Audio audio = PlayAudio(clips[clipIndex], AudioType);
            float clipPitch = Pitch + MathUtil.RandomRange(-RandomPitch, RandomPitch);
            audio.Pitch = clipPitch;

            return audio;
        }
        #endregion

        private Audio PlayAudio(IAudioClip clip, AudioType AudioType)
        {
            switch (AudioType)
            {
                case AudioType.Music:
                    return PlayAudioMusic(clip);
                case AudioType.Sound:
                    return PlayAudioSound(clip);
                case AudioType.UISound:
                    return PlayAudioUISound(clip);
                default:
                    return null;
            }
        }
        
        protected virtual Audio PlayAudioMusic(IAudioClip clip)
        {
            Audio audio = EazySoundManager.Instance.PlayMusic(clip, SoundVolume, false, PersistSoundOnSceneLoad,
                FadeInCanSet() ? FadeInSecond : 0f,
                FadeOutSecond, CurrentMusicFadeOut);
            _audioFromClips.Add(audio);
            return audio;
        }
        
        protected virtual Audio PlayAudioSound(IAudioClip clip)
        {
            Audio audio = EazySoundManager.Instance.PlaySound(clip, SoundVolume, false, PersistSoundOnSceneLoad,
                FadeInCanSet() ? FadeInSecond : 0f, 
                FadeOutSecond);
            _audioFromClips.Add(audio);
            return audio;
        }

        /// <summary>
        /// If there are no conditions prohibiting setting Fade In for the clip, then returns True.
        /// </summary>
        private bool FadeInCanSet()
        {
            // Check whether we can apply fade in to the clip in normal mode if there are no forbidding conditions
            bool canApplyFadeInToClip = (!FadeInFirstClipInQueue && !FadeInClipAfterCallPlay);
            // Check whether we can apply fade in to the clip if it is the first in the queue and the conditions allow
            bool canApplyFadeInToFirstClip = (FadeInFirstClipInQueue && _numberClipPlayedInList == 0 && !PlayAllClipsTogether);
            // Check whether we can apply fade in to the clip if it is playing after calling Play.
            bool canApplyFadeInBecausePlayningClipAfterCallPlay = (FadeInClipAfterCallPlay && _callPlay);
            bool fadeInSet = (canApplyFadeInToFirstClip || canApplyFadeInBecausePlayningClipAfterCallPlay || canApplyFadeInToClip);
            return fadeInSet;
        }

        protected virtual Audio PlayAudioUISound(IAudioClip clip)
        {
            Audio audio = EazySoundManager.Instance.PlayUISound(clip, SoundVolume);
            _audioFromClips.Add(audio);
            return audio;
        }
        #endregion
        
        /// <summary>
        /// Returns Audio of the current clips.
        /// </summary>
        private List<Audio> GetAudioFromClips()
        {
            if (!_audioFromClipsChanged)
            {
                return _audioFromClips;
            }
            
            int length = _audioFromClips.Count;
            for (int i = 0; i < length; i++)
            {
                Audio audio = _audioFromClips[i];
                
                if (audio == null || audio.Deleted)
                {
                    _audioFromClips.RemoveAt(i);
                    length--;
                    i--;
                }
            }

            _audioFromClipsChanged = false;
            return _audioFromClips;
        }

        /// <summary>
        /// If the clips are no longer playing, returns true.
        /// If there is at least one clip that is being played, it returns false.
        /// </summary>
        private bool PlaybackOfClipsIsComplete()
        {
            if (_ignorePlayingCurrentClips)
            {
                _ignorePlayingCurrentClips = false;
                ClipsQueueEnded?.Invoke();
                return true;
            }

            if (GetAudioFromClips().Count > 0) return false;

            ClipsQueueEnded?.Invoke();
            return true;
        }

        private void PlayOnAwakeningIfConditionIsMet()
        {
            if (!PlaySoundOnAwake) return;

            Play();
        }

        /// <summary>
        /// Set values at the start
        /// </summary>
        protected virtual void SetVariables()
        {
            _startInvoked = true;
        }

        /// <summary>
        /// Returns true if clips are installed.
        /// If the clips are not set, it calls gameObject.SetActive(false) and returns false.
        /// </summary>
        private bool CheckInstalledClipsAndDisableIfNotInstalled()
        {
            if (MusicClips.Count == 0 &&
                SoundClips.Count == 0 &&
                SoundUiClips.Count == 0)
            {
                throw new Exception("Clips not set");
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
