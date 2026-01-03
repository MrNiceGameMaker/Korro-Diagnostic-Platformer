using UnityEngine;
using KorroAI.Architecture;

namespace KorroAI.LevelSystem
{
    public class Key : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float bobSpeed = 2f;
        [SerializeField] private float bobHeight = 0.5f;

        [Header("Visual")]
        [SerializeField] private Transform visualModel;

        [Header("Events")]
        [Tooltip("Channel to broadcast when key is collected")]
        [SerializeField] private VoidEventChannelSO keyCollectedChannel;

        private Vector3 startVisualLocalPosition;

        private void Start()
        {
            // מציאת המודל הוויזואלי (כמו במטבע)
            if (visualModel == null)
            {
                if (transform.childCount > 0)
                {
                    visualModel = transform.GetChild(0);
                }
                else
                {
                    // Fallback אם אין ילד, נשתמש באובייקט עצמו (פחות מומלץ לרוטציה)
                    visualModel = transform;
                }
            }

            startVisualLocalPosition = visualModel.localPosition;
        }

        private void Update()
        {
            if (visualModel == null) return;

            // סיבוב סביב העולם (כדי שיראה טוב גם אם המודל שוכב ב-90 מעלות)
            visualModel.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.World);

            // ציפה למעלה ולמטה
            float newY = startVisualLocalPosition.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
            visualModel.localPosition = new Vector3(startVisualLocalPosition.x, newY, startVisualLocalPosition.z);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                // שידור האירוע למערכת
                if (keyCollectedChannel != null)
                {
                    keyCollectedChannel.RaiseEvent();
                    Debug.Log("🗝️ Key Collected!");
                }
                else
                {
                    Debug.LogWarning("Key Collected Channel is missing on the Key prefab!");
                }

                // השמדת המפתח
                Destroy(gameObject);
            }
        }
    }
}