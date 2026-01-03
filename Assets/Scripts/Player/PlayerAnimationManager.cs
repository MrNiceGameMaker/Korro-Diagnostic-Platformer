using UnityEngine;

namespace KorroAI.LevelSystem
{
    public class PlayerAnimationManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerController playerController;
        [SerializeField] private Animator animator;

        private int verInputHash;
        private int isJumpingHash;
        private int isGroundedHash;
        private int hitHash; // <-- חדש

        private void Awake()
        {
            if (playerController == null) playerController = GetComponent<PlayerController>();
            if (animator == null) animator = GetComponentInChildren<Animator>();

            verInputHash = Animator.StringToHash("Vertical");
            isJumpingHash = Animator.StringToHash("isJumping");
            isGroundedHash = Animator.StringToHash("isGrounded");
            hitHash = Animator.StringToHash("Hit"); // <-- המרה למספר (חייב להתאים לשם באנימטור)
        }

        private void Update()
        {
            if (playerController == null || animator == null) return;

            // תנועה
            float speed = Mathf.Clamp01(Mathf.Abs(playerController.HorizontalInput) + Mathf.Abs(playerController.VerticalInput));
            animator.SetFloat(verInputHash, speed, 0.05f, Time.deltaTime);

            // קפיצה
            animator.SetBool(isJumpingHash, playerController.IsJumping);
            animator.SetBool(isGroundedHash, playerController.IsGrounded);

            // --- בדיקת מכה (חדש) ---
            // אנחנו שואלים את הקונטרולר: "האם קיבלנו מכה?"
            // הפונקציה Consume דואגת להחזיר True רק פעם אחת לכל מכה
            if (playerController.ConsumeHitTrigger())
            {
                animator.SetTrigger(hitHash);
            }
        }
    }
}
