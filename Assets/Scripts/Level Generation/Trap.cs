using UnityEngine;
using KorroAI.Gameplay;

namespace KorroAI.LevelSystem
{
    public class Trap : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private int damageAmount = 1;
        [SerializeField] private float knockbackForce = 2f; // עדכנתי ל-2 כמו שביקשת

        [Header("Visuals & FX")]
        [Tooltip("Prefab containing Particles and Audio to play on destruction")]
        [SerializeField] private GameObject deathVfx; // <-- ההכנה לעתיד

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // 1. נזק
                var health = other.GetComponent<PlayerHealth>();
                if (health != null) health.TakeDamage(damageAmount);

                // 2. רתע
                var controller = other.GetComponent<PlayerController>();
                if (controller != null)
                {
                    Vector3 knockbackDir = other.transform.position - transform.position;
                    knockbackDir.y = 0; 
                    knockbackDir.Normalize();
                    controller.ApplyKnockback(knockbackDir, knockbackForce);
                }

                // 3. יצירת האפקט (אם קיים)
                if (deathVfx != null)
                {
                    Instantiate(deathVfx, transform.position, Quaternion.identity);
                }

                // 4. השמדת המלכודת
                Destroy(gameObject);
            }
        }
    }
}