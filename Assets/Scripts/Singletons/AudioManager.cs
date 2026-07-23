using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class AudioClipOptions {
    public float Volume { get; set; } = 1f;
    public float Pitch { get; set; } = 1f;

    public bool RandomPitch = false;

    public float PitchRange = 0.1f;

    public bool Loop = false;
}

public class AudioManager : Singleton<AudioManager> {

    [SerializeField]
    private AudioMixerGroup masterMixer;

    [SerializeField]
    private AudioMixerGroup musicMixer;

    [SerializeField]
    private AudioMixerGroup sfxMixer;

    private AudioClip GetRandomClip(List<AudioClip> clips) {
        int randomIndex = Random.Range(0, clips.Count);
        return clips[randomIndex];
    }

    public void PlaySound(AudioClip clip, AudioClipOptions options = null) {
        PlaySound(clip, Camera.main.transform, options);
    }

    public void PlaySound(List<AudioClip> clips, Vector3 worldPosition, AudioClipOptions options = null) {
        //get a random sound clip
        var clip = GetRandomClip(clips);

        PlaySound(clip, worldPosition, options);
    }

    public void PlaySound(AudioClip clip, Vector3 worldPosition, AudioClipOptions options = null) {
        if (!clip) {
            return;
        }

        GameObject soundGameObject = CreateSoundGameObject(clip.name, worldPosition);
        CreateAudioSource(clip, soundGameObject, options);
    }

    public void PlaySound(AudioClip clip, Transform parent, AudioClipOptions options = null) {
        if (!clip) {
            return;
        }

        GameObject soundGameObject = CreateSoundGameObject(clip.name, parent);
        CreateAudioSource(clip, soundGameObject, options);
    }

    private GameObject CreateSoundGameObject(string clipName, Vector3 worldPosition) {
        GameObject soundGameObject = new GameObject("Sound " + clipName);
        soundGameObject.transform.position = worldPosition;

        return soundGameObject;
    }

    private GameObject CreateSoundGameObject(string clipName, Transform parent) {
        GameObject soundGameObject = new GameObject("Sound " + clipName);
        soundGameObject.transform.SetParent(parent, false);

        return soundGameObject;
    }

    private void CreateAudioSource(AudioClip clip, GameObject audioObject, AudioClipOptions options) {
        AudioClipOptions audioOptions = options != null ? options : new AudioClipOptions();

        float pitch = audioOptions.RandomPitch ?
            Random.Range(audioOptions.Pitch - audioOptions.PitchRange, audioOptions.Pitch + audioOptions.PitchRange) :
            audioOptions.Pitch;

        AudioSource audioSource = audioObject.AddComponent<AudioSource>();
        audioSource.clip = clip;
        audioSource.maxDistance = 100f;
        audioSource.spatialBlend = 1f;
        audioSource.rolloffMode = AudioRolloffMode.Linear;
        audioSource.dopplerLevel = 0f;
        audioSource.outputAudioMixerGroup = sfxMixer;
        audioSource.pitch = pitch;
        audioSource.volume = audioOptions.Volume;
        audioSource.loop = audioOptions.Loop;

        audioSource.Play();

        Destroy(audioObject, audioSource.clip.length);
    }

    public void SetMasterVolume(float volume) {
        float MAX_VOLUME = 20f;
        float MIN_VOLUME = -50f;

        float newVolume = GetMixerVolume(volume / 100, MIN_VOLUME, MAX_VOLUME);

        if (newVolume == MIN_VOLUME) {
            newVolume = -80;
        }

        masterMixer.audioMixer.SetFloat("MasterVolume", newVolume);
    }

    public void SetMusicVolume(float volume) {
        float MAX_VOLUME = 20f;
        float MIN_VOLUME = -50f;

        float newVolume = GetMixerVolume(volume / 100, MIN_VOLUME, MAX_VOLUME);

        if (newVolume == MIN_VOLUME) {
            newVolume = -80;
        }

        musicMixer.audioMixer.SetFloat("MusicVolume", newVolume);
    }

    public void SetSfxVolume(float volume) {
        float MAX_VOLUME = 0f;
        float MIN_VOLUME = -50f;

        float newVolume = GetMixerVolume(volume / 100, MIN_VOLUME, MAX_VOLUME);

        if (newVolume == MIN_VOLUME) {
            newVolume = -80;
        }

        sfxMixer.audioMixer.SetFloat("SFXVolume", newVolume);
    }

    private float GetMixerVolume(float percent, float min, float max) {
        float VOLUME_RANGE = max - min;

        return min + (VOLUME_RANGE * percent);
    }
}