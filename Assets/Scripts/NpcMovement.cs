using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class NPCWalkRight : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f; // Kecepatan dibuat sedikit lebih lambat dari player (opsional)

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        // Mengambil referensi komponen
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Karena NPC ini tidak punya idle dan selalu jalan, 
        // kita set animasi isWalking menjadi true sejak awal
        animator.SetBool("isWalking", true);

        // Pastikan sprite menghadap ke kanan (asumsi default sprite menghadap kanan = flipX false)
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = false;
        }
    }

    void Update()
    {
        // Terapkan pergerakan konstan ke sumbu X positif (kanan)
        // Kecepatan Y dipertahankan untuk gravitasi
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
    }
}