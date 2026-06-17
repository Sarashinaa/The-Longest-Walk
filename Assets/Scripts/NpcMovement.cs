using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class NPCWalkRight : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3f; 

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    
    // Variabel untuk mengingat posisi awal
    private Vector3 startPos; 

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Simpan posisi awal tepat saat game baru di-play
        startPos = transform.position;
    }

    // Fungsi ini akan dieksekusi otomatis setiap kali objek di-SetActive(true)
    void OnEnable() 
    {
        // Reset posisi ke awal agar tidak nerusin jalan dari level sebelumnya
        transform.position = startPos;
        
        if (animator != null) animator.SetBool("isWalking", true);
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