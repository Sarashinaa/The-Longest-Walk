using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(AudioSource))]
public class NPCWalkRight : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f; 

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource; 
    private Vector3 startPos; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>(); 
        startPos = transform.position;
    }

    void OnEnable() 
    {
        transform.position = startPos;
    }

    void OnDisable()
    {
        if (audioSource != null) audioSource.Stop();
    }

    void Start()
    {
        if (spriteRenderer != null) spriteRenderer.flipX = false;
    }

    void Update()
    {
        // --- LOGIKA "LAMPU MERAH" & PAUSE MENU ---
        if (GameManager.Instance != null && (!GameManager.Instance.isGameStarted || GameManager.Instance.isPaused))
        {
            if (animator != null) animator.SetBool("isWalking", false);
            rb.velocity = new Vector2(0, rb.velocity.y); 
            if (audioSource != null && audioSource.isPlaying) audioSource.Stop();
            return;
        }

        // --- SAAT GAME BERJALAN ---
        if (animator != null) animator.SetBool("isWalking", true);
        if (audioSource != null && !audioSource.isPlaying) audioSource.Play();
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
    }
}