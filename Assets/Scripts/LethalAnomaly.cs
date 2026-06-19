using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class LethalAnomaly : MonoBehaviour
{
    [Tooltip("Suara khusus saat NPC ini menyerang/menabrak player (Opsional)")]
    public AudioClip attackSound;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Mengecek apakah yang ditabrak memiliki Tag "Player"
        if (collision.CompareTag("Player"))
        {
            if (GameManager.Instance != null)
            {
                // Panggil rutinitas Jumpscare dari GameManager
                GameManager.Instance.TriggerJumpscareReset(attackSound);
            }
        }
    }
}