using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // Jika nanti kamu pakai TextMeshPro untuk UI Lantai

// Struktur untuk menyimpan pasangan objek normal dan anomali
[System.Serializable]
public class AnomalyPair
{
    public string namaBarang; // Hanya untuk penamaan di Inspector agar rapi
    public GameObject normalObject;
    public GameObject anomalyObject;
}

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Player Settings")]
    public Transform player;
    private Vector3 spawnPosition = new Vector3(3.5f, -0.77f, 0f);

    [Header("Level Settings")]
    public int currentLevel = 6;
    private bool isAnomalyActive = false;
    private bool isTransitioning = false; // Mencegah player memicu trigger dua kali berturut-turut

    [Header("UI Settings")]
    public CanvasGroup fadeScreen;
    public float fadeDuration = 1f;

    [Header("Anomalies Database")]
    public List<AnomalyPair> anomalies;

    private void Awake()
    {
        // Setup Singleton agar mudah dipanggil dari script lain
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        // Mulai game dengan mengacak lorong pertama
        RollAnomaly();
    }

    // Fungsi utama pengecekan logika salah/benar
    public void CheckChoice(bool wentLeft)
    {
        if (isTransitioning) return;

        // Kiri = Maju (Harusnya TIDAK ada anomali)
        if (wentLeft) 
        {
            if (!isAnomalyActive) 
            {
                currentLevel--; // Benar, turun lantai
                Debug.Log("BENAR! Tidak ada anomali, maju ke lantai " + currentLevel);
            } 
            else 
            {
                currentLevel = 6; // Salah
                Debug.Log("SALAH! Ada anomali tapi kamu nekat maju. Reset ke lantai 6.");
            }
        }
        // Kanan = Mundur (Harusnya ADA anomali)
        else 
        {
            if (isAnomalyActive) 
            {
                currentLevel--; // Benar, turun lantai
                Debug.Log("BENAR! Ada anomali, kamu mundur ke lantai " + currentLevel);
            } 
            else 
            {
                currentLevel = 6; // Salah
                Debug.Log("SALAH! Lorong aman tapi kamu paranoid mundur. Reset ke lantai 6.");
            }
        }

        // Cek kondisi menang
        if (currentLevel < 1)
        {
            Debug.Log("GAME CLEAR! Pemain Bangun dari Mimpi.");
            // Nanti di sini bisa kamu tambahkan logika memuat Scene baru (Ending)
            currentLevel = 6; // Sementara kita reset saja
        }

        StartCoroutine(TransitionRoutine());
    }

    private IEnumerator TransitionRoutine()
    {
        isTransitioning = true;

        // FADE OUT (Layar menjadi hitam pelan-pelan)
        float timer = 0f;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            fadeScreen.alpha = Mathf.Lerp(0f, 1f, timer / fadeDuration);
            yield return null;
        }
        fadeScreen.alpha = 1f;

        // JEDA SEBENTAR SAAT GELAP
        yield return new WaitForSeconds(0.5f);

        // TELEPORT PLAYER & ACAK ANOMALI BARU SAAT LAYAR GELAP
        player.position = spawnPosition;
        RollAnomaly();

        // FADE IN (Layar kembali terang)
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

    private void RollAnomaly()
    {
        // 1. Reset semua barang menjadi Normal dulu
        foreach (AnomalyPair pair in anomalies)
        {
            if (pair.normalObject != null) pair.normalObject.SetActive(true);
            if (pair.anomalyObject != null) pair.anomalyObject.SetActive(false);
        }

        // 2. Acak angka 0 sampai 100
        int chance = Random.Range(0, 100);

        // 50% kemungkinan muncul anomali, 50% normal
        if (chance >= 50) 
        {
            isAnomalyActive = true;
            
            // Pilih satu anomali secara acak dari List
            int randomIndex = Random.Range(0, anomalies.Count);
            
            if (anomalies[randomIndex].normalObject != null) 
                anomalies[randomIndex].normalObject.SetActive(false);
                
            if (anomalies[randomIndex].anomalyObject != null) 
                anomalies[randomIndex].anomalyObject.SetActive(true);
                
            Debug.Log("System: Memunculkan Anomali " + anomalies[randomIndex].namaBarang);
        }
        else
        {
            isAnomalyActive = false;
            Debug.Log("System: Lorong Normal");
        }
    }
}