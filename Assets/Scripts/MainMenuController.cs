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

    [Tooltip("Slider pengatur volume di panel Settings")]
    public Slider volumeSlider;
    
    [Tooltip("Video Player yang ada di panel Credits")]
    public VideoPlayer creditsVideoPlayer;

    [Header("Navigation Support")]
    public GameObject playButton;
    public GameObject settingsBackButton;

    void Start()
    {
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
    }

    public void OnClickPlay()
    {
        PlayConfirmSound();
        menuPanel.SetActive(false);
        gameObject.SetActive(false); 
        
        // --- INI KUNCINYA: Memberi Lampu Hijau ke Seluruh Game ---
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isGameStarted = true;
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

    private void EndCredits(VideoPlayer vp)
    {
        creditsPanel.SetActive(false);
        menuPanel.SetActive(true);
        EventSystem.current.SetSelectedGameObject(playButton);
    }

    public void OnClickQuit()
    {
        PlayConfirmSound();
        Debug.Log("Keluar dari Game...");
        Application.Quit();
    }

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