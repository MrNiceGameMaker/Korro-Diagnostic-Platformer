using System;
using System.Collections.Generic;
using UnityEngine;

namespace KorroAI.LevelSystem
{
    [Serializable]
    public class SavedPlatformData
    {
        public int typeIndex;       // סוג הפלטפורמה (רגיל, זז, קרח...)
        public Vector3 position;    // מיקום
        public Vector3 scale;       // גודל
        
        // מה יושב על הפלטפורמה?
        public bool hasCoin;
        public bool hasTrap;
        public bool hasKey;
        public bool hasDoor;
    }

    [Serializable]
    public class SavedLevelData
    {
        public int levelId;
        public List<SavedPlatformData> platforms = new List<SavedPlatformData>();
    }
}