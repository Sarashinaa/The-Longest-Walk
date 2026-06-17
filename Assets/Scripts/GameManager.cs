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

    [Header("Player & Trigger Settings")]
    public Transform player;
    public Vector3 spawnPosition = new Vector3(3.5f, -0.77f, 0f);
    [Tooltip("Masukkan komponen BoxCollider2D dari TriggerKanan ke sini")]
    public Collider2D triggerKananCollider;

    [Header("Level Settings")]
    public int maxLevel = 6;
    public int currentLevel = 6;
    
    private bool isAnomalyActive = false;
    private bool isTransitioning = false;
    private bool isFirstRoom = true; 
    private int consecutiveNormalCount = 0; 

    // Variabel Cycle untuk jaminan NPC
    private bool hasNPCAppearedThisCycle = false;
    private int guaranteedNPCLevel = 0;

    [Header("UI Settings")]
    public CanvasGroup fadeScreen;
    public float fadeDuration = 1f;

    [Header("Visual Lantai Database")]
    public List<FloorVisual> floorVisuals;

    [Header("Anomalies Database")]
    public List<AnomalyData> anomalies;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        foreach (AnomalyData data in anomalies)
        {
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

        ResetCycle(); // Mulai cycle pertama
        UpdateFloorVisual();
        RollAnomaly();
    }

    // Fungsi untuk mereset semua kondisi cycle saat mulai atau saat player salah
    private void ResetCycle()
    {
        currentLevel = maxLevel;
        isFirstRoom = true;
        consecutiveNormalCount = 0;
        hasNPCAppearedThisCycle = false;
        
        // Acak di lantai berapa NPC WAJIB muncul (antara lantai maxLevel-1 sampai lantai 2)
        guaranteedNPCLevel = Random.Range(2, maxLevel); 
        Debug.Log("System: Cycle Baru dimulai. NPC dijamin akan muncul di Lantai " + guaranteedNPCLevel);
    }

    public void CheckChoice(bool wentLeft)
    {
        if (isTransitioning) return;

        if (currentLevel == 1)
        {
            if (wentLeft) 
            {
                Debug.Log("GAME CLEAR! Player berhasil keluar. Bersiap ke Main Menu...");
                // UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
                return; 
            }
            else 
            {
                Debug.Log("SALAH! Sudah di pintu keluar kok malah putar balik? Reset!");
                ResetCycle();
            }
        }
        else
        {
            if (wentLeft) 
            {
                if (!isAnomalyActive) { currentLevel--; Debug.Log("BENAR! Maju."); } 
                else { Debug.Log("SALAH! Ada anomali. Reset!"); ResetCycle(); }
            }
            else 
            {
                if (isAnomalyActive) { currentLevel--; Debug.Log("BENAR! Mundur."); } 
                else { Debug.Log("SALAH! Bersih kok mundur? Reset!"); ResetCycle(); }
            }
        }

        StartCoroutine(TransitionRoutine());
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

    private void UpdateFloorVisual()
    {
        // Atur Visual Background & Angka Lantai
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

        // Atur Blokir Tangga Kanan (IsTrigger = false membuat objek menjadi tembok)
        if (triggerKananCollider != null)
        {
            if (currentLevel == maxLevel || currentLevel == 1)
            {
                triggerKananCollider.isTrigger = false; 
            }
            else
            {
                triggerKananCollider.isTrigger = true; 
            }
        }
    }

    private void RollAnomaly()
    {
        if (anomalies == null || anomalies.Count == 0) return;

        // Cari tahu di Index mana letak NPC berada
        int npcIndex = -1;
        for (int i = 0; i < anomalies.Count; i++)
        {
            if (anomalies[i].isNPC && anomalies[i].anomalyVariants != null && anomalies[i].anomalyVariants.Count > 0)
            {
                npcIndex = i;
                break;
            }
        }

        // 1. Matikan objek dan kembalikan posisinya ke titik semula
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

        // 2. Tentukan logika anomali
        bool forceNPC = false;

        // Jika ini lantai jaminan NPC dan NPC belum pernah muncul
        if (currentLevel == guaranteedNPCLevel && !hasNPCAppearedThisCycle && npcIndex != -1)
        {
            forceNPC = true;
        }

        if (isFirstRoom)
        {
            isAnomalyActive = false;
            consecutiveNormalCount++;
            isFirstRoom = false; 
        }
        else if (forceNPC)
        {
            isAnomalyActive = true;
            consecutiveNormalCount = 0;
            Debug.Log("System: MEMAKSA NPC Anomali muncul sesuai jaminan cycle!");
        }
        else if (consecutiveNormalCount >= 2)
        {
            isAnomalyActive = true;
            consecutiveNormalCount = 0; 
            Debug.Log("System: Memaksa Anomali (Mencegah 3x Normal Beruntun)");
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

        // 3. Nyalakan objek sesuai hasil kocokan
        if (!isAnomalyActive)
        {
            foreach (AnomalyData data in anomalies)
            {
                if (data.normalObject != null) data.normalObject.SetActive(true);
            }
        }
        else
        {
            int randomGrup;

            if (forceNPC) 
            {
                randomGrup = npcIndex;
                hasNPCAppearedThisCycle = true;
            }
            else 
            {
                randomGrup = Random.Range(0, anomalies.Count);
                // Jika hasil acak kebetulan memilih NPC secara natural, catat agar jaminan tidak aktif ganda
                if (randomGrup == npcIndex) hasNPCAppearedThisCycle = true; 
            }
            
            if (anomalies[randomGrup].anomalyVariants == null || anomalies[randomGrup].anomalyVariants.Count == 0)
            {
                foreach (AnomalyData data in anomalies)
                {
                    if (data.normalObject != null) data.normalObject.SetActive(true);
                }
                return;
            }

            int randomVarian = Random.Range(0, anomalies[randomGrup].anomalyVariants.Count);

            for (int i = 0; i < anomalies.Count; i++)
            {
                if (i == randomGrup)
                {
                    if (anomalies[i].anomalyVariants[randomVarian] != null)
                        anomalies[i].anomalyVariants[randomVarian].SetActive(true);
                }
                else
                {
                    if (anomalies[i].normalObject != null)
                        anomalies[i].normalObject.SetActive(true);
                }
            }
        }
    }
}