using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using KorroAI.Architecture;
using Unity.Cinemachine; 

namespace KorroAI.Managers
{
    public class LevelFlowManager : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private VoidEventChannelSO levelCompletedChannel;
        [SerializeField] private IntEventChannelSO healthChangedChannel;
        [SerializeField] private IntEventChannelSO scoreChangedChannel;
        
        [Header("Campaign Data")]
        [SerializeField] private CampaignDataSO campaignData;

        [Header("Cutscene Settings")]
        [SerializeField] private float flyThroughDuration = 3f; 
        [SerializeField] private Vector3 cameraOffset = new Vector3(0, 5, -10);
        // משתנה חדש לשליטה בזווית - למשל (45, 0, 0)
        [SerializeField] private Vector3 cameraRotation = new Vector3(45, 0, 0);

        private int currentHealth;
        private int currentScore;
        private bool isCutscenePlaying = false;

        void Start()
        {
            StartCoroutine(PlayStartCutscene());
        }

        private IEnumerator PlayStartCutscene()
        {
            yield return new WaitForEndOfFrame();

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            GameObject door = GameObject.FindGameObjectWithTag("ExitDoor");
            Camera mainCam = Camera.main;
            CinemachineCamera vcam = FindFirstObjectByType<CinemachineCamera>();

            if (player == null || door == null || mainCam == null || vcam == null)
            {
                yield break;
            }

            isCutscenePlaying = true;

            vcam.enabled = false; 
            var controller = player.GetComponent<MonoBehaviour>(); 
            if (controller != null) controller.enabled = false;

            Vector3 startPos = door.transform.position + cameraOffset;
            Vector3 endPos = player.transform.position + cameraOffset;
            
            // קביעת הזווית לפי מה שהגדרת ב-Inspector
            Quaternion targetRotation = Quaternion.Euler(cameraRotation);
            
            mainCam.transform.position = startPos;
            mainCam.transform.rotation = targetRotation;

            float elapsed = 0;
            while (elapsed < flyThroughDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / flyThroughDuration;
                t = t * t * (3f - 2f * t); 

                mainCam.transform.position = Vector3.Lerp(startPos, endPos, t);
                // המצלמה שומרת על הזווית הקבועה לאורך כל הדרך
                mainCam.transform.rotation = targetRotation;
                
                yield return null;
            }

            vcam.enabled = true; 
            if (controller != null) controller.enabled = true;
            isCutscenePlaying = false;
        }

        private void OnEnable()
        {
            if (levelCompletedChannel != null) levelCompletedChannel.OnEventRaised += OnLevelComplete;
            if (healthChangedChannel != null) healthChangedChannel.OnEventRaised += (val) => currentHealth = val;
            if (scoreChangedChannel != null) scoreChangedChannel.OnEventRaised += (val) => currentScore = val;
        }

        private void OnDisable()
        {
            if (levelCompletedChannel != null) levelCompletedChannel.OnEventRaised -= OnLevelComplete;
        }

        private void OnLevelComplete()
        {
            if (GameSession.IsCampaignMode)
            {
                GameSession.SavedHealth = currentHealth;
                GameSession.SavedScore = currentScore;

                int nextIndex = GameSession.CurrentLevelIndex + 1;
                if (campaignData != null && nextIndex >= campaignData.LevelCount)
                {
                    Debug.Log("🏁 Last Level Reached!");
                }
            }
        }

        public void LoadNextLevel()
        {
            Time.timeScale = 1f;
            if (GameSession.IsCampaignMode)
            {
                int nextIndex = GameSession.CurrentLevelIndex + 1;
                if (campaignData != null && nextIndex < campaignData.LevelCount)
                {
                    GameSession.CurrentLevelIndex = nextIndex;
                    if (KorroAI.Analytics.AnalyticsManager.Instance != null)
                        KorroAI.Analytics.AnalyticsManager.Instance.StartNewLevel(nextIndex);
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                else SceneManager.LoadScene("MainMenu");
            }
            else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}