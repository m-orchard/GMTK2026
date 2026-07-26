using UnityEngine;
using UnityEngine.Audio;

[RequireComponent(typeof(AudioSource))]
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

    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private AudioSource musicSource;
    private MusicPhase currentPhase = MusicPhase.None;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.spatialBlend = 0f;
        musicSource.volume = volume;
        musicSource.outputAudioMixerGroup = musicMixer;
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
        if (!countdownTimer.IsRunning)
        {
            return;
        }

        float timeRemaining = countdownTimer.TimeRemaining;

        if (currentPhase < MusicPhase.Final && timeRemaining <= finalStartsAtSecondsRemaining)
        {
            PlayTrack(finalTrack, MusicPhase.Final);
        }
        else if (currentPhase < MusicPhase.Build && timeRemaining <= buildStartsAtSecondsRemaining)
        {
            PlayTrack(buildTrack, MusicPhase.Build);
        }
    }

    private void PlayLoop()
    {
        PlayTrack(loopTrack, MusicPhase.Loop);
    }

    private void StopMusic()
    {
        currentPhase = MusicPhase.None;
        musicSource.Stop();
    }

    private void PlayTrack(AudioClip track, MusicPhase phase)
    {
        currentPhase = phase;

        if (track == null)
        {
            return;
        }

        musicSource.clip = track;
        musicSource.loop = phase == MusicPhase.Loop;
        musicSource.Play();
    }
}
