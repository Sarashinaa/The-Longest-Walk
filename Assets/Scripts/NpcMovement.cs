using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator), typeof(AudioSource))]
public class NPCWalkRight : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f; 

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource; // Tambahan komponen Audio
    
    private Vector3 startPos; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>(); // Mengambil referensi AudioSource

        startPos = transform.position;
    }

    void OnEnable() 
    {
        transform.position = startPos;
        
        // --- LOGIKA AUDIO LANGKAH KAKI NPC ---
        // Play suara otomatis setiap kali NPC dimunculkan ke layar
        if (audioSource != null && !audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    void OnDisable()
    {
        // Matikan suara secara paksa saat NPC disembunyikan (saat pindah lantai)
        if (audioSource != null)
        {
            audioSource.Stop();
        }
    }

    void Start()
    {
        if (spriteRenderer != null) spriteRenderer.flipX = false;
    }

    void Update()
    {
        rb.velocity = new Vector2(moveSpeed, rb.velocity.y);
    }
}