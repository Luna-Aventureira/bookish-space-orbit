using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;


public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float speed = 5f;
    public float laneSpeed = 5f;
    public float jumpLength = 0.6f;
    public float jumpHeight = 1.5f;
    public float laneWidth = 2f;
    public float minSpeed = 10f;
    public float maxSpeed = 30f;

    [Header("Health Settings")]
    public int maxLife = 3;
    public float invincibleTime = 2f;
    public float hitAnimationDuration = 0.5f;

    [Header("References")]
    public GameObject model;
    public UIManager uiManager;

    // Components
    private Animator anim;
    private Rigidbody rb;

    // State
    private Vector3 verticalTargetPosition;
    private bool jumping = false;
    private float jumpTimeCounter;
    private bool isHoldingJump = false;
    private int currentLife;
    private bool invincible = false;
    private int currentLane = 1; // 0: left, 1: center, 2: right
    private bool canMove = true;
    private bool isTakingDamage = false;
    
    // Properties
    public bool IsDead { get; private set; }
    public int DamageCount { get; private set; }
    public int GetCurrentLife() => currentLife;
    public int GetCurrentLane() => currentLane;

    void Start()
    {
        InitializeComponents();
        ResetPlayerState();
    }

    void InitializeComponents()
    {
        rb = GetComponent<Rigidbody>();
        if (model != null)
        {
            anim = model.GetComponent<Animator>();
        }
        
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

    void ResetPlayerState()
    {
        verticalTargetPosition = transform.position;
        currentLife = maxLife;
        DamageCount = 0;
        IsDead = false;
        canMove = true;
        
        if (anim != null)
        {
            ForcePlayAnimation("runStart");
        }
    }

    void Update()
    {
        if (!canMove || IsDead || isTakingDamage) return;

        HandleInput();
        HandleJump();
        UpdatePosition();
    }

    void FixedUpdate()
    {
        if (!IsDead && canMove && !isTakingDamage)
        {
            rb.velocity = Vector3.forward * speed;
        }
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow)) ChangeLane(-1);
        else if (Input.GetKeyDown(KeyCode.RightArrow)) ChangeLane(1);

        if (Input.GetKeyDown(KeyCode.Space) && !jumping) StartJump();
        if (Input.GetKeyUp(KeyCode.Space)) isHoldingJump = false;
    }

    void ChangeLane(int direction)
    {
        if (IsDead || !canMove || isTakingDamage) return;

        int targetLane = currentLane + direction;
        if (targetLane < 0 || targetLane > 2) return;

        currentLane = targetLane;
    }

    void StartJump()
    {
        if (!jumping && !IsDead && !isTakingDamage)
        {
            jumping = true;
            isHoldingJump = true;
            jumpTimeCounter = 0f;
            
            if (anim != null)
            {
                anim.SetBool("Jumping", true);
                anim.SetFloat("JumpSpeed", speed / jumpLength);
            }
        }
    }

    void HandleJump()
    {
        if (jumping)
        {
            jumpTimeCounter += Time.deltaTime;
            float ratio = jumpTimeCounter / jumpLength;

            if (ratio >= 1f || !isHoldingJump)
            {
                jumping = false;
                if (anim != null) anim.SetBool("Jumping", false);
            }
            else
            {
                verticalTargetPosition.y = Mathf.Sin(ratio * Mathf.PI) * jumpHeight;
            }
        }
        else
        {
            verticalTargetPosition.y = Mathf.MoveTowards(verticalTargetPosition.y, 0, 5 * Time.deltaTime);
        }
    }

    void UpdatePosition()
    {
        Vector3 newPosition = transform.position;
        newPosition.x = Mathf.Lerp(newPosition.x, (currentLane - 1) * laneWidth, Time.deltaTime * laneSpeed);
        newPosition.y = verticalTargetPosition.y;
        transform.position = newPosition;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Obstacle") && !invincible && !IsDead && canMove && !isTakingDamage)
        {
            TakeDamage();
        }
    }

    public void TakeDamage()
    {
        if (IsDead || invincible) return;

        GameDataLogger.RecordHit();
        isTakingDamage = true;
        canMove = false;
        currentLife--;
        DamageCount++;
        speed = 0;
        rb.velocity = Vector3.zero;

        if (uiManager != null)
        {
            uiManager.UpdateLives(currentLife);
        }

        if (anim != null)
        {
            anim.Rebind();
            anim.Update(0f);
            ForcePlayAnimation("Hit");
        }

        if (currentLife <= 0 || DamageCount >= 3)
        {
            StartCoroutine(Die());
        }
        else
        {
            StartCoroutine(RecoverFromHit());
        }
    }

    IEnumerator RecoverFromHit()
    {
        invincible = true;
        float timer = 0f;
        float blinkInterval = 0.1f;
        bool visible = true;

        // Blink effect during invincibility
        while (timer < hitAnimationDuration)
        {
            if (model != null) model.SetActive(visible);
            visible = !visible;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        if (model != null) model.SetActive(true);

        // Wait for hit animation to finish
        if (anim != null)
        {
            yield return new WaitUntil(() => !anim.GetCurrentAnimatorStateInfo(0).IsName("Hit"));
        }

        isTakingDamage = false;
        canMove = true;
        speed = minSpeed;
        
        if (anim != null)
        {
            ForcePlayAnimation("runStart");
        }

        // Remaining invincibility time
        while (timer < invincibleTime)
        {
            yield return new WaitForSeconds(0.1f);
            timer += 0.1f;
        }

        invincible = false;
    }

    IEnumerator Die()
    {
        IsDead = true;
        if (model != null) model.SetActive(true);
        
        if (anim != null)
        {
            anim.SetBool("Dead", true);
            anim.SetBool("Jumping", false);
        }
        
        rb.velocity = Vector3.zero;
        speed = 0;

        if (uiManager != null)
        {
            yield return new WaitForSeconds(3f);
            uiManager.ShowGameOver();
            GameDataLogger.SetGameResult(false);
            GameDataLogger.SaveToCSV();
            Invoke("ReturnToMenu", 2f);
        }
    }

    public void StopPlayer()
    {
        speed = 0f;
        canMove = false;
        rb.velocity = Vector3.zero;
        
        if (anim != null)
        {
            anim.SetBool("Running", false);
            anim.SetBool("Jumping", false);
        }
    }

    void ForcePlayAnimation(string animationName)
    {
        if (model != null && anim != null)
        {
            model.SetActive(true);
            anim.Play(animationName, -1, 0f);
            anim.Update(0f);
        }
    }

    void ReturnToMenu()
    {
        SceneManager.LoadScene("Menu_01");
    }
}