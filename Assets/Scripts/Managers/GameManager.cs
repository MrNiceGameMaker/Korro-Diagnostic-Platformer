using UnityEngine;
using KorroAI.Architecture;

namespace KorroAI.Managers
{
    public class GameManager : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private VoidEventChannelSO levelCompletedChannel;
private void Awake()
        {
            Time.timeScale = 1f;
        }
        private void OnEnable()
        {
            if (levelCompletedChannel != null)
                levelCompletedChannel.OnEventRaised += OnLevelCompleted;
        }

        private void OnDisable()
        {
            if (levelCompletedChannel != null)
                levelCompletedChannel.OnEventRaised -= OnLevelCompleted;
        }

        private void OnLevelCompleted()
        {
            Debug.Log("🏆 GAME WON! Stopping Time.");
            
            // עצירת הזמן (אפקט של סיום משחק)
            Time.timeScale = 0f;
            
            // החזרת העכבר כדי שאפשר יהיה ללחוץ על תפריטים בעתיד
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}