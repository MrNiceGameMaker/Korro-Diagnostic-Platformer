using System.Collections.Generic;
using UnityEngine;
using System.IO; 
using KorroAI.Managers;
using KorroAI.Architecture;

namespace KorroAI.LevelSystem
{
    [System.Serializable]
    public struct PlatformTypeWeight
    {
        public string name;
        public int typeIndex;
        public float weight;
        [Range(0, 100)] public float coinSpawnChance;
    }

    public class LevelGenerator : MonoBehaviour
    {
        [Header("Campaign Configuration")]
        [SerializeField] private CampaignDataSO campaignData; 

        [Header("Prefabs")]
        [SerializeField] private GameObject basePlatformPrefab;
        [SerializeField] private GameObject coinPrefab;
        [SerializeField] private GameObject keyPrefab;
        [SerializeField] private GameObject doorPrefab;
        [SerializeField] private GameObject trapPrefab;
        
        [Header("Config (Random Mode)")]
        [SerializeField] private int levelLength = 20;
        
        [Header("Generation Logic")]
        [SerializeField] private float minHeightOffset = -1f;
        [SerializeField] private float maxHeightOffset = 2f;
        [Range(0, 100)] [SerializeField] private float trapSpawnChance = 15f;
        
        [Header("Platform Settings")]
        [SerializeField] private Vector2 xSpawnRange = new Vector2(-2f, 2f);
        [SerializeField] private Vector2 distSpacingRange = new Vector2(4f, 7f);
        [SerializeField] private List<PlatformTypeWeight> platformWeights;

        private List<GameObject> generatedPlatforms = new List<GameObject>();

        private void Start()
        {
            if (Application.isPlaying) GenerateLevel();
        }

        private void Update()
        {
            // R לריסטארט רק באקראי
            if (Application.isPlaying && !GameSession.IsCampaignMode && Input.GetKeyDown(KeyCode.R))
            {
                GenerateLevel();
            }
        }

        [ContextMenu("🛠️ Generate Random Level (Editor)")]
        public void GenerateLevelInEditor()
        {
            GenerateRandomLevel();
        }

        [ContextMenu("💾 Save Level to JSON")]
        public void SaveCurrentLevel()
        {
            SaveLevelLogic();
        }

        public void GenerateLevel()
        {
            Debug.Log($"Generating Level... Mode: {(GameSession.IsCampaignMode ? "CAMPAIGN" : "RANDOM")}");

            if (GameSession.IsCampaignMode)
            {
                LoadCampaignLevel(GameSession.CurrentLevelIndex);
            }
            else
            {
                GenerateRandomLevel();
            }
        }

        private void LoadCampaignLevel(int index)
        {
            if (campaignData == null)
            {
                Debug.LogError("🚨 ERROR: Campaign Data SO is MISSING in LevelGenerator Inspector! Loading Random Level instead.");
                GenerateRandomLevel();
                return;
            }

            TextAsset levelJson = campaignData.GetLevelData(index);
            
            if (levelJson == null)
            {
                Debug.LogError($"🚨 ERROR: Level index {index} not found in Campaign SO list! Check if list is empty.");
                GenerateRandomLevel();
                return;
            }

            Debug.Log($"📂 Loading Campaign Level: {index + 1}");
            LoadLevelFromText(levelJson.text);
        }

        private void GenerateRandomLevel()
        {
            // --- התיקון שהוספנו: שימוש בסיד השמור ---
            if (!GameSession.IsCampaignMode)
            {
                Random.InitState(GameSession.RandomSeed);
            }
            // ----------------------------------------

            ClearLevel();
            float currentHeight = 0; float currentZ = 0; float currentX = 0;

            int keyIndex;
            
            if (levelLength <= 3)
            {
                keyIndex = 1; 
            }
            else
            {
                keyIndex = Random.Range(2, levelLength - 1);
            }

            if (keyIndex >= levelLength - 1) keyIndex = levelLength - 2;
            if (keyIndex < 1) keyIndex = 1;

            Debug.Log($"🔑 Key spawned at platform index: {keyIndex} (Level Length: {levelLength})");

            for (int i = 0; i < levelLength; i++)
            {
                if (i > 0)
                {
                    currentHeight += Mathf.Round(Random.Range(minHeightOffset, maxHeightOffset));
                    if (currentHeight > 10) currentHeight -= 2; if (currentHeight < -5) currentHeight += 2;
                    currentZ += Random.Range(distSpacingRange.x, distSpacingRange.y);
                    currentX = Random.Range(xSpawnRange.x, xSpawnRange.y);
                }

                int type = (i==0 || i==levelLength-1) ? 0 : GetWeightedRandomPlatformType();
                
                Vector3 pos = new Vector3(currentX, currentHeight, currentZ);
                GameObject plat = InstantiatePrefab(basePlatformPrefab, pos, Quaternion.identity);
                generatedPlatforms.Add(plat);
                AttachPlatformBehavior(plat, type, i);

                if (i == levelLength - 1) 
                {
                    SpawnItem(doorPrefab, plat, 1.5f);
                }
                else if (i == keyIndex) 
                {
                    SpawnItem(keyPrefab, plat, 1.5f);
                }
                else if (type == 0 && i > 1) 
                {
                    if (Random.Range(0, 100) < trapSpawnChance) SpawnItem(trapPrefab, plat, 0.5f);
                    else TrySpawnCoin(plat, type);
                }
            }
        }

        private void LoadLevelFromText(string jsonText)
        {
            SavedLevelData data = JsonUtility.FromJson<SavedLevelData>(jsonText);
            ClearLevel();
            foreach (var pData in data.platforms)
            {
                GameObject plat = InstantiatePrefab(basePlatformPrefab, pData.position, Quaternion.identity);
                if (plat == null) continue;
                plat.transform.localScale = pData.scale;
                generatedPlatforms.Add(plat);
                AttachBehaviorByType(plat, pData.typeIndex);

                if (pData.hasDoor) SpawnItem(doorPrefab, plat, 1.5f);
                if (pData.hasKey) SpawnItem(keyPrefab, plat, 1.5f);
                if (pData.hasTrap) SpawnItem(trapPrefab, plat, 0.5f);
                if (pData.hasCoin) SpawnItem(coinPrefab, plat, 1.5f);
            }
        }

        private void SaveLevelLogic() 
        {
            SavedLevelData levelData = new SavedLevelData();
            
            foreach (Transform child in transform)
            {
                GameObject plat = child.gameObject;
                if (!plat.activeInHierarchy) continue;

                SavedPlatformData pData = new SavedPlatformData();
                pData.position = plat.transform.position;
                pData.scale = plat.transform.localScale;
                pData.typeIndex = GetPlatformTypeIndex(plat);

                pData.hasCoin = plat.GetComponentInChildren<Coin>() != null;
                pData.hasTrap = plat.GetComponentInChildren<Trap>() != null;
                pData.hasKey = plat.GetComponentInChildren<Key>() != null;
                pData.hasDoor = plat.GetComponentInChildren<ExitDoor>() != null;

                levelData.platforms.Add(pData);
            }

            string dirPath = Application.dataPath + "/Resources/Levels";
            if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

            int fileIndex = 1;
            string fullPath = "";
            
            do 
            {
                fullPath = Path.Combine(dirPath, $"Level_{fileIndex}.json");
                fileIndex++;
            } 
            while (File.Exists(fullPath));

            levelData.levelId = fileIndex - 1;

            string json = JsonUtility.ToJson(levelData, true);
            File.WriteAllText(fullPath, json);
            
            Debug.Log($"✅ LEVEL SAVED: Level_{fileIndex - 1}.json");
            
#if UNITY_EDITOR
            UnityEditor.AssetDatabase.Refresh();
#endif
        }

        private GameObject InstantiatePrefab(GameObject prefab, Vector3 pos, Quaternion rot)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                GameObject obj = UnityEditor.PrefabUtility.InstantiatePrefab(prefab, transform) as GameObject;
                obj.transform.position = pos;
                obj.transform.rotation = rot;
                return obj;
            }
#endif
            return Instantiate(prefab, pos, rot, transform);
        }
        
        private void ClearLevel()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEditor.Selection.activeGameObject = this.gameObject;
            }
#endif
            if (transform.childCount > 0)
            {
                for (int i = transform.childCount - 1; i >= 0; i--)
                {
                    Transform childTransform = transform.GetChild(i);
                    if (childTransform == null) continue;

                    GameObject child = childTransform.gameObject;
                    
                    if (Application.isPlaying) 
                    {
                        Destroy(child);
                    }
                    else 
                    {
                        DestroyImmediate(child);
                    }
                }
            }
            generatedPlatforms.Clear();
        }

        private void SpawnItem(GameObject prefab, GameObject parent, float yOffset)
        {
            if (prefab == null) return;
            GameObject item = InstantiatePrefab(prefab, parent.transform.position + Vector3.up * yOffset, Quaternion.identity);
            item.transform.SetParent(parent.transform);
        }

        private void AttachPlatformBehavior(GameObject p, int type, int index) 
        {
            if (index == 0 || index == levelLength - 1) p.transform.localScale = new Vector3(4f, 0.3f, 4f);
            else p.transform.localScale = new Vector3(Random.Range(2f, 4f), 0.3f, 1);
            AttachBehaviorByType(p, type);
        }

        private void AttachBehaviorByType(GameObject p, int type) 
        {
            switch (type) {
                case 0: p.AddComponent<StandardPlatform>(); break;
                case 1: p.AddComponent<MovingPlatform>().InitializeWithSpeed(new Vector2(1,3)); break;
                case 2: p.AddComponent<GhostPlatform>(); break;
                case 4: p.AddComponent<BoostPlatform>(); break;
                case 5: p.AddComponent<IcePlatform>(); break;
            }
        }

        private int GetWeightedRandomPlatformType() 
        {
            if (platformWeights == null || platformWeights.Count == 0) 
            {
                Debug.LogWarning("Platform Weights list is empty! Generating only standard platforms.");
                return 0;
            }

            float totalWeight = 0f;
            foreach (var w in platformWeights) totalWeight += w.weight;

            float randomValue = Random.Range(0f, totalWeight);
            float currentWeight = 0f;

            foreach (var w in platformWeights)
            {
                currentWeight += w.weight;
                if (randomValue <= currentWeight) 
                {
                    return w.typeIndex;
                }
            }
            return 0;
        }

        private void TrySpawnCoin(GameObject platform, int platformType) 
        {
            if (coinPrefab == null) return;
            foreach (var w in platformWeights) 
            { 
                if (w.typeIndex == platformType) 
                { 
                    if (Random.Range(0f, 100f) < w.coinSpawnChance)
                        SpawnItem(coinPrefab, platform, 1.5f);
                    return; 
                } 
            }
        }
        
        private int GetPlatformTypeIndex(GameObject p) 
        {
            if (p.GetComponent<MovingPlatform>()) return 1;
            if (p.GetComponent<GhostPlatform>()) return 2;
            if (p.GetComponent<BoostPlatform>()) return 4;
            if (p.GetComponent<IcePlatform>()) return 5;
            return 0;
        }
    }
}