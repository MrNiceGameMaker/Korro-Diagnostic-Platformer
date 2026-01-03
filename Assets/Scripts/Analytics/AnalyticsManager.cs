using UnityEngine;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using KorroAI.Architecture;

namespace KorroAI.Analytics
{
    public class AnalyticsManager : MonoBehaviour
    {
        public static AnalyticsManager Instance;

        [Header("--- Listening To ---")]
        [SerializeField] private VoidEventChannelSO coinCollectedEvent;
        [SerializeField] private VoidEventChannelSO levelCompletedEvent;
        [SerializeField] private VoidEventChannelSO jumpEvent;
        [SerializeField] private IntEventChannelSO healthChangedEvent; 
        [SerializeField] private VoidEventChannelSO playerDiedEvent; 

        private RunSession currentRun;
        private LevelStats currentLevelStats;
        
        private float levelStartTime;
        private float lastJumpTime;
        private int currentHealth = 3;

        private List<float> jumpIntervals = new List<float>();
        private List<float> stressJumpIntervals = new List<float>();

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                StartNewRun();
            }
            else Destroy(gameObject);
        }

     public void StartNewRun()
{
    currentRun = new RunSession();
    currentRun.runDate = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
    StartNewLevel(0); // מתחילים משלב 0 ברשימה חדשה
    Debug.Log("📊 Analytics: New Run Started (Full Reset)");
}

        public void ResetAnalytics() => StartNewRun();

        public void StartNewLevel(int index)
        {
            currentLevelStats = new LevelStats();
            currentLevelStats.levelIndex = index;
            levelStartTime = Time.time;
            lastJumpTime = Time.time;
            jumpIntervals.Clear();
            stressJumpIntervals.Clear();
        }

        private void OnEnable()
        {
            if (coinCollectedEvent) coinCollectedEvent.OnEventRaised += OnCoin;
            if (jumpEvent) jumpEvent.OnEventRaised += OnJump;
            if (healthChangedEvent) healthChangedEvent.OnEventRaised += OnHealthChanged;
            if (levelCompletedEvent) levelCompletedEvent.OnEventRaised += OnLevelComplete;
            if (playerDiedEvent) playerDiedEvent.OnEventRaised += OnGameOver;
        }
        
        private void OnDisable()
        {
            if (coinCollectedEvent) coinCollectedEvent.OnEventRaised -= OnCoin;
            if (jumpEvent) jumpEvent.OnEventRaised -= OnJump;
            if (healthChangedEvent) healthChangedEvent.OnEventRaised -= OnHealthChanged;
            if (levelCompletedEvent) levelCompletedEvent.OnEventRaised -= OnLevelComplete;
            if (playerDiedEvent) playerDiedEvent.OnEventRaised -= OnGameOver;
        }

        private void OnCoin() => currentLevelStats.coinsCollected++;
        private void OnHealthChanged(int hp) 
        {
            if (hp < currentHealth) currentLevelStats.damageTaken++;
            currentHealth = hp;
        }

        public void AddIdleTime(float time) => currentLevelStats.totalIdleTime += time;

        private void OnJump()
        {
            currentLevelStats.jumpCount++;
            float interval = Time.time - lastJumpTime;
            lastJumpTime = Time.time;
            if (interval > 10f) return;
            if (currentHealth <= 1) stressJumpIntervals.Add(interval);
            else jumpIntervals.Add(interval);
        }

        private void OnLevelComplete() => FinalizeLevelStats(true);
        private void OnGameOver() => FinalizeLevelStats(false);

 private void FinalizeLevelStats(bool completed)
{
    UpdateCurrentLevelLiveStats(completed);
    currentRun.levels.Add(currentLevelStats);
    CalculateOverallAverages();
    
    // הסרנו מכאן את הקריאה ל-StartNewLevel
}

        // פונקציה לעדכון הנתונים בזמן אמת (עבור התפריט)
        private void UpdateCurrentLevelLiveStats(bool? completed = null)
        {
            currentLevelStats.duration = Time.time - levelStartTime;
            if (completed.HasValue) currentLevelStats.isCompleted = completed.Value;
            currentLevelStats.avgJumpInterval = jumpIntervals.Count > 0 ? jumpIntervals.Average() : 0;
            currentLevelStats.stressJumpInterval = stressJumpIntervals.Count > 0 ? stressJumpIntervals.Average() : 0;
        }

        private void CalculateOverallAverages()
        {
            if (currentRun.levels.Count == 0) return;
            currentRun.overallAvgDuration = currentRun.levels.Average(l => l.duration);
            currentRun.overallAvgCoins = (float)currentRun.levels.Average(l => l.coinsCollected);
            currentRun.overallAvgDamage = (float)currentRun.levels.Average(l => l.damageTaken);
            currentRun.overallAvgJumps = (float)currentRun.levels.Average(l => l.jumpCount);
            currentRun.overallAvgIdleTime = currentRun.levels.Average(l => l.totalIdleTime);
            
            var validIntervals = currentRun.levels.Where(l => l.avgJumpInterval > 0).ToList();
            currentRun.overallAvgJumpInterval = validIntervals.Count > 0 ? validIntervals.Average(l => l.avgJumpInterval) : 0;

            var validStress = currentRun.levels.Where(l => l.stressJumpInterval > 0).ToList();
            currentRun.overallAvgStressInterval = validStress.Count > 0 ? validStress.Average(l => l.stressJumpInterval) : 0;
        }

       public string GetFormattedReport()
{
    CalculateOverallAverages();
    UpdateCurrentLevelLiveStats(); 

    StringBuilder sb = new StringBuilder();
    sb.AppendLine("<size=130%><color=#FFD700>--- SESSION ANALYSIS ---</color></size>");
    sb.AppendLine($"<b>Avg Time:</b> {currentRun.overallAvgDuration:F1}s | <b>Avg Coins:</b> {currentRun.overallAvgCoins:F1}");
    sb.AppendLine($"<b>Avg Damage:</b> {currentRun.overallAvgDamage:F1} | <b>Avg Jumps:</b> {currentRun.overallAvgJumps:F1}");
    sb.AppendLine($"<b>Avg Idle:</b> {currentRun.overallAvgIdleTime:F1}s");
    sb.AppendLine("------------------------------");

    // הצג IN PROGRESS רק אם השלב הנוכחי לא הסתיים (למשל באמצע משחק או אחרי מוות שטרם עבר ריסטארט)
    if (!currentLevelStats.isCompleted)
    {
        sb.AppendLine($"<b>Lvl {currentLevelStats.levelIndex + 1}:</b> <color=yellow>IN PROGRESS</color> | {currentLevelStats.duration:F1}s");
        sb.AppendLine($"Jumps: {currentLevelStats.jumpCount} | Damage: {currentLevelStats.damageTaken} | Idle: {currentLevelStats.totalIdleTime:F1}s");
        sb.AppendLine("---");
    }

    for (int i = currentRun.levels.Count - 1; i >= 0; i--)
    {
        LevelStats s = currentRun.levels[i];
        string status = s.isCompleted ? "<color=green>WIN</color>" : "<color=red>FAIL</color>";
        sb.AppendLine($"<b>Lvl {s.levelIndex + 1}:</b> {status} | {s.duration:F1}s");
        sb.AppendLine($"Jumps: {s.jumpCount} | Damage: {s.damageTaken} | Idle: {s.totalIdleTime:F1}s");
        if (s.duration < currentRun.overallAvgDuration) sb.AppendLine("<color=green><i>* Faster than average!</i></color>");
        sb.AppendLine("---");
    }
    return sb.ToString();
}

 public void ExportRunToJSON()
{
    CalculateOverallAverages();

    // 1. יצירת עותק זמני של הריצה
    RunSession snapshotRun = new RunSession();
    snapshotRun.runDate = currentRun.runDate;
    snapshotRun.levels = new List<LevelStats>(currentRun.levels);

    // 2. עדכון נתוני השלב הנוכחי
    UpdateCurrentLevelLiveStats(); 
    
    // --- התיקון: בדיקה האם השלב הנוכחי כבר מופיע ברשימה כשלב שהסתיים ---
    // אם האינדקס של השלב הנוכחי כבר קיים ברשימה, סימן שהוא כבר נשמר כ-WIN/FAIL
    bool alreadySaved = currentRun.levels.Any(l => l.levelIndex == currentLevelStats.levelIndex);

    if (!alreadySaved)
    {
        LevelStats currentInProgress = new LevelStats();
        currentInProgress.levelIndex = currentLevelStats.levelIndex;
        currentInProgress.duration = currentLevelStats.duration;
        currentInProgress.coinsCollected = currentLevelStats.coinsCollected;
        currentInProgress.damageTaken = currentLevelStats.damageTaken;
        currentInProgress.jumpCount = currentLevelStats.jumpCount;
        currentInProgress.totalIdleTime = currentLevelStats.totalIdleTime;
        currentInProgress.avgJumpInterval = currentLevelStats.avgJumpInterval;
        currentInProgress.stressJumpInterval = currentLevelStats.stressJumpInterval;
        currentInProgress.isCompleted = false; 

        snapshotRun.levels.Add(currentInProgress);
    }

    // 3. חישוב ממוצעים מעודכנים עבור הקובץ
    if (snapshotRun.levels.Count > 0)
    {
        snapshotRun.overallAvgDuration = snapshotRun.levels.Average(l => l.duration);
        snapshotRun.overallAvgCoins = (float)snapshotRun.levels.Average(l => l.coinsCollected);
        snapshotRun.overallAvgDamage = (float)snapshotRun.levels.Average(l => l.damageTaken);
        snapshotRun.overallAvgJumps = (float)snapshotRun.levels.Average(l => l.jumpCount);
    }

    // 4. שמירה לקובץ
    string json = JsonUtility.ToJson(snapshotRun, true);
    string folderPath = Path.Combine(Application.dataPath, "RunReports");
    if (!Directory.Exists(folderPath)) Directory.CreateDirectory(folderPath);

    string timestamp = System.DateTime.Now.ToString("HH-mm-ss");
    string fileName = $"Run_{currentRun.runDate}_Snapshot_{timestamp}.json";
    
    File.WriteAllText(Path.Combine(folderPath, fileName), json);
    Debug.Log($"📊 Snapshot Saved: {fileName}");

    #if UNITY_EDITOR
    UnityEditor.AssetDatabase.Refresh();
    #endif
}
    }
}