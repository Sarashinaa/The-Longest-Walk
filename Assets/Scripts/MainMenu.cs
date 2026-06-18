using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    public void StartGame()
    {
        // Load the main game scene
        UnityEngine.SceneManagement.SceneManager.LoadScene("Safe Floor");
    }

    public void QuitGame()
    {
        // Jika dijalankan di dalam Unity Editor, stop Play Mode
    #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
    #else
        // Jika sudah jadi game asli (.exe), tutup aplikasinya
        Application.Quit();
    #endif
    }
}
