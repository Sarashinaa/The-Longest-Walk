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

    [Header("Visual Lantai Database")]
    public List<FloorVisual> floorVisuals;

    [Header("Anomalies Database")]
    public List<AnomalyData> anomalies;

    // Tracker untuk NPC per Cycle
    private bool hasNpcSpawnedThisCycle = false;
    private int npcGroupIndex = -1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // 1. Simpan posisi awal benda & Cari indeks NPC
        for (int i = 0; i < anomalies.Count; i++)
        {
            AnomalyData data = anomalies[i];
            
            if (data.isNPC) npcGroupIndex = i; // Simpan posisi NPC di database

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
    }

    public void CheckChoice(bool wentLeft)
    {
        if (isTransitioning) return;

        // --- LOGIKA LANTAI 1 (EXIT SIGN) ---
        if (currentLevel == 1)
        {
            if (wentLeft) 
            {
                Debug.Log("GAME CLEAR! Player berhasil keluar.");
                return; // Stop eksekusi agar tidak transisi
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

    // Fungsi khusus untuk mereset seluruh cycle pemain
    private void ResetCycle()
    {
        currentLevel = maxLevel;
        isFirstRoom = true; // Lantai 6 kembali jadi lantai hafalan normal
        hasNpcSpawnedThisCycle = false; // Reset Tracker NPC
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
        // Update PNG Lantai
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

        // LOGIKA BLOCKER: Jika lantai awal atau lantai exit, jadikan Trigger Kanan tembok
        if (triggerKananCollider != null)
        {
            if (currentLevel == maxLevel || currentLevel == 1)
                triggerKananCollider.isTrigger = false; // Tembok (Nge-block player)
            else
                triggerKananCollider.isTrigger = true;  // Trigger normal bisa ditembus
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
        if (isFirstRoom)
        {
            isAnomalyActive = false;
            consecutiveNormalCount++;
            isFirstRoom = false; 
        }
        else
        {
            // LOGIKA PAKSA NPC (Syarat: Lantai 2, Belum Muncul, dan NPC ada di database)
            if (currentLevel == 2 && !hasNpcSpawnedThisCycle && npcGroupIndex != -1)
            {
                isAnomalyActive = true;
                forceNpc = true;
                consecutiveNormalCount = 0;
                Debug.Log("System: Memaksa Anomali NPC (Lantai terakhir sebelum Exit)");
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
                if (data.normalObject != null) data.normalObject.SetActive(true);
            }
        }
        else
        {
            // Tentukan mau muncul berapa anomali? (Maksimal 3 atau sesuai total barang yang ada)
            int maxPossibleAnomalies = Mathf.Min(3, anomalies.Count); 
            int numToSpawn = Random.Range(1, maxPossibleAnomalies + 1);

            List<int> availableIndices = new List<int>();
            for (int i = 0; i < anomalies.Count; i++) availableIndices.Add(i);

            List<int> chosenIndices = new List<int>();

            // Jika dipaksa NPC, amankan slot NPC duluan
            if (forceNpc && npcGroupIndex != -1)
            {
                chosenIndices.Add(npcGroupIndex);
                availableIndices.Remove(npcGroupIndex);
                numToSpawn--;
            }

            // Pilih sisa grup anomali secara acak
            for (int i = 0; i < numToSpawn; i++)
            {
                if (availableIndices.Count == 0) break;
                int rnd = Random.Range(0, availableIndices.Count);
                chosenIndices.Add(availableIndices[rnd]);
                availableIndices.RemoveAt(rnd); // Hapus dari antrean biar nggak kepilih dobel
            }

            // Terapkan ke scene
            for (int i = 0; i < anomalies.Count; i++)
            {
                if (chosenIndices.Contains(i))
                {
                    // Pastikan varian tidak kosong untuk mencegah error
                    if (anomalies[i].anomalyVariants == null || anomalies[i].anomalyVariants.Count == 0)
                    {
                        if (anomalies[i].normalObject != null) anomalies[i].normalObject.SetActive(true);
                    }
                    else
                    {
                        int randomVarian = Random.Range(0, anomalies[i].anomalyVariants.Count);
                        if (anomalies[i].anomalyVariants[randomVarian] != null)
                            anomalies[i].anomalyVariants[randomVarian].SetActive(true);

                        // Tandai NPC sudah muncul
                        if (i == npcGroupIndex) hasNpcSpawnedThisCycle = true;
                    }
                }
                else
                {
                    // Barang yang tidak terpilih tetap normal
                    if (anomalies[i].normalObject != null)
                        anomalies[i].normalObject.SetActive(true);
                }
            }
        }
    }
}