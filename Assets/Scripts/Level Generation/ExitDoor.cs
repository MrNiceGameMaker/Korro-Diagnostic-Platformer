using UnityEngine;
using KorroAI.Architecture;

namespace KorroAI.LevelSystem
{
    public class ExitDoor : MonoBehaviour
    {
        [Header("Events")]
        [Tooltip("Event to listen to: When key is collected")]
        [SerializeField] private VoidEventChannelSO keyCollectedChannel;
        
        [Tooltip("Event to raise: When level is finished")]
        [SerializeField] private VoidEventChannelSO levelCompletedChannel;

        [Header("Visuals")]
        [SerializeField] private Renderer doorRenderer;
        [SerializeField] private Color lockedColor = Color.red;
        [SerializeField] private Color unlockedColor = Color.green;

        private bool isLocked = true;

        private void OnEnable()
        {
            // הרשמה לאירוע איסוף המפתח
            if (keyCollectedChannel != null)
                keyCollectedChannel.OnEventRaised += UnlockDoor;
                
            // אתחול צבע התחלתי
            UpdateDoorVisuals();
        }

        private void OnDisable()
        {
            // הסרה מהאירוע (חשוב למניעת דליפות זיכרון)
            if (keyCollectedChannel != null)
                keyCollectedChannel.OnEventRaised -= UnlockDoor;
        }

        private void UnlockDoor()
        {
            isLocked = false;
            Debug.Log("🚪 Door Unlocked! Go to the exit.");
            UpdateDoorVisuals();
        }

        private void UpdateDoorVisuals()
        {
            if (doorRenderer != null)
            {
                doorRenderer.material.color = isLocked ? lockedColor : unlockedColor;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (isLocked)
                {
                    Debug.Log("🔒 Door is locked. Find the key first!");
                }
                else
                {
                    Debug.Log("🎉 Level Completed!");
                    if (levelCompletedChannel != null)
                    {
                        levelCompletedChannel.RaiseEvent();
                    }
                    // כאן בעתיד נוסיף עצירה של המשחק או טעינת תפריט
                }
            }
        }
    }
}