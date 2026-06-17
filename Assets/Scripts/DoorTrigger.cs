using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    [Tooltip("Centang jika ini adalah trigger di ujung kiri (Maju/Turun Tangga)")]
    public bool isKiriMaju;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Memastikan hanya tereksekusi jika yang menabrak memiliki Tag "Player"
        if (collision.CompareTag("Player"))
        {
            // Panggil wasit untuk mengecek keputusan
            GameManager.Instance.CheckChoice(isKiriMaju);
        }
    }
}