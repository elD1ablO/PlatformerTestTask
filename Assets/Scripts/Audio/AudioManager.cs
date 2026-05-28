using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Volume")]
    [Range(0f, 2f)]
    [SerializeField] private float _masterVolume = 1f;

    [Header("Collections")]
    [SerializeField] private SoundsCollectionSO _soundsCollection;

    [Header("Audio Mixers")]
    [SerializeField] private AudioMixerGroup _sfxMixer;
    [SerializeField] private AudioMixerGroup _musicMixer;

    [Header("Music")]
    [SerializeField] private bool _playMusicOnStart = true;

    private AudioSource _musicSource;
    private Coroutine _musicFadeRoutine;

    private const float FADE_OUT_TIME = 0.2f;
    private const float FADE_IN_TIME = 0.2f;

    #region Unity Lifecycle

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeMusicSource();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        if (_playMusicOnStart)
        {
            PlayGameMusic();
        }
    }

    #endregion

    #region Music

    public void PlayMenuMusic()
    {
        FadeToRandomMusic(_soundsCollection != null ? _soundsCollection.MenuMusic : null);
    }

    public void PlayGameMusic()
    {
        FadeToRandomMusic(_soundsCollection != null ? _soundsCollection.GameMusic : null);
    }

    public void StopMusic()
    {
        if (_musicFadeRoutine != null)
        {
            StopCoroutine(_musicFadeRoutine);
            _musicFadeRoutine = null;
        }

        if (_musicSource != null)
        {
            _musicSource.Stop();
        }
    }

    private void InitializeMusicSource()
    {
        GameObject musicObject = new GameObject("MusicSource");

        musicObject.transform.SetParent(transform);

        _musicSource = musicObject.AddComponent<AudioSource>();
        _musicSource.playOnAwake = false;
        _musicSource.outputAudioMixerGroup = _musicMixer;
        _musicSource.volume = 0f;
    }

    private void FadeToRandomMusic(SoundSO[] sounds)
    {
        if (sounds == null || sounds.Length == 0)
        {
            return;
        }

        SoundSO sound = sounds[Random.Range(0, sounds.Length)];

        FadeToMusicSound(sound, FADE_OUT_TIME, FADE_IN_TIME);
    }

    private void FadeToMusicSound(SoundSO soundSO, float fadeOutDuration, float fadeInDuration)
    {
        if (soundSO == null || soundSO.audioClip == null)
        {
            return;
        }

        if (_musicFadeRoutine != null)
        {
            StopCoroutine(_musicFadeRoutine);
        }

        _musicFadeRoutine = StartCoroutine(FadeMusicRoutine(soundSO, fadeOutDuration, fadeInDuration));
    }

    private IEnumerator FadeMusicRoutine(SoundSO soundSO, float fadeOutDuration, float fadeInDuration)
    {
        float startVolume = _musicSource.isPlaying ? _musicSource.volume : 0f;

        if (_musicSource.isPlaying && fadeOutDuration > 0f)
        {
            float timer = 0f;

            while (timer < fadeOutDuration)
            {
                timer += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(timer / fadeOutDuration);

                _musicSource.volume = Mathf.Lerp(startVolume, 0f, t);

                yield return null;
            }
        }

        _musicSource.Stop();
        _musicSource.volume = 0f;

        _musicSource.clip = soundSO.audioClip;
        _musicSource.loop = soundSO.Loop;
        _musicSource.pitch = GetPitch(soundSO);

        float targetVolume = soundSO.Volume * _masterVolume;

        _musicSource.Play();

        if (fadeInDuration > 0f)
        {
            float timer = 0f;

            while (timer < fadeInDuration)
            {
                timer += Time.unscaledDeltaTime;

                float t = Mathf.Clamp01(timer / fadeInDuration);

                _musicSource.volume = Mathf.Lerp(0f, targetVolume, t);

                yield return null;
            }
        }

        _musicSource.volume = targetVolume;

        _musicFadeRoutine = null;
    }

    #endregion

    #region Public SFX API

    public void PlayWalk()
    {
        PlayRandomSfx(_soundsCollection != null ? _soundsCollection.Walk : null);
    }

    public void PlayJump()
    {
        PlayRandomSfx(_soundsCollection != null ? _soundsCollection.Jump : null);
    }

    public void PlayLand()
    {
        PlayRandomSfx(_soundsCollection != null ? _soundsCollection.Land : null);
    }

    public void PlayHeadHit()
    {
        PlayRandomSfx(_soundsCollection != null ? _soundsCollection.HeadHit : null);
    }

    public void PlayWallHit()
    {
        PlayRandomSfx(_soundsCollection != null ? _soundsCollection.WallHit : null);
    }

    public void PlayBonusCubeHit()
    {
        PlayRandomSfx(_soundsCollection != null ? _soundsCollection.BonusCubeHit : null);
    }

    public void PlayBonusEffect()
    {
        PlayRandomSfx(_soundsCollection != null ? _soundsCollection.BonusEffect : null);
    }

    #endregion

    #region SFX

    private void PlayRandomSfx(SoundSO[] sounds)
    {
        if (sounds == null || sounds.Length == 0)
        {
            return;
        }

        SoundSO sound = sounds[Random.Range(0, sounds.Length)];

        PlaySfx(sound);
    }

    private void PlaySfx(SoundSO soundSO)
    {
        if (soundSO == null || soundSO.audioClip == null)
        {
            return;
        }

        GameObject sfxObject = new GameObject($"SFX_{soundSO.name}");

        sfxObject.transform.SetParent(transform);

        AudioSource source = sfxObject.AddComponent<AudioSource>();

        source.playOnAwake = false;
        source.outputAudioMixerGroup = _sfxMixer;
        source.clip = soundSO.audioClip;
        source.loop = soundSO.Loop;
        source.pitch = GetPitch(soundSO);
        source.volume = soundSO.Volume * _masterVolume;

        source.Play();

        if (!source.loop)
        {
            Destroy(sfxObject, source.clip.length + 0.1f);
        }
    }

    private float GetPitch(SoundSO soundSO)
    {
        if (!soundSO.RandomizePitch)
        {
            return soundSO.Pitch;
        }

        float randomPitch = Random.Range(
            -soundSO.RandomPitchModifier,
            soundSO.RandomPitchModifier);

        return soundSO.Pitch + randomPitch;
    }

    #endregion
}