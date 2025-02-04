using Lineri.ESS.Core.Interfaces;


namespace Lineri.ESS.Core;

public interface IAudio
{
    /// <summary>
    /// The ID of the Audio
    /// </summary>
    int AudioID { get; }

    /// <summary>
    /// The type of the Audio
    /// </summary>
    AudioType Type { get; }

    /// <summary>
    /// Whether the audio is currently playing
    /// </summary>
    bool IsPlaying { get; }

    /// <summary>
    /// Whether the audio is paused
    /// </summary>
    bool Paused { get; }

    /// <summary>
    /// Whether the audio is stopping
    /// </summary>
    bool Stopping { get; }

    /// <summary>
    /// Whether the audio is created and updated at least once. 
    /// </summary>
    bool Activated { get; }

    bool PlayedStart { get; }
    bool Deleted { get; }

    /// <summary>
    /// The volume of the audio. Use SetVolume to change it.
    /// </summary>
    float Volume { get; }

    /// <summary>
    /// The audio source that is responsible for this audio. Do not modify the audiosource directly as it could result to unpredictable behaviour. Use the Audio class instead.
    /// </summary>
    IAudioSource AudioSource { get; }

    /// <summary>
    /// Audio clip to play/is playing
    /// </summary>
    IAudioClip Clip { get; }

    /// <summary>
    /// Whether the audio will be lopped
    /// </summary>
    bool Loop { get; set; }

    /// <summary>
    /// Whether the audio is muted
    /// </summary>
    bool Mute { get; set; }

    /// <summary>
    /// The pitch of the audio
    /// </summary>
    float Pitch { get; set; }
    
    public bool Persist { get; set; }
    public float FadeInSeconds { get; set; }
    public float FadeOutSeconds { get; set; }

    /// <summary>
    /// Sets the values for the class. Do not call this method manually!
    /// </summary>
    void Init( int audioID, AudioType audioType, IAudioClip clip, bool loop, bool persist, float volume, float fadeInValue,
        float fadeOutValue, IAudioSource audioSource);

    /// <summary>
    /// Update loop of the Audio. This is automatically called from the sound manager itself. Do not use this function anywhere else, as it may lead to unwanted behaviour.
    /// </summary>
    void Update();

    /// <summary>
    /// Start playing audio clip from the beginning
    /// </summary>
    void Play();

    /// <summary>
    /// Start playing audio clip from the beggining
    /// </summary>
    /// <param name="volume">The target volume</param>
    void Play(float volume);

    /// <summary>
    /// Stop playing audio clip
    /// </summary>
    void Stop();

    /// <summary>
    /// Stop instantly playing audio clip
    /// </summary>
    void StopInstantly();

    /// <summary>
    /// Pause playing audio clip
    /// </summary>
    void Pause();

    /// <summary>
    /// Resume playing audio clip
    /// </summary>
    void UnPause();

    /// <summary>
    /// Sets the audio volume
    /// </summary>
    /// <param name="volume">The target volume</param>
    void SetVolume(float volume);

    /// <summary>
    /// Sets the audio volume
    /// </summary>
    /// <param name="volume">The target volume</param>
    /// <param name="fadeSeconds">How many seconds it needs for the audio to fade in/out to reach target volume. If passed, it will override the Audio's fade in/out seconds, but only for this transition</param>
    void SetVolume(float volume, float fadeSeconds);

    /// <summary>
    /// Sets the audio volume
    /// </summary>
    /// <param name="volume">The target volume</param>
    /// <param name="fadeSeconds">How many seconds it needs for the audio to fade in/out to reach target volume. If passed, it will override the Audio's fade in/out seconds, but only for this transition</param>
    /// <param name="startVolume">Immediately set the volume to this value before beginning the fade. If not passed, the Audio will start fading from the current volume towards the target volume</param>
    void SetVolume(float volume, float fadeSeconds, float startVolume);

    void Delete();
}