using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using KorroAI.Architecture;
using KorroAI.Gameplay;
using KorroAI.Analytics;

namespace KorroAI.LevelSystem
{
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [Header("--- Input Setup ---")]
        private PlayerControls inputActions;

        [Header("--- Visuals ---")]
        [SerializeField] private float rotationSpeed = 15f;

        [Header("--- Movement ---")]
        [SerializeField] private float moveSpeed = 8.0f;
        
        [Header("--- Jumping ---")]
        [SerializeField] private float jumpHeight = 3.0f;
        [SerializeField] private int maxJumps = 2;
        [SerializeField] private float gravity = -30f;
        [SerializeField] private float fallGravityMultiplier = 2.0f;
        
        [Header("--- Game Feel ---")]
        [SerializeField] private float coyoteTime = 0.2f;
        [SerializeField] private float jumpBufferTime = 0.2f;
        [SerializeField] private CinemachineImpulseSource impulseSource;

        [Header("--- Reset Logic ---")]
        [SerializeField] private float deathYThreshold = -10f;
        [SerializeField] private Vector3 respawnPosition = new Vector3(0f, 2f, 0f);
        [SerializeField] private VoidEventChannelSO playerDiedChannel;

        [Header("Events")]
        [SerializeField] private VoidEventChannelSO pauseChannel;
        [SerializeField] private VoidEventChannelSO jumpChannel;

        private CharacterController characterController;
        private Vector3 velocity; // וקטור לכוח משיכה וקפיצות (Y)
        private int jumpsRemaining;
        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private float currentTraction = 1.0f;
        private Vector3 momentum = Vector3.zero; // וקטור לתנועה אופקית (X, Z)
        private Vector3 platformVelocity = Vector3.zero;
        private float lastVelocityY;
        private bool wasGrounded;

        private float jumpCooldownTimer = 0f; 

        // --- משתנים חדשים לטיפול במכה ---
        private float stunTimer = 0f; // טיימר להקפאת תנועה אחרי מכה
        private bool hitTriggered = false;

        public float HorizontalInput { get; private set; } 
        public float VerticalInput { get; private set; }   
        public bool IsJumping { get; private set; }
        public bool IsGrounded => characterController.isGrounded;

        private void Awake()
        {
            characterController = GetComponent<CharacterController>();
            inputActions = new PlayerControls();

            inputActions.Gameplay.Jump.started += ctx => OnJumpPressed();
            inputActions.Gameplay.Jump.canceled += ctx => OnJumpReleased();
            inputActions.Gameplay.Reset.performed += ctx => OnResetLevel();
            inputActions.Gameplay.Pause.performed += ctx => OnPausePressed();
        }

        private void OnEnable() 
        {
            inputActions.Enable();
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (playerDiedChannel != null) playerDiedChannel.OnEventRaised += Respawn;
        }

        private void OnDisable() 
        {
            inputActions.Disable();
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (playerDiedChannel != null) playerDiedChannel.OnEventRaised -= Respawn;
        }

        private void Update()
        {
            if (HorizontalInput == 0 && VerticalInput == 0 && characterController.isGrounded && stunTimer <= 0)
    {
        if (AnalyticsManager.Instance != null)
        {
            AnalyticsManager.Instance.AddIdleTime(Time.deltaTime);
        }
    }
            // עדכון טיימר הלם
            if (stunTimer > 0)
            {
                stunTimer -= Time.deltaTime;
                // בזמן הלם, אנחנו לא קוראים את האינפוט מהשחקן
                HorizontalInput = 0;
                VerticalInput = 0;
            }
            else
            {
                // קריאה רגילה
                Vector2 input = inputActions.Gameplay.Move.ReadValue<Vector2>();
                HorizontalInput = input.x;
                VerticalInput = input.y;
            }

            // לוגיקת קפיצה ונחיתה
            if (jumpCooldownTimer > 0) jumpCooldownTimer -= Time.deltaTime;

            if (characterController.isGrounded && jumpCooldownTimer <= 0)
            {
                IsJumping = false;
                if (!wasGrounded && lastVelocityY < -5f && impulseSource != null) 
                {
                    impulseSource.GenerateImpulseWithForce(0.5f);
                }
            }
            
            UpdateTimers();
            HandleMovement();
            HandleGravity();
            CheckDeath();

            // קפיצה אפשרית רק אם אנחנו לא בהלם
            if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && stunTimer <= 0)
            {
                PerformJump();
            }

            platformVelocity = Vector3.Lerp(platformVelocity, Vector3.zero, Time.deltaTime * 10f);
            
            wasGrounded = characterController.isGrounded;
            lastVelocityY = velocity.y;
        }

       private void HandleMovement()
        {
            Transform camTransform = Camera.main.transform;
            Vector3 camForward = camTransform.forward;
            Vector3 camRight = camTransform.right;
            camForward.y = 0; camRight.y = 0;
            camForward.Normalize(); camRight.Normalize();

            Vector3 targetMove = Vector3.zero;

            if (stunTimer <= 0)
            {
                targetMove = (camForward * VerticalInput + camRight * HorizontalInput) * moveSpeed;
            }

            // --- התיקון כאן ---
            float lerpSpeed;
            if (stunTimer > 0)
            {
                // בלימה אגרסיבית! (העלינו מ-5 ל-15)
                // ככל שהמספר הזה גבוה יותר, השחקן יעצור מהר יותר אחרי המכה
                lerpSpeed = 15f; 
            }
            else if (characterController.isGrounded)
            {
                currentTraction = Mathf.Lerp(currentTraction, 1.0f, Time.deltaTime * 2f);
                lerpSpeed = currentTraction * 10f;
            }
            else
            {
                lerpSpeed = 2f;
            }

            momentum = Vector3.Lerp(momentum, targetMove, lerpSpeed * Time.deltaTime);
            characterController.Move((momentum + platformVelocity) * Time.deltaTime);

            if (targetMove != Vector3.zero && stunTimer <= 0)
            {
                Quaternion targetRotation = Quaternion.LookRotation(targetMove);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }

        public void ApplyKnockback(Vector3 direction, float force)
        {
            platformVelocity = Vector3.zero;
            
            direction.y = 0;
            momentum = direction.normalized * force;

            // --- התיקון כאן ---
            // הורדנו את הקפיצה מ-5 ל-2. זה ירגיש יותר כמו מכה ופחות כמו שיגור לחלל
            velocity.y = 2f; 

            stunTimer = 0.5f;
            hitTriggered = true;
            
            Debug.Log("💥 KNOCKBACK & STUN!");
        }

        public bool ConsumeHitTrigger()
        {
            if (hitTriggered)
            {
                hitTriggered = false;
                return true;
            }
            return false;
        }

        // ... (שאר הפונקציות: HandleGravity, PerformJump, Respawn וכו' נשארות אותו דבר) ...
        // הקפד להעתיק את שאר הפונקציות מהקובץ הקודם שלך אם חסרות כאן
        
        private void UpdateTimers()
        {
            if (characterController.isGrounded) { coyoteTimeCounter = coyoteTime; jumpsRemaining = maxJumps; }
            else { coyoteTimeCounter -= Time.deltaTime; }
            if (jumpBufferCounter > 0) jumpBufferCounter -= Time.deltaTime;
        }

        private void HandleGravity()
        {
            if (velocity.y < 0) velocity.y += gravity * fallGravityMultiplier * Time.deltaTime;
            else velocity.y += gravity * Time.deltaTime;
            if (characterController.isGrounded && velocity.y < 0) velocity.y = -2f;
            characterController.Move(velocity * Time.deltaTime);
        }

        private void PerformJump()
        {
            jumpBufferCounter = 0; 
            coyoteTimeCounter = 0; 
            jumpsRemaining--;
            
            // חישוב הפיזיקה של הקפיצה
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
            
            IsJumping = true; 
            jumpCooldownTimer = 0.2f;

            // --- הוסף את זה כאן ---
            if (jumpChannel != null) 
            {
                jumpChannel.RaiseEvent();
            }
        }
        private void OnJumpPressed()
        {
            jumpBufferCounter = jumpBufferTime;
            if ((coyoteTimeCounter > 0 || jumpsRemaining > 1) && stunTimer <= 0) PerformJump();
        }
        
        private void OnJumpReleased() { if (velocity.y > 0) velocity.y *= 0.5f; }
        
private void CheckDeath()
        {
            if (transform.position.y < deathYThreshold)
            {
                // 1. קודם כל - עונש! מורידים חיים אחד
                var health = GetComponent<PlayerHealth>();
                if (health != null)
                {
                    health.TakeDamage(1); 
                }

                // 2. מחזירים את השחקן להתחלה
                Respawn();
            }
        }        
        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            BasePlatform platform = hit.gameObject.GetComponentInParent<BasePlatform>();
            if (platform != null && hit.normal.y > 0.5f) platform.OnPlayerTouch(this);
        }

        public void SetPlatformVelocity(Vector3 vel) => platformVelocity = vel;
        public void SetTraction(float traction) => currentTraction = traction;
        public void ApplyJumpForce(float forceMultiplier) { velocity.y = Mathf.Sqrt(jumpHeight * forceMultiplier * -2f * gravity); IsJumping = true; jumpCooldownTimer = 0.2f; }
        
        private void OnResetLevel()
        {
            var generator = FindFirstObjectByType<LevelGenerator>();
            if (generator != null) { generator.GenerateLevel(); Respawn(); }
        }

        private void OnPausePressed()
        {
            if (pauseChannel != null) pauseChannel.RaiseEvent();
        }

       private void Respawn()
        {
            // ניתוק זמני של הקונטרולר כדי להזיז את הדמות בלי באגים
            characterController.enabled = false;
            transform.position = respawnPosition;
            characterController.enabled = true;

            // איפוס פיזיקה
            velocity = Vector3.zero;
            momentum = Vector3.zero;
            platformVelocity = Vector3.zero;
            stunTimer = 0; 

            // --- שינוי חשוב: מחקנו את השורה health.ResetHealth() ---
            // הסיבה: אם נפלנו, אנחנו רוצים להישאר עם הפחות חיים, לא להירפא.
            // (האיפוס המלא קורה רק כשמתחילים שלב חדש או משחק חדש דרך ה-Manager)
        }
    }
}