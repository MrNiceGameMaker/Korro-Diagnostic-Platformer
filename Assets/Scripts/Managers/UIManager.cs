using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using KorroAI.Architecture;
using KorroAI.Analytics;

namespace KorroAI.Managers
{
    public class UIManager : MonoBehaviour
    {
        [Header("Events")]
        [SerializeField] private VoidEventChannelSO levelCompletedChannel;
        [SerializeField] private VoidEventChannelSO pauseChannel;
        [SerializeField] private VoidEventChannelSO playerDiedChannel;
        [SerializeField] private IntEventChannelSO scoreChangedChannel;
        [SerializeField] private IntEventChannelSO healthChangedChannel;
        [SerializeField] private VoidEventChannelSO keyCollectedChannel;

        [Header("HUD")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private Image keyIcon;
        [SerializeField] private Color keyActiveColor = Color.white;
        [SerializeField] private Color keyInactiveColor = new Color(1, 1, 1, 0.2f);

        [Header("Menu Panel")]
        [SerializeField] private GameObject gameMenuPanel;      
        [SerializeField] private TextMeshProUGUI menuTitleText; 
        [SerializeField] private TextMeshProUGUI analyticsText; 
        [SerializeField] private Button saveReportButton;       

        [Header("Buttons")]
        [SerializeField] private GameObject resumeButton;     
        [SerializeField] private GameObject nextLevelButton;  
        [SerializeField] private GameObject restartButton;    
        [SerializeField] private GameObject menuButton;       
        [SerializeField] private GameObject newRandomLevelButton;

        [Header("Data References")]
        [SerializeField] private CampaignDataSO campaignData;

        private enum MenuState { Pause, GameOver, LevelComplete }

        private void Awake()
        {
            if (gameMenuPanel != null) gameMenuPanel.SetActive(false);
            Time.timeScale = 1f;
            SetCursorState(false);
        }

        private void OnEnable()
        {
            if (levelCompletedChannel != null) levelCompletedChannel.OnEventRaised += ShowLevelComplete;
            if (pauseChannel != null) pauseChannel.OnEventRaised += TogglePause;
            if (playerDiedChannel != null) playerDiedChannel.OnEventRaised += ShowGameOver;
            if (scoreChangedChannel != null) scoreChangedChannel.OnEventRaised += UpdateScoreUI;
            if (healthChangedChannel != null) healthChangedChannel.OnEventRaised += UpdateHealthUI;
            if (keyCollectedChannel != null) keyCollectedChannel.OnEventRaised += ActivateKeyUI;
        }

        private void OnDisable()
        {
            if (levelCompletedChannel != null) levelCompletedChannel.OnEventRaised -= ShowLevelComplete;
            if (pauseChannel != null) pauseChannel.OnEventRaised -= TogglePause;
            if (playerDiedChannel != null) playerDiedChannel.OnEventRaised -= ShowGameOver;
            if (scoreChangedChannel != null) scoreChangedChannel.OnEventRaised -= UpdateScoreUI;
            if (healthChangedChannel != null) healthChangedChannel.OnEventRaised -= UpdateHealthUI;
            if (keyCollectedChannel != null) keyCollectedChannel.OnEventRaised -= ActivateKeyUI;
        }

        private void Start()
        {
            UpdateScoreUI(0);
            if (keyIcon != null) keyIcon.color = keyInactiveColor;

            if (saveReportButton != null)
            {
                saveReportButton.onClick.RemoveAllListeners();
                saveReportButton.onClick.AddListener(() =>
                {
                    if (AnalyticsManager.Instance != null)
                        AnalyticsManager.Instance.ExportRunToJSON();
                });
            }
        }

        private void TogglePause()
        {
            if (gameMenuPanel.activeSelf && Time.timeScale == 0f) ResumeGame();
            else ShowGameMenu(MenuState.Pause);
        }

        private void ShowGameOver() => ShowGameMenu(MenuState.GameOver);
        private void ShowLevelComplete() => ShowGameMenu(MenuState.LevelComplete);

        private void ShowGameMenu(MenuState state)
        {
            if (gameMenuPanel == null) return;

            gameMenuPanel.SetActive(true);
            Time.timeScale = 0f; 
            SetCursorState(true);

            // איפוס כפתורים
            if (resumeButton) resumeButton.SetActive(false);
            if (nextLevelButton) nextLevelButton.SetActive(false);
            if (newRandomLevelButton) newRandomLevelButton.SetActive(false);
            if (restartButton) restartButton.SetActive(true);
            if (menuButton) menuButton.SetActive(true);

            if (analyticsText != null && AnalyticsManager.Instance != null)
                analyticsText.text = AnalyticsManager.Instance.GetFormattedReport();

            switch (state)
            {
                case MenuState.Pause:
                    if (menuTitleText) menuTitleText.text = "PAUSED";
                    if (resumeButton) resumeButton.SetActive(true);
                    SetRestartButtonText("RESTART");
                    break;

                case MenuState.GameOver:
                    if (menuTitleText) menuTitleText.text = "GAME OVER";
                    if (GameSession.IsCampaignMode)
                    {
                        SetRestartButtonText("RESTART CAMPAIGN");
                    }
                    else
                    {
                        SetRestartButtonText("RETRY SEED");
                        if (newRandomLevelButton) newRandomLevelButton.SetActive(true);
                    }
                    break;

                case MenuState.LevelComplete:
                    HandleLevelCompleteUI();
                    break;
            }
        }

        private void HandleLevelCompleteUI()
        {
            if (GameSession.IsCampaignMode)
            {
                int nextIndex = GameSession.CurrentLevelIndex + 1;
                bool hasMoreLevels = campaignData != null && nextIndex < campaignData.LevelCount;

                if (hasMoreLevels)
                {
                    if (menuTitleText) menuTitleText.text = "LEVEL COMPLETE!";
                    if (nextLevelButton) nextLevelButton.SetActive(true);
                }
                else
                {
                    if (menuTitleText) menuTitleText.text = "VICTORY!";
                    if (nextLevelButton) nextLevelButton.SetActive(false);
                }
                SetRestartButtonText("RESTART");
            }
            else
            {
                if (menuTitleText) menuTitleText.text = "LEVEL COMPLETE!";
                SetRestartButtonText("RETRY SEED");
                if (newRandomLevelButton) newRandomLevelButton.SetActive(true);
            }
        }

        public void RestartGame()
        {
            Time.timeScale = 1f;

            if (GameSession.IsCampaignMode)
            {
                // תיקון: איפוס מלא של הקמפיין במקרה של מוות או רצון להתחיל מחדש
                GameSession.ResetSession(); 
                
                if (AnalyticsManager.Instance != null)
                    AnalyticsManager.Instance.ResetAnalytics();

                // טעינה מחדש תגרום לטעינת שלב 0 בגלל ה-ResetSession
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
            else
            {
                // במצב אקראי שומרים על הרצף (Retry Seed)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }

        public void LoadNewRandomLevel()
        {
            Time.timeScale = 1f;
            if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.ResetAnalytics();
            GameSession.GenerateNewRandomSeed();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        public void GoToMainMenu()
        {
            if (AnalyticsManager.Instance != null) AnalyticsManager.Instance.ResetAnalytics();
            GameSession.ResetSession();
            Time.timeScale = 1f;
            SceneManager.LoadScene("MainMenu");
        }

        private void SetRestartButtonText(string text)
        {
            if (restartButton == null) return;
            var tmp = restartButton.GetComponentInChildren<TextMeshProUGUI>();
            if (tmp != null) tmp.text = text;
        }

        private void SetCursorState(bool visible)
        {
            Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = visible;
        }

        public void ResumeGame()
        {
            if (gameMenuPanel != null) gameMenuPanel.SetActive(false);
            Time.timeScale = 1f;
            SetCursorState(false);
        }

        private void UpdateScoreUI(int score) { if (scoreText) scoreText.text = $"Coins: {score}"; }
        private void UpdateHealthUI(int hp) { if (healthText) healthText.text = $"HP: {hp}"; }
        private void ActivateKeyUI() { if (keyIcon) keyIcon.color = keyActiveColor; }
    }
}