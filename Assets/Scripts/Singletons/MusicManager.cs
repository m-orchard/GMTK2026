using UnityEngine;
using UnityEngine.Audio;

public class MusicManager : Singleton<MusicManager>
{
    private enum MusicPhase { None, Loop, Build, Final }

    [SerializeField] private Timer countdownTimer;
    [SerializeField] private AudioMixerGroup musicMixer;

    [SerializeField] private AudioClip loopTrack;
    [SerializeField] private AudioClip buildTrack;
    [SerializeField] private AudioClip finalTrack;

    [SerializeField, Min(0f)] private float buildStartsAtSecondsRemaining = 16f;
    [SerializeField, Min(0f)] private float finalStartsAtSecondsRemaining = 8f;

    [SerializeField, Min(0f)] private float loopFadeOutSeconds = 2f;

    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private AudioSource loopSource;
    private AudioSource primarySource;
    private MusicPhase currentPhase = MusicPhase.None;
    private float loopFadeRemaining;

    private void Awake()
    {
        loopSource = CreateMusicSource();
        loopSource.loop = true;

        primarySource = CreateMusicSource();
        primarySource.loop = false;
    }

    private void OnEnable()
    {
        countdownTimer.OnTimerStarted += PlayLoop;
        countdownTimer.OnTimerStopped += StopMusic;
    }

    private void OnDisable()
    {
        countdownTimer.OnTimerStarted -= PlayLoop;
        countdownTimer.OnTimerStopped -= StopMusic;
    }

    private void Update()
    {
        FadeLoop();

        if (!countdownTimer.IsRunning)
        {
            return;
        }

        float timeRemaining = countdownTimer.TimeRemaining;

        if (currentPhase < MusicPhase.Final && timeRemaining <= finalStartsAtSecondsRemaining)
        {
            PlayPrimary(finalTrack, MusicPhase.Final);
        }
        else if (currentPhase < MusicPhase.Build && timeRemaining <= buildStartsAtSecondsRemaining)
        {
            PlayPrimary(buildTrack, MusicPhase.Build);
            StartLoopFadeOut();
        }
    }

    private void PlayLoop()
    {
        currentPhase = MusicPhase.Loop;
        loopFadeRemaining = 0f;

        primarySource.Stop();

        if (loopTrack == null)
        {
            return;
        }

        loopSource.clip = loopTrack;
        loopSource.volume = volume;
        loopSource.Play();
    }

    private void StopMusic()
    {
        currentPhase = MusicPhase.None;
        loopFadeRemaining = 0f;
        loopSource.Stop();
        primarySource.Stop();
    }

    private void PlayPrimary(AudioClip track, MusicPhase phase)
    {
        currentPhase = phase;

        if (track == null)
        {
            return;
        }

        primarySource.clip = track;
        primarySource.volume = volume;
        primarySource.Play();
    }

    private void StartLoopFadeOut()
    {
        if (loopFadeOutSeconds <= 0f)
        {
            loopSource.Stop();
            return;
        }

        loopFadeRemaining = loopFadeOutSeconds;
    }

    private void FadeLoop()
    {
        if (loopFadeRemaining <= 0f)
        {
            return;
        }

        loopFadeRemaining -= Time.unscaledDeltaTime;

        if (loopFadeRemaining <= 0f)
        {
            loopFadeRemaining = 0f;
            loopSource.volume = 0f;
            loopSource.Stop();
            return;
        }

        loopSource.volume = volume * (loopFadeRemaining / loopFadeOutSeconds);
    }

    private AudioSource CreateMusicSource()
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        source.volume = volume;
        source.outputAudioMixerGroup = musicMixer;
        return source;
    }
}
