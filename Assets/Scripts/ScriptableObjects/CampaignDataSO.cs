using System.Collections.Generic;
using UnityEngine;

namespace KorroAI.Architecture
{
    [CreateAssetMenu(fileName = "NewCampaign", menuName = "KorroAI/Campaign Data")]
    public class CampaignDataSO : ScriptableObject
    {
        [Header("Campaign Configuration")]
        [Tooltip("Drag your JSON level files here in the desired order")]
        public List<TextAsset> campaignLevels = new List<TextAsset>();

        // פונקציית עזר לקבלת כמות השלבים
        public int LevelCount => campaignLevels.Count;

        // פונקציית עזר לקבלת שלב ספציפי
        public TextAsset GetLevelData(int index)
        {
            if (index >= 0 && index < campaignLevels.Count)
            {
                return campaignLevels[index];
            }
            return null;
        }
    }
}