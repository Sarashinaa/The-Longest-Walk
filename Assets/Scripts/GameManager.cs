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
    [Tooltip("Masukkan BoxCollider2D dari TriggerKanan ke sini")]
    public Collider2D triggerKananCollider;

    [Header("Ending Settings")]
    [Tooltip("Masukkan MainMenuCanvas agar bisa dihidupkan lagi")]
    public GameObject mainMenuCanvas;
    [Tooltip("Masukkan objek Image/Teks Congratulation di sini")]
    public GameObject congratulationsImage;
    [Tooltip("Audio kemenangan yang diputar bersamaan dengan layar hitam")]
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
                    if (variant != null && !data.isNPC) 
                        data.anomalyStartPos.Add(variant.transform.position);
                    else 
                        data.anomalyStartPos.Add(Vector3.zero);
                }
            }
        }

        currentLevel = maxLevel;
        UpdateFloorVisual();
        RollAnomaly();

        if (AudioManager.Instance != null && AudioManager.Instance.bgmInGame != null)
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmInGame);
    }

    public void CheckChoice(bool wentLeft)
    {
        if (isTransitioning) return;

        // --- LOGIKA LANTAI 1 (EXIT SIGN) ---
        if (currentLevel == 1)
        {
            if (wentLeft) 
            {
                Debug.Log("GAME CLEAR! Memulai Sequence Ending...");
                StartCoroutine(EndingRoutine());
                return; 
            }
        }
        // --- LOGIKA NORMAL (Lantai 6 sampai 2) ---
        else
        {
            if (wentLeft) 
            {
                if (!isAnomalyActive) { currentLevel--; Debug.Log("BENAR! Maju."); } 
                else { ResetCycle(); Debug.Log("SALAH! Ada anomali. Reset ke awal!"); }
            }
            else 
            {
                if (isAnomalyActive) { currentLevel--; Debug.Log("BENAR! Mundur."); } 
                else { ResetCycle(); Debug.Log("SALAH! Bersih kok mundur? Reset ke awal!"); }
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
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        fadeScreen.alpha = 1f;

        if (AudioManager.Instance != null && AudioManager.Instance.sfxGantiLantai != null)
            AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxGantiLantai);

        yield return new WaitForSeconds(0.2f); 

        player.position = spawnPosition;
        UpdateFloorVisual(); 
        RollAnomaly();       

        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        fadeScreen.alpha = 0f;

        isTransitioning = false;
    }

    // --- COROUTINE KHUSUS ENDING ---
    private IEnumerator EndingRoutine()
    {
        isTransitioning = true;

        // 1. Matikan background music saat ini (Biar dramatis)
        if (AudioManager.Instance != null && AudioManager.Instance.bgmSource != null)
        {
            AudioManager.Instance.bgmSource.Stop();
        }

        // 2. Fade Out perlahan ke Layar Hitam Pekat
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        fadeScreen.alpha = 1f;

        // 3. Munculkan PNG "Congratulation" & Putar Audio
        if (congratulationsImage != null) congratulationsImage.SetActive(true);
        
        float waitTime = 4f; // Waktu tunggu bawaan jika tidak ada audio
        
        if (winAudio != null && AudioManager.Instance != null)
        {
            AudioManager.Instance.PlaySFX(winAudio);
            waitTime = winAudio.length; // Otomatis nunggu sesuai durasi lagu/suara
        }

        // 4. Jeda sistem selama durasi audio kemenangan berputar
        yield return new WaitForSeconds(waitTime + 0.5f); // Ekstra 0.5 detik biar gak terlalu mendadak

        // 5. Matikan PNG Congratulation
        if (congratulationsImage != null) congratulationsImage.SetActive(false);

        // 6. Reset Game State ke Lantai 6
        ResetCycle();
        player.position = spawnPosition;
        UpdateFloorVisual();
        RollAnomaly();

        // 7. Kembalikan kontrol ke Main Menu
        isGameStarted = false; // Lampu merah untuk Player dan NPC
        if (mainMenuCanvas != null) mainMenuCanvas.SetActive(true);
        
        // Terangkan layar kembali agar Main Menu terlihat
        fadeScreen.alpha = 0f;
        isTransitioning = false;

        // Putar ulang BGM Main Menu jika punya (Opsional)
        if (AudioManager.Instance != null && AudioManager.Instance.bgmMainMenu != null)
        {
            AudioManager.Instance.PlayBGM(AudioManager.Instance.bgmMainMenu);
        }
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

        if (triggerKananCollider != null)
        {
            if (currentLevel == maxLevel || currentLevel == 1)
                triggerKananCollider.isTrigger = false; 
            else
                triggerKananCollider.isTrigger = true;  
        }
    }

    private void RollAnomaly()
    {
        if (anomalies == null || anomalies.Count == 0) return;

        // 1. Matikan dan reset posisi semua barang
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

        // 2. Tentukan status anomali
        if (currentLevel == 1)
        {
            // LOGIKA BARU: Jika di Lantai 1 (Exit), larang keras ada anomali!
            isAnomalyActive = false;
        }
        else if (isFirstRoom)
        {
            isAnomalyActive = false;
            consecutiveNormalCount++;
            isFirstRoom = false; 
        }
        else
        {
            if (currentLevel == 2 && !hasNpcSpawnedThisCycle && npcGroupIndex != -1)
            {
                isAnomalyActive = true;
                forceNpc = true;
                consecutiveNormalCount = 0;
            }
            else if (consecutiveNormalCount >= 2)
            {
                isAnomalyActive = true;
                consecutiveNormalCount = 0; 
            }
            else
            {
                int chance = Random.Range(1, 101); 
                if (chance % 2 != 0) 
                {
                    isAnomalyActive = true;  
                    consecutiveNormalCount = 0; 
                }
                else 
                {
                    isAnomalyActive = false; 
                    consecutiveNormalCount++;   
                }
            }
        }

        // 3. Eksekusi Visual Anomali/Normal
        if (!isAnomalyActive)
        {
            foreach (AnomalyData data in anomalies)
            {
                // LOGIKA BARU: Cegah NPC normal muncul di Lantai 1
                if (currentLevel == 1 && data.isNPC) continue; 

                if (data.normalObject != null) data.normalObject.SetActive(true);
            }
        }
        else
        {
            int maxPossibleAnomalies = Mathf.Min(3, anomalies.Count); 
            int numToSpawn = Random.Range(1, maxPossibleAnomalies + 1);

            List<int> availableIndices = new List<int>();
            for (int i = 0; i < anomalies.Count; i++) availableIndices.Add(i);

            List<int> chosenIndices = new List<int>();

            if (forceNpc && npcGroupIndex != -1)
            {
                chosenIndices.Add(npcGroupIndex);
                availableIndices.Remove(npcGroupIndex);
                numToSpawn--;
            }

            for (int i = 0; i < numToSpawn; i++)
            {
                if (availableIndices.Count == 0) break;
                int rnd = Random.Range(0, availableIndices.Count);
                chosenIndices.Add(availableIndices[rnd]);
                availableIndices.RemoveAt(rnd); 
            }

            for (int i = 0; i < anomalies.Count; i++)
            {
                if (chosenIndices.Contains(i))
                {
                    if (anomalies[i].anomalyVariants == null || anomalies[i].anomalyVariants.Count == 0)
                    {
                        if (anomalies[i].normalObject != null) anomalies[i].normalObject.SetActive(true);
                    }
                    else
                    {
                        int randomVarian = Random.Range(0, anomalies[i].anomalyVariants.Count);
                        if (anomalies[i].anomalyVariants[randomVarian] != null)
                            anomalies[i].anomalyVariants[randomVarian].SetActive(true);

                        if (i == npcGroupIndex) hasNpcSpawnedThisCycle = true;
                    }
                }
                else
                {
                    if (anomalies[i].normalObject != null)
                        anomalies[i].normalObject.SetActive(true);
                }
            }
        }
    }

    // ==========================================
    //         --- JUMPSCARE & MATI ---
    // ==========================================
    
    public void TriggerJumpscareReset(AudioClip npcAttackAudio)
    {
        if (isTransitioning) return;
        StartCoroutine(JumpscareRoutine(npcAttackAudio));
    }

    private IEnumerator JumpscareRoutine(AudioClip npcAttackAudio)
    {
        isTransitioning = true;

        // 1. Putar Audio Teriak (Player) dan Audio Serangan (NPC) berbarengan
        if (AudioManager.Instance != null)
        {
            if (AudioManager.Instance.sfxAnomaliTeriak != null)
                AudioManager.Instance.PlaySFX(AudioManager.Instance.sfxAnomaliTeriak);
                
            if (npcAttackAudio != null)
                AudioManager.Instance.PlaySFX(npcAttackAudio);
        }

        // 2. FADE OUT CEPAT (3x lebih cepat dari transisi pintu normal)
        float fastFadeDuration = fadeDuration / 3f;
        float timer = 0f;
        while (timer < fastFadeDuration)
        {
            timer += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(0f, 1f, timer / fastFadeDuration);
            yield return null;
        }
        fadeScreen.alpha = 1f;

        // 3. Jeda dramatis di layar hitam pekat sambil dengerin sisa suara
        yield return new WaitForSeconds(1.5f);

        // 4. RESET GAME KEMBALI KE LANTAI 6
        Debug.Log("Player Mati Tertabrak Anomali! Reset ke awal.");
        ResetCycle();
        player.position = spawnPosition;
        UpdateFloorVisual();
        RollAnomaly();

        // 5. FADE IN (Layar kembali terang dengan kecepatan normal)
        timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);
            yield return null;
        }
        fadeScreen.alpha = 0f;

        isTransitioning = false;
    }
}