//If seted, when you try to play an already existing clip, this call will be ignored,
//you will receive a link to the existing Audio. No duplicates. Рas little effect on performance.
#define ESS_OnlyOneClip

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
    public sealed partial class EazySoundManager : BaseClass, IDisposable
    {
        /// <summary>
        /// If set to true, when starting a new music Clip, all others will be stopped
        /// </summary>
        public static bool OnlyOnePlayableMusicClip = true;

        public static bool CanPlayInBackground = true;
        public static bool CanPlay => Application.IsFocused || CanPlayInBackground;

        /// <summary>
        /// Global volume ranging from 0 to 1
        /// </summary>
        public static float GlobalVolume
        {
            get => _globalVolume;
            set => _globalVolume = MathUtil.Clamp01(value);
        }
        private static float _globalVolume = 1f;

        /// <summary>
        /// Global music volume ranging from 0 to 1
        /// </summary>
        public static float GlobalMusicVolume
        {
            get => _globalMusicVolume;
            set => _globalMusicVolume = MathUtil.Clamp01(value);
        }
        private static float _globalMusicVolume = 1f;

        /// <summary>
        /// Global sounds volume ranging from 0 to 1
        /// </summary>
        public static float GlobalSoundsVolume
        {
            get => _globalSoundsVolume;
            set => _globalSoundsVolume = MathUtil.Clamp01(value);
        }
        private static float _globalSoundsVolume = 1f;

        /// <summary>
        /// Global UI sounds volume ranging from 0 to 1
        /// </summary>
        public static float GlobalUISoundsVolume
        {
            get => _globalUISoundsVolume;
            set => _globalUISoundsVolume = MathUtil.Clamp01(value);
        }
        private static float _globalUISoundsVolume = 1f;

        #region Oprimize
        private static Queue<IAudioSource> _cachedAudioSourceOnGameobject;
        private static Queue<Audio> _cachedAudio;
        #endregion

        private static ListAudio _musicAudio;
        private static ListAudio _soundsAudio;
        private static ListAudio _UISoundsAudio;
#if ESS_OnlyOneClip
        // For one clip playble
        private static Dictionary<IAudioClip, Audio> _clipAudioDictionary; 
#endif
        
        
        private static bool _pausedByLostFocus = false;
        

        public static EazySoundManager Instance
        {
            get => _instance;
            private set => _instance = value;
        }
        private static EazySoundManager _instance = null;
        private static bool _isInitialized = false;

        private EazySoundManager()
        {
            if (_isInitialized)
            {
                throw new System.Exception("More one singlton.");
            }

            _isInitialized = true;

            Application.SceneLoaded += OnSceneLoaded;
        }
        public void Dispose() => Application.SceneLoaded -= OnSceneLoaded;

        static EazySoundManager()
        {
            Instance = new EazySoundManager();

            Instance.Init();
        }

        /// <summary>
        /// Initialized the sound manager
        /// </summary>
        private void Init()
        {
            _musicAudio = new ListAudio(64);
            _soundsAudio = new ListAudio(64);
            _UISoundsAudio = new ListAudio(64);
            _cachedAudioSourceOnGameobject = new Queue<IAudioSource>(64);
            _cachedAudio = new Queue<Audio>(64);

            #if ESS_OnlyOneClip
            _clipAudioDictionary = new Dictionary<IAudioClip, Audio>(128);
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
        private static void UpdateAllAudio(ListAudio listAudio)
        {
            // Go through all audios and update them
            int count = listAudio.Count;
            for (int i = 0; i < count; i++)
            {
                Audio audio = listAudio[i];

                if (audio == null || audio.Paused || !audio.PlayedStart) continue;

                audio.Update();

                // Remove it if it is no longer active (playing)
                if (!audio.IsPlaying)
                {
                    DeleteAudio(audio, listAudio, ref i);
                }
            }
        }

        /// <summary>
        /// Retrieves the audio dist based on the audioType
        /// </summary>
        /// <param name="audioType">The audio type of the dist to return</param>
        /// <returns>An audio dist</returns>
        private static ListAudio GetAudioTypeList(AudioType audioType)
        {
            ListAudio listAudio;

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
                default:
                    return null;
            }

            return listAudio;
        }
        
    
        #region GetAudio Functions
        public static Audio GetAudio(AudioType audioType, int audioID)
        {
            ListAudio listAudio = GetAudioTypeList(audioType);

            if (!listAudio.Contains(audioID)) return null;

            return listAudio[audioID];
        }

        public static List<Audio> GetAudios(AudioType audioType, IAudioClip audioClip)
        {
            ListAudio listAudio = GetAudioTypeList(audioType);
            List<Audio> result = new List<Audio>();
            
            int count = listAudio.Count;
            for (int i = 0; i < count; i++)
            {
                Audio audio = listAudio[i];
                
                if (audio != null && audio.Clip == audioClip) result.Add(audio);
            }
            
            return result;
        }
        #endregion
        
        #region Play Functions
        public static Audio PlayAudio(AudioType audioType, IAudioClip clip, float volume, bool loop, bool persist,
            float fadeInSeconds, float fadeOutSeconds, float currentMusicFadeOutSeconds = 0f)
        {
            // Stop all current music playing
            if (OnlyOnePlayableMusicClip && audioType == AudioType.Music)
            {
                StopAudio(AudioType.Music, currentMusicFadeOutSeconds);
            }

            Audio audio = PrepareAudio(audioType, clip, volume, loop, persist, fadeInSeconds, fadeOutSeconds);
            audio.Play();

            return audio;
        }
        #endregion

        #region Prepare Function
        public static Audio PrepareAudio(AudioType audioType, IAudioClip clip, float volume, bool loop, bool persist,
            float fadeInSeconds, float fadeOutSeconds)
        {
            #if DEBUG
            if (clip == null)
            {
                throw new System.Exception("[Eazy Sound Manager] Audio clip is null");
            }
            #endif
            #if ESS_OnlyOneClip
            if (_clipAudioDictionary.TryGetValue(clip, out Audio audioOld))
            {
                return audioOld;
            }
            #endif

            ListAudio listAudio = GetAudioTypeList(audioType);
            int id = listAudio.GetFreeIndex();
            
            Audio audio = GetAudioClass();
            audio.Init(id, audioType, clip, loop, persist, volume, fadeInSeconds, fadeOutSeconds, GetAudioSource());

            // Add it to list
            listAudio[id] = (audio);
            
            #if ESS_OnlyOneClip
            _clipAudioDictionary.Add(clip, audio);
            #endif
            
            return audio;
        }

        private static Audio GetAudioClass()
        {
            Audio audio;
            
            if (!_cachedAudio.TryDequeue(out audio))
            {
                audio = new Audio();
            }

            return audio;
        }
        
        private static IAudioSource GetAudioSource()
        {
            IAudioSource audioSource;

            if (!_cachedAudioSourceOnGameobject.TryDequeue(out audioSource))
            {
                audioSource = new AudioSource();
            }

            return audioSource;
        }
        #endregion

        #region Stop Functions
        public static void StopAll()
        {
            StopAudio(AudioType.Music);
            StopAudio(AudioType.Sound);
            StopAudio(AudioType.UISound);
        }

        public static void StopAudio(AudioType audioType, float fadeOutSeconds = -1f)
        {
            ListAudio listAudio = GetAudioTypeList(audioType);

            foreach (Audio audio in listAudio)
            {
                if (fadeOutSeconds >= 0) audio.FadeOutSeconds = fadeOutSeconds;

                audio.Stop();
            }
        }
        #endregion

        #region Pause Functions
        public static void PauseAudio(AudioType audioType)
        {
            ListAudio listAudio = GetAudioTypeList(audioType);

            foreach (Audio audio in listAudio)
            {
                audio.Pause();
            }
        }

        public static void PauseAllAudio()
        {
            PauseAudio(AudioType.Music);
            PauseAudio(AudioType.Sound);
            PauseAudio(AudioType.UISound);
        }
        #endregion

        #region Resume Functions
        public static void UnPauseAudio(AudioType audioType)
        {
            ListAudio listAudio = GetAudioTypeList(audioType);

            foreach (Audio audio in listAudio) audio.UnPause();
        }

        public static void UnPauseAllAudio()
        {
            UnPauseAudio(AudioType.Music);
            UnPauseAudio(AudioType.Sound);
            UnPauseAudio(AudioType.UISound);
        }
        #endregion

        #region Delete and remove Audio
        /// <summary>
        /// Remove all non-persistant audios from an audio dist
        /// </summary>
        /// <param name="listAudio">The audio dist whose non-persistant audios are getting removed</param>
        private static void RemoveNonPersistAudio(ListAudio listAudio)
        {
            // Go through all audios and remove them if they should not persist through scenes
            for (int i = 0; i < listAudio.Count; i++)
            {
                Audio audio = listAudio[i];

                if (audio.Persist && audio.Activated) continue;

                DeleteAudio(audio, listAudio, ref i);
            }
        }

        private static void DeleteAudio(Audio audio, ListAudio listAudio, ref int key)
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
        private static void DeleteUnusedAudioSource()
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
