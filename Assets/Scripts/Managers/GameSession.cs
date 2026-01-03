using UnityEngine;

namespace KorroAI.Managers
{
    public static class GameSession
    {
        // הגדרות משחק
        public static bool IsCampaignMode = true;
        public static int CurrentLevelIndex = 0;
        
        // --- ניהול סיד לרנדום ---
        public static int RandomSeed = 0;

        // שמירת נתונים בין שלבים
        public static int SavedHealth = 3; 
        public static int SavedScore = 0;

        // הוספת הפונקציה בשם המדויק שה-UIManager מחפש
        public static void GenerateNewRandomSeed()
        {
            RandomSeed = Random.Range(0, 1000000);
            IsCampaignMode = false; // כשמגרילים סיד חדש, אנחנו עוברים למצב רנדום
            Debug.Log($"🎲 New Seed Generated: {RandomSeed}");
        }

        // פונקציה לאיפוס נתונים (נקראת כשחוזרים לתפריט או מתחילים מחדש)
        public static void ResetSession()
        {
            CurrentLevelIndex = 0;
            SavedHealth = 3; 
            SavedScore = 0;
        }
    }
}