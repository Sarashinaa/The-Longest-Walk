using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.EventSystems;

public class MainMenuController : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject menuPanel;
    public GameObject settingsPanel;
    public GameObject creditsPanel;

    [Header("Game Components")]
    public PlayerMovement playerMovement;
    public Slider volumeSlider;
    public VideoPlayer creditsVideoPlayer;

    [Header("Navigation Support")]
    public GameObject playButton;
    public GameObject settingsBackButton;

    void Start()
    {
        if (playerMovement != null) playerMovement.enabled = false;

        menuPanel.SetActive(true);
        settingsPanel.SetActive(false);
        creditsPanel.SetActive(false);

        EventSystem.current.SetSelectedGameObject(playButton);

        if (volumeSlider != null)
        {
            volumeSlider.value = AudioListener.volume;
            volumeSlider.onValueChanged.AddListener(SetMasterVolume);
        }

        if (creditsVideoPlayer != null)
        {
            creditsVideoPlayer.loopPointReached += EndCredits;
        }

        if (AudioManager.Instance != null && AudioManager.Instance.bgmMainMenu != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmMainMenu);
        }

        // --- SISTEM OTOMATIS PATH VIDEO WINDOWS & WEBGL ---
        if (creditsVideoPlayer != null)
        {
            // Ubah source menjadi URL agar WebGL bisa membaca
            creditsVideoPlayer.source = VideoSource.Url;
            
            // Menggabungkan path StreamingAssets dengan nama file video kamu
            string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "CreditsSceneNoBG.mp4");
            
            creditsVideoPlayer.url = videoPath;
        }

        if (AudioManager.Instance != null && AudioManager.Instance.bgmMainMenu != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmMainMenu);
        }
    }

    // --- FITUR BARU: Deteksi Tombol Escape ---
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Jika lagi buka Settings, tutup pakai fungsi OnClickCloseSettings
            if (settingsPanel.activeInHierarchy)
            {
                OnClickCloseSettings();
            }
            // Jika lagi nonton Credits, tutup paksa
            else if (creditsPanel.activeInHierarchy)
            {
                CloseCreditsWithSound();
            }
        }
    }

    // --- FUNGSI TOMBOL ---

    public void OnClickPlay()
    {
        PlayConfirmSound();
        menuPanel.SetActive(false);
        gameObject.SetActive(false); 
        
        // --- GANTI BAGIAN INI ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartGameplay(); // Memanggil fungsi dari GameManager
        }

        if (AudioManager.Instance != null && AudioManager.Instance.bgmInGame != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmInGame);
        }
    }

    public void OnClickSettings()
    {
        PlayConfirmSound();
        menuPanel.SetActive(false);
        settingsPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(settingsBackButton); 
    }

    public void OnClickCloseSettings()
    {
        PlayConfirmSound();
        settingsPanel.SetActive(false);
        menuPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(playButton); 
    }

    public void OnClickCredits()
    {
        PlayConfirmSound();
        menuPanel.SetActive(false);
        creditsPanel.SetActive(true);
        creditsVideoPlayer.Play();
    }

    // Fungsi dipanggil saat mencet ESC (Keluar paksa + SFX)
    private void CloseCreditsWithSound()
    {
        PlayConfirmSound();
        StopAndCloseCreditsPanel();
    }

    // Fungsi dipanggil otomatis saat video tamat (Tanpa SFX tambahan)
    private void EndCredits(VideoPlayer vp)
    {
        StopAndCloseCreditsPanel();
    }

    // Fungsi inti untuk menghentikan video dan balik ke menu
    private void StopAndCloseCreditsPanel()
    {
        if (creditsVideoPlayer != null && creditsVideoPlayer.isPlaying)
        {
            creditsVideoPlayer.Stop();
        }
        creditsPanel.SetActive(false);
        menuPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(playButton);
    }

   // saat balik ke Main Menu, keyboard otomatis milih tombol Play lagi
    // Fungsi ini dipanggil otomatis setiap kali MainMenuCanvas dinyalakan
    // --- UPDATE SCRIPT YANG SUDAH ADA ---
    private bool playCreditsDirectly = false; // Penanda apakah masuk dari ending

    private void OnEnable()
    {
        if (playCreditsDirectly)
        {
            // Jika baru tamat, langsung setel video credits dan sembunyikan menu utama
            if (menuPanel != null) menuPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(true);
            if (creditsVideoPlayer != null) creditsVideoPlayer.Play();
            
            playCreditsDirectly = false; // Reset kembali ke normal
        }
        else
        {
            // Kondisi normal saat game baru dibuka pertama kali atau setelah credits selesai
            if (menuPanel != null) menuPanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (creditsPanel != null) creditsPanel.SetActive(false);

            if (playButton != null && EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(playButton);
            }
        }
    }

    public void StartCreditsFromEnding()
    {
        playCreditsDirectly = true;
        gameObject.SetActive(true); // Menyalakan Canvas Main Menu
    }

    public void OnClickQuit()
    {
        PlayConfirmSound();
        Debug.Log("Keluar dari Game...");

        // Kode ajaib: Jika di editor matikan Play Mode, jika sudah di-build keluar dari aplikasi
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // --- FUNGSI UTILITAS ---

    private void SetMasterVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    private void PlayConfirmSound()
    {
        if (AudioManager.Instance != null && AudioManager.Instance.sfxConfirm != null)
        {
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxConfirm);
        }
    }
}