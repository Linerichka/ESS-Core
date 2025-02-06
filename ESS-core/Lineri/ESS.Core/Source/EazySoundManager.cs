//If seted, when you try to play an already existing clip, this call will be ignored,
//you will receive a link to the existing Audio. No duplicates. Рas little effect on performance.
//#define ESS_OnlyOneClip

using System;
using System.Collections.Generic;
using Lineri.ESS.Core.Base;
using Lineri.ESS.Core.Interfaces;
using Lineri.ESS.Core.Utils;

namespace Lineri.ESS.Core
{
    /// <summary>
    /// Static class responsible for playing and managing audio and sounds.
    /// </summary>
    public partial class EazySoundManager<TAudio, TSource> : BaseClass, IEazySoundManager<TAudio, TSource>
    where TAudio : class, IAudio, new()
    where TSource : class, IAudioSource, new()
    {
        public bool CanPlayInBackground = true;
        public bool CanPlay => Application.IsFocused || CanPlayInBackground;
        
        public float GlobalVolume
        {
            get => _globalVolume;
            set => _globalVolume = float.IsNaN(value) ? _globalVolume : MathUtil.Clamp01(value);
        }
        private float _globalVolume = 1f;
        
        public float GlobalMusicVolume
        {
            get => _globalMusicVolume;
            set => _globalMusicVolume = float.IsNaN(value) ? _globalMusicVolume : MathUtil.Clamp01(value);
        }
        private float _globalMusicVolume = 1f;
        
        public float GlobalSoundVolume
        {
            get => _globalSoundVolume;
            set => _globalSoundVolume = float.IsNaN(value) ? _globalSoundVolume : MathUtil.Clamp01(value);
        }
        private float _globalSoundVolume = 1f;

        public float GlobalUISoundVolume
        {
            get => _globalUiSoundVolume;
            set => _globalUiSoundVolume = float.IsNaN(value) ? _globalUiSoundVolume : MathUtil.Clamp01(value);
        }
        private float _globalUiSoundVolume = 1f;

        #region Oprimize
        private Queue<IAudioSource> _cachedAudioSourceOnGameobject;
        private Queue<TAudio> _cachedAudio;
        #endregion

        private ListAudio<TAudio> _musicAudio;
        private ListAudio<TAudio> _soundsAudio;
        private ListAudio<TAudio> _UISoundsAudio;
        #if ESS_OnlyOneClip
        // For one clip playble
        private Dictionary<IAudioClip, TAudio> _clipAudioDictionary; 
        #endif

        private bool _pausedByLostFocus = false;
        
        public static EazySoundManager<TAudio, TSource> Instance
        {
            get => _instance;
            private set => _instance = value;
        }
        private static EazySoundManager<TAudio, TSource>  _instance = null;
        private static bool _isInitialized = false;

        public EazySoundManager()
        {
            if (_isInitialized)
            {
                throw new System.Exception("More one singlton.");
            }

            Instance = this;
            Instance.Init();
            
            _isInitialized = true;

            Application.SceneLoaded += OnSceneLoaded;
        }

        public void Dispose()
        {
            Application.SceneLoaded -= OnSceneLoaded;
            Instance = null;
            _isInitialized = false;
        }

        /// <summary>
        /// Initialized the sound manager
        /// </summary>
        private void Init()
        {
            _musicAudio = new ListAudio<TAudio>(64);
            _soundsAudio = new ListAudio<TAudio>(64);
            _UISoundsAudio = new ListAudio<TAudio>(64);
            _cachedAudioSourceOnGameobject = new Queue<IAudioSource>(64);
            _cachedAudio = new Queue<TAudio>(64);

            #if ESS_OnlyOneClip
            _clipAudioDictionary = new Dictionary<IAudioClip, TAudio>(128);
            #endif
        }

        private void OnSceneLoaded()
        {
            // Stop and remove all non-persistent audio
            RemoveNonPersistAudio(_musicAudio);
            RemoveNonPersistAudio(_soundsAudio);
            RemoveNonPersistAudio(_UISoundsAudio);
            //
            DeleteUnusedAudioSource();
        }

        public void Update()
        {
            if (CanPlay)
            {
                if (_pausedByLostFocus)
                {
                    _pausedByLostFocus = false;
                    UnPauseAllAudio();
                }
                
                UpdateAllAudio(_musicAudio);
                UpdateAllAudio(_soundsAudio);
                UpdateAllAudio(_UISoundsAudio);
            }
            else
            {
                PauseAllAudio();
                _pausedByLostFocus = true;
            }
        }
        
        /// <summary>
        /// Updates the state of all audios of an audio dist
        /// </summary>
        /// <param name="listAudio">The audio dist to update</param>
        private void UpdateAllAudio(ListAudio<TAudio> listAudio)
        {
            // Go through all audios and update them
            int count = listAudio.Count;
            for (int i = 0; i < count; i++)
            {
                TAudio audio = listAudio[i];

                if (audio == null || audio.Paused || !audio.PlayedStart) continue;

                audio.Update();

                // Remove it if it is no longer active (playing)
                if (!audio.IsPlaying)
                {
                    DeleteAudio(audio, listAudio, i);
                }
            }
        }

        /// <summary>
        /// Retrieves the audio dist based on the audioType
        /// </summary>
        /// <param name="audioType">The audio type of the dist to return</param>
        /// <returns>An audio dist</returns>
        private ListAudio<TAudio> GetAudioTypeList(AudioType audioType)
        {
            ListAudio<TAudio> listAudio = null;

            switch (audioType)
            {
                case AudioType.Music:
                    listAudio = _musicAudio;
                    break;
                case AudioType.Sound:
                    listAudio = _soundsAudio;
                    break;
                case AudioType.UISound:
                    listAudio = _UISoundsAudio;
                    break;
            }

            return listAudio;
        }
        
    
        #region GetAudio Functions
        public TAudio GetAudio(AudioType audioType, int audioID)
        {
            ListAudio<TAudio> listAudio = GetAudioTypeList(audioType);

            if (!listAudio.Contains(audioID)) return null;

            return listAudio[audioID];
        }

        public List<TAudio> GetAudios(AudioType audioType, IAudioClip audioClip)
        {
            ListAudio<TAudio> listAudio = GetAudioTypeList(audioType);
            List<TAudio> result = new List<TAudio>();
            
            int count = listAudio.Count;
            for (int i = 0; i < count; i++)
            {
                TAudio audio = listAudio[i];
                
                if (audio != null && audio.Clip == audioClip) result.Add(audio);
            }
            
            return result;
        }
        #endregion
        
        #region Play Functions
        public TAudio PlayAudio(AudioType audioType, IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false,
            float fadeInSeconds = 0f, float fadeOutSeconds = 0f)
        {
            TAudio audio = PrepareAudio(audioType, clip, volume, loop, persist, fadeInSeconds, fadeOutSeconds);
            audio.Play();

            return audio;
        }
        #endregion

        #region Prepare Function
        public TAudio PrepareAudio(AudioType audioType, IAudioClip clip, float volume = 1f, bool loop = false, bool persist = false,
            float fadeInSeconds = 0f, float fadeOutSeconds = 0f)
        {
            #if DEBUG
            if (clip == null)
            {
                throw new System.Exception("[Eazy Sound Manager] Audio clip is null");
            }
            #endif
            #if ESS_OnlyOneClip
            if (_clipAudioDictionary.TryGetValue(clip, out TAudio audioOld))
            {
                return audioOld;
            }
            #endif

            ListAudio<TAudio> listAudio = GetAudioTypeList(audioType);
            int id = listAudio.GetFreeIndex();
            
            TAudio audio = GetAudioClass();
            audio.Init(id, audioType, clip, loop, persist, volume, fadeInSeconds, fadeOutSeconds, GetAudioSource());

            // Add it to list
            listAudio[id] = audio;
            
            #if ESS_OnlyOneClip
            _clipAudioDictionary.Add(clip, audio);
            #endif
            
            return audio;
        }

        private TAudio GetAudioClass()
        {
            TAudio audio;
            
            if (!_cachedAudio.TryDequeue(out audio))
            {
                audio = new TAudio();
            }

            return audio;
        }
        
        private IAudioSource GetAudioSource()
        {
            IAudioSource audioSource;

            if (!_cachedAudioSourceOnGameobject.TryDequeue(out audioSource))
            {
                audioSource = new TSource();
            }

            return audioSource;
        }
        #endregion
        
        public void StopAudio(AudioType audioType, float fadeOutSeconds = -1f)
        {
            ListAudio<TAudio> listAudio = GetAudioTypeList(audioType);

            foreach (TAudio audio in listAudio)
            {
                if (fadeOutSeconds >= 0) audio.FadeOutSeconds = fadeOutSeconds;

                audio.Stop();
            }
        }
        
        public void PauseAudio(AudioType audioType)
        {
            ListAudio<TAudio> listAudio = GetAudioTypeList(audioType);

            foreach (TAudio audio in listAudio)
            {
                audio.Pause();
            }
        }
        
        public void UnPauseAudio(AudioType audioType)
        {
            ListAudio<TAudio> listAudio = GetAudioTypeList(audioType);

            foreach (TAudio audio in listAudio)
            {
                audio.UnPause();
            }
        }

        #region Delete and remove Audio
        /// <summary>
        /// Remove all non-persistant audios from an audio dist
        /// </summary>
        /// <param name="listAudio">The audio dist whose non-persistant audios are getting removed</param>
        private void RemoveNonPersistAudio(ListAudio<TAudio> listAudio)
        {
            // Go through all audios and remove them if they should not persist through scenes
            for (int i = 0; i < listAudio.Count; i++)
            {
                TAudio audio = listAudio[i];

                if (audio == null || (audio.Persist && audio.Activated)) continue;

                DeleteAudio(audio, listAudio, i);
            }
        }

        private void DeleteAudio(TAudio audio, ListAudio<TAudio> listAudio, int key)
        {
            #if ESS_OnlyOneClip
            _clipAudioDictionary.Remove(audio.Clip);
            #endif
            
            audio.AudioSource.Clip = null;
            _cachedAudioSourceOnGameobject.Enqueue(audio.AudioSource);

            audio.Delete();
            listAudio[key] = null;
            _cachedAudio.Enqueue(audio);
        }

        /// <summary>
        /// Clear the AudioSource queue and delete the AudioSource if they do not contain a clip.
        /// </summary>
        private void DeleteUnusedAudioSource()
        {
            while (_cachedAudioSourceOnGameobject.TryDequeue(out IAudioSource audioSource))
            {
                if (audioSource.Clip != null) continue;

                Destroy(audioSource);
            }
        }
        #endregion

    }
}
