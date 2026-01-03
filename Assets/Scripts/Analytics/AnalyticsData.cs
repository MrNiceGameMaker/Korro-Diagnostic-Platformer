using System;
using System.Collections.Generic;

namespace KorroAI.Analytics
{
    [Serializable]
    public class LevelStats
    {
        public int levelIndex;
        public float duration;           // כמה זמן לקח השלב
        public int coinsCollected;
        public int damageTaken;
        public int jumpCount = 0;        // שדה קריטי לשגיאות
        public bool isCompleted = false;
        
        // --- התנהגות שחקן ---
        public float totalIdleTime;      // כמה זמן "חשב"
        public float avgJumpInterval;    // קצב הקפיצות הממוצע (רגוע)
        public float stressJumpInterval; // קצב הקפיצות כשיש קצת חיים (לחץ)
    }

    [Serializable]
    public class RunSession
    {
        public string runDate;
        public List<LevelStats> levels = new List<LevelStats>();
        
        // --- סיכומים וממוצעים כלליים לריצה (חייבים להופיע כאן כדי למנוע שגיאות קומפילציה) ---
        public float overallAvgDuration;
        public float overallAvgCoins;
        public float overallAvgDamage;        
        public float overallAvgJumps;         
        public float overallAvgIdleTime;      
        public float overallAvgJumpInterval;  
        public float overallAvgStressInterval;
    }
}