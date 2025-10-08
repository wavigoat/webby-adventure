using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    public Animator animator;
    public PlayerAudio playerAudio;
    public PlayerAttack playerAttack;
    public GameManager gm;
    public GameSceneManager gsm;

    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 10f;
    public int extraJumps = 1;
    public bool isFlipped = false;
    public bool isMultiDirectional = false;

    [Header("Jump Check")]
    public Transform groundCheck;
    public float checkRadius = 0.2f;
    public LayerMask groundLayer;
    public LayerMask noJumpLayer;

    [Header("Control Flags")]
    public bool allowAudio = true;
    public bool isDisabled = false;

    private Rigidbody2D rb;
    private int jumpsLeft;
    private bool isGrounded;
    private bool onNoJump;
    private float lastDirection = 1;

    public void FreezePlayer()
    {
        rb.velocity = Vector2.zero;
        animator.SetBool("isMoving", false);
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        jumpsLeft = extraJumps;
    }

    void Update()
    {
        if (isDisabled) return;

        isGrounded = Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
        onNoJump = Physics2D.OverlapCircle(groundCheck.position, checkRadius, noJumpLayer);
        if (isGrounded || onNoJump) jumpsLeft = extraJumps;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        HandleJump();
        HandleAttack();
        HandleAnimation(moveX, moveY);
        HandleMovement(moveX, moveY);
        HandleOrientation(moveX);
        if (allowAudio) HandleAudio(moveX);
    }

    void HandleJump()
    {
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.velocity = Vector2.up * jumpForce;
            if (allowAudio) playerAudio?.JumpSource?.Play();
        }
        else if (Input.GetButtonDown("Jump") && jumpsLeft > 0 && !onNoJump)
        {
            rb.velocity = Vector2.up * jumpForce;
            jumpsLeft--;
            if (allowAudio) playerAudio?.JumpSource?.Play();
        }

        // Update isJumping status based on ground checks
        if (!isGrounded && !onNoJump)
        {
            animator.SetBool("isJumping", true);
        }
        else
        {
            animator.SetBool("isJumping", false);
            jumpsLeft = extraJumps;
        }
    }

    void HandleMovement(float x, float y)
    {
        if (isMultiDirectional)
        {
            Vector2 input = new Vector2(x, y).normalized;
            transform.Translate(moveSpeed * Time.deltaTime * input);
        }
        else
        {
            rb.velocity = new Vector2(x * moveSpeed, rb.velocity.y);
        }
    }

    void HandleOrientation(float x)
    {
        if (x != 0) lastDirection = x;
        if (x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    void HandleAnimation(float x, float y)
    {
        bool isMoving = x != 0 || y != 0;
        animator.SetBool("isMoving", isMoving);

        if (isMultiDirectional)
        {
            animator.SetFloat("MoveHorizontal", x);
            animator.SetFloat("MoveVertical", y);
            animator.SetFloat("MoveMagnitude", new Vector2(x, y).magnitude);
        }
    }

    void HandleAttack()
    {
        if (Input.GetMouseButton(0))
        {
            animator.SetBool("isAttacking", true);
            playerAttack?.Attack(transform.localScale);
        }
        else
        {
            animator.SetBool("isAttacking", false);
            playerAttack?.StopAttack();
        }
    }

    void HandleAudio(float x)
    {
        if (animator.GetBool("isMoving"))
        {
            if (!playerAudio.WalkSource.isPlaying)
                playerAudio.WalkSource.Play();
        }
        else
        {
            if (playerAudio.WalkSource.isPlaying)
                playerAudio.WalkSource.Stop();
        }

        if (animator.GetBool("isAttacking") && !playerAudio.AttackSource.isPlaying)
        {
            playerAudio.AttackSource.Play();
        }

        if (animator.GetBool("isDead") && !playerAudio.DeathSource.isPlaying)
        {
            playerAudio.DeathSource.Play();
        }
    }

    public void TimeToDie()
    {
        if (!animator.GetBool("isDead"))
            StartCoroutine(DeathRoutine());
    }

    IEnumerator DeathRoutine()
    {
        if (gm && gsm)
        {
            animator.SetBool("isDead", true);
            isDisabled = true;

            yield return new WaitForSeconds(1f);
            yield return StartCoroutine(gsm.FadeOut());

            gm.Respawn(gameObject);
            yield return StartCoroutine(gsm.FadeIn());

            GetComponent<PlayerHealth>()?.ResetHealth();
            isDisabled = false;
            animator.SetBool("isDead", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Platform") && rb.velocity.y < 0)
            transform.parent = col.transform;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Platform"))
            transform.parent = null;
    }
}
