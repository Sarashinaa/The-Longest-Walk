using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AnomalyData
{
    public string namaBarang;
    public GameObject normalObject;
    public List<GameObject> anomalyVariants; 
    
    [Header("NPC Settings")]
    public bool isNPC; 
    public Vector3 npcSpawnPoint = new Vector3(-9.9f, -0.77f, 0f);

    [HideInInspector] public Vector3 normalStartPos;
    [HideInInspector] public List<Vector3> anomalyStartPos;
}

[System.Serializable]
public class FloorVisual
{
    public int levelLantai; 
    public GameObject indikatorLantai; 
    public GameObject backgroundLantai; 
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isGameStarted = false;
    [HideInInspector] public bool isPaused = false; // Status Pause

    [Header("Player Settings")]
    public Transform player;
    public Vector3 spawnPosition = new Vector3(3.5f, -0.77f, 0f);

    [Header("Level Settings")]
    public int maxLevel = 6;
    public int currentLevel = 6;
    
    private bool isAnomalyActive = false;
    private bool isTransitioning = false;
    private bool isFirstRoom = true; 
    private int consecutiveNormalCount = 0; 

    [Header("UI & Blocker Settings")]
    public CanvasGroup fadeScreen;
    public float fadeDuration = 1f;
    public Collider2D triggerKananCollider;

    [Header("Pause Settings")]
    public GameObject pauseButtonUI;
    public GameObject pauseMenuPanel;

    [Header("Ending Settings")]
    public GameObject mainMenuCanvas;
    public GameObject congratulationsImage;
    public AudioClip winAudio;

    [Header("Visual Lantai Database")]
    public List<FloorVisual> floorVisuals;

    [Header("Anomalies Database")]
    public List<AnomalyData> anomalies;

    private bool hasNpcSpawnedThisCycle = false;
    private int npcGroupIndex = -1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        for (int i = 0; i < anomalies.Count; i++)
        {
            AnomalyData data = anomalies[i];
            if (data.isNPC) npcGroupIndex = i; 

            if (data.normalObject != null && !data.isNPC) 
                data.normalStartPos = data.normalObject.transform.position;
            
            data.anomalyStartPos = new List<Vector3>();
            if (data.anomalyVariants != null)
            {
                foreach (GameObject variant in data.anomalyVariants)
                {
                    if (variant != null && !data.isNPC) data.anomalyStartPos.Add(variant.transform.position);
                    else data.anomalyStartPos.Add(Vector3.zero);
                }
            }
        }

        currentLevel = maxLevel;
        UpdateFloorVisual();
        RollAnomaly();
    }

    private void Update()
    {
        // Deteksi tombol ESC untuk Pause/Resume (Hanya saat game main & tidak sedang transisi)
        if (isGameStarted && !isTransitioning)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }
    }

    // ==========================================
    // --- FITUR PAUSE MENU ---
    // ==========================================

    public void StartGameplay()
    {
        isGameStarted = true;
        if (pauseButtonUI != null) pauseButtonUI.SetActive(true); // Munculkan tombol pause
    }

    public void TogglePause()
    {
        if (isPaused) ResumeGame();
        else PauseGame();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Bekukan waktu Unity
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (pauseButtonUI != null) pauseButtonUI.SetActive(false); // Sembunyikan tombol UI saat menu terbuka

        if (AudioManager.Instance != null && AudioManager.Instance.sfxMenu != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxMenu);
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Jalankan waktu kembali
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (pauseButtonUI != null) pauseButtonUI.SetActive(true);

        if (AudioManager.Instance != null && AudioManager.Instance.sfxConfirm != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxConfirm);
    }

    public void QuitToMainMenuFromPause()
    {
        // 1. Reset Waktu & UI Pause
        isPaused = false;
        Time.timeScale = 1f; 
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (pauseButtonUI != null) pauseButtonUI.SetActive(false);

        // 2. Putar Suara Konfirmasi
        if (AudioManager.Instance != null && AudioManager.Instance.sfxConfirm != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxConfirm);

        // 3. Reset Game State ke awal
        ResetCycle();
        player.position = spawnPosition;
        UpdateFloorVisual();
        RollAnomaly();

        // 4. Matikan game & nyalakan Main Menu
        isGameStarted = false;
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);

        // 5. Ganti BGM balik ke Main Menu
        if (AudioManager.Instance != null && AudioManager.Instance.bgmMainMenu != null)
        {
            AudioManager.Instance.bgmSource.Stop();
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmMainMenu);
        }
    }

    // ==========================================
    // --- SISA KODE (TIDAK BERUBAH BANYAK) ---
    // ==========================================

    public void CheckChoice(bool wentLeft)
    {
        if (isTransitioning) return;

        if (currentLevel == 1)
        {
            if (wentLeft) 
            {
                StartCoroutine(EndingRoutine());
                return; 
            }
        }
        else
        {
            if (wentLeft) 
            {
                if (!isAnomalyActive) currentLevel--; 
                else ResetCycle(); 
            }
            else 
            {
                if (isAnomalyActive) currentLevel--; 
                else ResetCycle(); 
            }
        }

        StartCoroutine(TransitionRoutine());
    }

    private void ResetCycle()
    {
        currentLevel = maxLevel;
        isFirstRoom = true; 
        hasNpcSpawnedThisCycle = false; 
        consecutiveNormalCount = 0;
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;

        float timer = 0f;
        while (timer < fadeDuration) { timer += Time.deltaTime; fadeScreen.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration); yield return null; }
        fadeScreen.alpha = 1f;

        if (AudioManager.Instance != null && AudioManager.Instance.sfxGantiLantai != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxGantiLantai);

        yield return new WaitForSeconds(0.2f); 

        player.position = spawnPosition;
        UpdateFloorVisual(); RollAnomaly();       

        timer = 0f;
        while (timer < fadeDuration) { timer += Time.deltaTime; fadeScreen.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration); yield return null; }
        fadeScreen.alpha = 0f;

        isTransitioning = false;
    }

    private IEnumerator EndingRoutine()
    {
        isTransitioning = true;
        if (AudioManager.Instance != null && AudioManager.Instance.bgmSource != null) AudioManager.Instance.bgmSource.Stop();

        float timer = 0f;
        while (timer < fadeDuration) { timer += Time.deltaTime; fadeScreen.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration); yield return null; }
        fadeScreen.alpha = 1f;

        if (congratulationsImage != null) congratulationsImage.SetActive(true);
        
        float waitTime = 4f; 
        if (winAudio != null && AudioManager.Instance != null) { AudioManager.Instance.PlaySFX(winAudio); waitTime = winAudio.length; }

        yield return new WaitForSeconds(waitTime + 0.5f); 

        if (congratulationsImage != null) congratulationsImage.SetActive(false);

        ResetCycle();
        player.position = spawnPosition;
        UpdateFloorVisual();
        RollAnomaly();

        isGameStarted = false; 
        if (pauseButtonUI != null) pauseButtonUI.SetActive(false); // Sembunyikan pause di ending
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        
        fadeScreen.alpha = 0f;
        isTransitioning = false;

        if (AudioManager.Instance != null && AudioManager.Instance.bgmMainMenu != null) AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmMainMenu);
    }

    public void TriggerJumpscareReset(AudioClip npcAttackAudio)
    {
        if (isTransitioning) return;
        StartCoroutine(JumpscareRoutine(npcAttackAudio));
    }

    private IEnumerator JumpscareRoutine(AudioClip npcAttackAudio)
    {
        isTransitioning = true;
        if (AudioManager.Instance != null)
        {
            if (AudioManager.Instance.sfxAnomaliTeriak != null) AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAnomaliTeriak);
            if (npcAttackAudio != null) AudioManager.Instance.PlaySFX(npcAttackAudio);
        }

        float fastFadeDuration = fadeDuration / 3f;
        float timer = 0f;
        while (timer < fastFadeDuration) { timer += Time.deltaTime; fadeScreen.alpha = Mathf.Lerp(0f, 1f, timer / fastFadeDuration); yield return null; }
        fadeScreen.alpha = 1f;

        yield return new WaitForSeconds(1.5f);

        ResetCycle();
        player.position = spawnPosition;
        UpdateFloorVisual();
        RollAnomaly();

        timer = 0f;
        while (timer < fadeDuration) { timer += Time.deltaTime; fadeScreen.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration); yield return null; }
        fadeScreen.alpha = 0f;

        isTransitioning = false;
    }

    private void UpdateFloorVisual()
    {
        foreach (FloorVisual floor in floorVisuals)
        {
            if (floor.indikatorLantai != null) floor.indikatorLantai.SetActive(false);
            if (floor.backgroundLantai != null) floor.backgroundLantai.SetActive(false);
            if (floor.levelLantai == currentLevel)
            {
                if (floor.indikatorLantai != null) floor.indikatorLantai.SetActive(true);
                if (floor.backgroundLantai != null) floor.backgroundLantai.SetActive(true);
            }
        }
        if (triggerKananCollider != null) triggerKananCollider.isTrigger = (currentLevel != maxLevel && currentLevel != 1);
    }

    private void RollAnomaly()
    {
        if (anomalies == null || anomalies.Count == 0) return;
        foreach (AnomalyData data in anomalies)
        {
            if (data.normalObject != null)
            {
                data.normalObject.SetActive(false);
                if (data.isNPC) data.normalObject.transform.position = data.npcSpawnPoint;
                else if (data.normalStartPos != Vector3.zero) data.normalObject.transform.position = data.normalStartPos;
            }
            if (data.anomalyVariants != null && data.anomalyStartPos != null)
            {
                for (int i = 0; i < data.anomalyVariants.Count; i++)
                {
                    if (data.anomalyVariants[i] != null)
                    {
                        data.anomalyVariants[i].SetActive(false);
                        if (data.isNPC) data.anomalyVariants[i].transform.position = data.npcSpawnPoint;
                        else if (i < data.anomalyStartPos.Count) data.anomalyVariants[i].transform.position = data.anomalyStartPos[i];
                    }
                }
            }
        }

        bool forceNpc = false;
        if (currentLevel == 1) { isAnomalyActive = false; }
        else if (isFirstRoom) { isAnomalyActive = false; consecutiveNormalCount++; isFirstRoom = false; }
        else
        {
            if (currentLevel == 2 && !hasNpcSpawnedThisCycle && npcGroupIndex != -1) { isAnomalyActive = true; forceNpc = true; consecutiveNormalCount = 0; }
            else if (consecutiveNormalCount >= 2) { isAnomalyActive = true; consecutiveNormalCount = 0; }
            else { int chance = Random.Range(1, 101); if (chance % 2 != 0) { isAnomalyActive = true; consecutiveNormalCount = 0; } else { isAnomalyActive = false; consecutiveNormalCount++; } }
        }

        if (!isAnomalyActive)
        {
            foreach (AnomalyData data in anomalies)
            {
                if (currentLevel == 1 && data.isNPC) continue; 
                if (data.normalObject != null) data.normalObject.SetActive(true);
            }
        }
        else
        {
            int maxPossibleAnomalies = Mathf.Min(3, anomalies.Count); 
            int numToSpawn = Random.Range(1, maxPossibleAnomalies + 1);
            List<int> availableIndices = new List<int>(); for (int i = 0; i < anomalies.Count; i++) availableIndices.Add(i);
            List<int> chosenIndices = new List<int>();

            if (forceNpc && npcGroupIndex != -1) { chosenIndices.Add(npcGroupIndex); availableIndices.Remove(npcGroupIndex); numToSpawn--; }
            for (int i = 0; i < numToSpawn; i++) { if (availableIndices.Count == 0) break; int rnd = Random.Range(0, availableIndices.Count); chosenIndices.Add(availableIndices[rnd]); availableIndices.RemoveAt(rnd); }

            for (int i = 0; i < anomalies.Count; i++)
            {
                if (chosenIndices.Contains(i))
                {
                    if (anomalies[i].anomalyVariants == null || anomalies[i].anomalyVariants.Count == 0) { if (anomalies[i].normalObject != null) anomalies[i].normalObject.SetActive(true); }
                    else
                    {
                        int randomVarian = Random.Range(0, anomalies[i].anomalyVariants.Count);
                        if (anomalies[i].anomalyVariants[randomVarian] != null) anomalies[i].anomalyVariants[randomVarian].SetActive(true);
                        if (i == npcGroupIndex) hasNpcSpawnedThisCycle = true;
                    }
                }
                else { if (anomalies[i].normalObject != null) anomalies[i].normalObject.SetActive(true); }
            }
        }
    }
}