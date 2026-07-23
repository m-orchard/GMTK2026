using UnityEngine;

public class GameStartSound : MonoBehaviour
{
    [SerializeField] private AudioClip startSfx;

    [Tooltip("Volume for the start sound. Above 1 amplifies past the clip's recorded level.")]
    [SerializeField, Min(0f)] private float startSfxVolume = 2f;

    private void Start()
    {
        if (GameSession.Instance == null)
        {
            PlayStartSound();
            return;
        }

        GameSession.Instance.OnGameStarted += PlayStartSound;

        if (GameSession.Instance.HasStarted)
        {
            PlayStartSound();
        }
    }

    private void OnDestroy()
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.OnGameStarted -= PlayStartSound;
        }
    }

    private void PlayStartSound()
    {
        AudioManager.Instance.PlaySound(startSfx, new AudioClipOptions { Volume = startSfxVolume });
    }
}
