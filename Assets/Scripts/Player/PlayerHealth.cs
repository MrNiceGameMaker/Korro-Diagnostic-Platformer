using UnityEngine;
using System.Collections;
using KorroAI.Architecture;
using KorroAI.LevelSystem; // בשביל הגישה לקונטרולר אם צריך
using KorroAI.Managers;

namespace KorroAI.Gameplay
{
    public class PlayerHealth : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int maxHealth = 3;
        [SerializeField] private float invincibilityDuration = 3.0f; // ביקשת 3 שניות
        [SerializeField] private Color damageColor = Color.red;
        [SerializeField] private float flashSpeed = 0.2f; // מהירות ההבהוב

        [Header("Events")]
        [SerializeField] private IntEventChannelSO healthChangedChannel;
        [SerializeField] private VoidEventChannelSO playerDiedChannel;

        private int currentHealth;
        private bool isInvincible = false;
        private Renderer playerRenderer;
        private Color originalColor;
        
        // נשמור רפרנס לקורוטינה כדי למנוע כפילויות
        private Coroutine flashCoroutine;

        private void Start()
        {
            // אם אנחנו בקמפיין (או מצב מתמשך), טען את החיים שנשמרו
            if (GameSession.IsCampaignMode)
            {
                currentHealth = GameSession.SavedHealth;
            }
            else
            {
                currentHealth = maxHealth; // התחלה רגילה
            }

            // עדכון ה-UI ההתחלתי
            if (healthChangedChannel != null) healthChangedChannel.RaiseEvent(currentHealth);
        }

       public void TakeDamage(int damage)
{
    if (currentHealth <= 0) return; // אם כבר מתים, לא עושים כלום

    currentHealth -= damage;
    
    // --- התיקון: לא נותנים למספר לרדת מתחת ל-0 ---
    if (currentHealth < 0) currentHealth = 0; 
    
    // עדכון UI וכו'...
    if (healthChangedChannel != null) healthChangedChannel.RaiseEvent(currentHealth);

    if (currentHealth == 0) Die();
}

        private IEnumerator InvincibilityRoutine()
        {
            isInvincible = true;
            
            float elapsed = 0f;
            bool isRed = false;

            while (elapsed < invincibilityDuration)
            {
                elapsed += flashSpeed;
                
                // החלפת צבעים (Red <-> Original)
                if (playerRenderer != null)
                {
                    isRed = !isRed;
                    playerRenderer.material.color = isRed ? damageColor : originalColor;
                }
                
                yield return new WaitForSeconds(flashSpeed);
            }

            // סיום: החזרת צבע מקורי וביטול חסינות
            if (playerRenderer != null) playerRenderer.material.color = originalColor;
            isInvincible = false;
        }

        private void Die()
        {
            Debug.Log("💀 Player Died!");
            if (playerDiedChannel != null)
                playerDiedChannel.RaiseEvent();
        }
        
        public void ResetHealth()
        {
            currentHealth = maxHealth;
            isInvincible = false;
            if (playerRenderer != null) playerRenderer.material.color = originalColor;
            
            if (healthChangedChannel != null) 
                healthChangedChannel.RaiseEvent(currentHealth);
        }
    }
}