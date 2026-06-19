using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(AudioSource))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource; 
    private bool isFacingRight = true;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>(); 
    }

    void Update()
    {
        // --- LOGIKA "LAMPU MERAH" & PAUSE MENU ---
        if (GameManager.Instance != null && (!GameManager.Instance.isGameStarted || GameManager.Instance.isPaused))
        {
            animator.SetBool("isWalking", false);
            rb.velocity = new Vector2(0, rb.velocity.y); 
            if (audioSource.isPlaying) audioSource.Stop();
            return; 
        }

        // --- LOGIKA NORMAL (Saat Game Berjalan) ---
        float moveInput = Input.GetAxisRaw("Horizontal");
        rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

        bool isWalking = moveInput != 0;
        animator.SetBool("isWalking", isWalking);

        if (isWalking)
        {
            if (!audioSource.isPlaying) audioSource.Play();
        }
        else
        {
            if (audioSource.isPlaying) audioSource.Stop();
        }

        if (moveInput > 0 && !isFacingRight) Flip();
        else if (moveInput < 0 && isFacingRight) Flip();
    }

    private void Flip()
    {
        isFacingRight = !isFacingRight;
        spriteRenderer.flipX = !isFacingRight;
    }
}