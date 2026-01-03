using UnityEngine;
using KorroAI.Architecture;

namespace KorroAI.Managers
{
    public class ScoreManager : MonoBehaviour
    {
        [Header("Listening To")]
        [SerializeField] private VoidEventChannelSO coinCollectedChannel;

        [Header("Broadcasting To")]
        [SerializeField] private IntEventChannelSO scoreChangedChannel; // הערוץ החדש

        private int currentScore = 0;

        private void Start()
        {
            if (GameSession.IsCampaignMode)
            {
                currentScore = GameSession.SavedScore;
            }
            else
            {
                currentScore = 0;
            }

            if (scoreChangedChannel != null)
            {
                scoreChangedChannel.RaiseEvent(currentScore);
            }
        }

        private void OnEnable()
        {
            if (coinCollectedChannel != null)
                coinCollectedChannel.OnEventRaised += AddScore;
        }

        private void OnDisable()
        {
            if (coinCollectedChannel != null)
                coinCollectedChannel.OnEventRaised -= AddScore;
        }

        private void AddScore()
        {
            currentScore++;
            // עדכון ה-UI דרך האירוע
            if (scoreChangedChannel != null)
                scoreChangedChannel.RaiseEvent(currentScore);
                
            Debug.Log($"Score updated: {currentScore}");
        }
    }
}
