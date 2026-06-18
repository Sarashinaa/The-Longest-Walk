using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("Audio Sources")]
    [Tooltip("Mesin pemutar untuk musik latar (BGM)")]
    public AudioSource bgmSource;
    [Tooltip("Mesin pemutar untuk efek suara sekali lewat (SFX)")]
    public AudioSource sfxSource;

    [Header("BGM Clips")]
    public AudioClip bgmMainMenu;
    public AudioClip bgmInGame;

    [Header("Global SFX Clips")]
    public AudioClip sfxGantiLantai;
    public AudioClip sfxMenu;
    public AudioClip sfxConfirm;
    public AudioClip sfxAnomaliTeriak;
    public AudioClip sfxAnomaliKetawa;

    private void Awake()
    {
        // Setup Singleton & Kebal Pindah Scene
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Objek ini tidak akan hancur saat ganti Scene
        }
        else
        {
            Destroy(gameObject); // Cegah ada 2 AudioManager tumpang tindih
        }
    }

    // Fungsi untuk memutar musik latar
    public void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;
        
        // Cek agar musik tidak mengulang dari awal jika BGM-nya sama
        if (bgmSource.clip == clip) return; 

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    // Fungsi untuk memutar efek suara (bisa bertumpuk tanpa memotong suara lain)
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip);
    }
}