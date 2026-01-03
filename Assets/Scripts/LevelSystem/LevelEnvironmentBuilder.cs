using UnityEngine;

public class LevelEnvironmentBuilder : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] private GameObject wallPrefab; // קיר צד
    [SerializeField] private GameObject endWallPrefab; // קיר סגירה (אחורי וקדמי)

    [Header("Dimensions")]
    [SerializeField] private float wallXOffset = 4f;    // מרחק הקירות מהמרכז
    [SerializeField] private float segmentLength = 10f; // אורך מודל הקיר (Z)
    [SerializeField] private float backWallOffset = 10f; // כמה הקיר האחורי רחוק מההתחלה

    public void BuildEnvironment(float lastPlatformZ)
    {
        // 1. ניקוי סביבה קודמת (אם קיימת)
        foreach (Transform child in transform) {
            Destroy(child.gameObject);
        }

        // 2. חישוב טווח הבנייה
        float startZ = -backWallOffset; 
        float endZ = lastPlatformZ + 5f; // עוד קצת מרווח אחרי הדלת
        
        // 3. בניית קירות הצד (X=4 ו- X=-4)
        for (float z = startZ; z < endZ; z += segmentLength)
        {
            // ימין
            Instantiate(wallPrefab, new Vector3(wallXOffset, 0, z), Quaternion.Euler(0, -90, 0), transform);
            // שמאל
            Instantiate(wallPrefab, new Vector3(-wallXOffset, 0, z), Quaternion.Euler(0, 90, 0), transform);
        }

        // 4. בניית הקיר הקדמי (בסוף השלב)
        Vector3 frontWallPos = new Vector3(0, 0, endZ);
        Instantiate(endWallPrefab, frontWallPos, Quaternion.identity, transform);

        // 5. בניית הקיר האחורי (בתחילת השלב)
        Vector3 backWallPos = new Vector3(0, 0, startZ);
        Instantiate(endWallPrefab, backWallPos, Quaternion.Euler(0, 180, 0), transform);
        
        Debug.Log($"🏠 Environment built from {startZ} to {endZ}");
    }
}