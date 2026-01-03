using UnityEngine;

namespace KorroAI.Audio
{
    [CreateAssetMenu(fileName = "NewSoundData", menuName = "KorroAI/Audio/Sound Data")]
    public class SoundDataSO : ScriptableObject
    {
        [Header("Audio Config")]
        public AudioClip[] clips; // מערך - כדי שתוכל לשים כמה וריאציות לאותו סאונד
        
        [Range(0f, 1f)] public float volume = 1f;
        
        [Header("Random Pitch (The Juice)")]
        [Range(0.1f, 3f)] public float minPitch = 0.9f; // שינוי קטן בפיץ' נותן תחושה טבעית
        [Range(0.1f, 3f)] public float maxPitch = 1.1f;

        [Header("3D Settings")]
        [Range(0f, 1f)] public float spatialBlend = 0f; // 0 = דו מימד (UI/מוזיקה), 1 = תלת מימד (צעדים/פיצוצים)
        public float minDistance = 1f;
        public float maxDistance = 20f;

        // פונקציה לבחירת קליפ אקראי מתוך הרשימה
        public AudioClip GetRandomClip()
        {
            if (clips.Length == 0) return null;
            return clips[Random.Range(0, clips.Length)];
        }
    }
}