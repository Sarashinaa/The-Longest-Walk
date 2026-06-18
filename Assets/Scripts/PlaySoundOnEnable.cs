using UnityEngine;

public class PlaySoundOnEnable : MonoBehaviour
{
    [Tooltip("Suara apa yang mau diputar saat objek ini muncul?")]
    public AudioClip soundToPlay;

    private void OnEnable()
    {
        // Fungsi OnEnable otomatis berjalan tiap kali objek di-SetActive(true) oleh GameManager
        if (AudioManager.Instance != null && soundToPlay != null)
        {
            AudioManager.Instance.PlaySFX(soundToPlay);
        }
    }
}