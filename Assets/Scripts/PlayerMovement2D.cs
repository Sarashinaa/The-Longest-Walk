using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isFacingRight = true;

    void Start()
    {
        // Mengambil referensi komponen yang ada di GameObject ini
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 1. Ambil Input (A/D atau Panah Kiri/Kanan)
        // GetAxisRaw membuat pergerakan lebih responsif (langsung 1 atau -1)
        float moveInput = Input.GetAxisRaw("Horizontal");

        // 2. Terapkan pergerakan ke Rigidbody2D
        // Kita pertahankan kecepatan Y agar gravitasi atau lompatan tetap berfungsi
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        // 3. Trigger Animasi Walk
        // Jika moveInput tidak sama dengan 0, berarti karakter sedang bergerak
        bool isWalking = moveInput != 0;
        animator.SetBool("isWalking", isWalking);

        // 4. Balik arah hadap karakter (Flip)
        if (moveInput > 0 && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput < 0 && isFacingRight)
        {
            Flip();
        }
    }

    // Method untuk membalikkan sumbu X pada karakter
    private void Flip()
    {
    isFacingRight = !isFacingRight;
    // Gunakan flipX bawaan SpriteRenderer, bukan mengubah localScale
    spriteRenderer.flipX = !isFacingRight;
    }
}