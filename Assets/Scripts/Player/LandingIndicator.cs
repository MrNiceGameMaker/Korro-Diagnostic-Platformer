using UnityEngine;

public class LandingIndicator : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject shadowPrefab; // ה-Prefab של הצל
    [SerializeField] private LayerMask groundLayer;
    
    [Header("Distance Configuration")]
    [SerializeField] private float maxDistance = 5f;

    [Header("Scale Configuration")]
    [SerializeField] private float minScaleXZ = 0f;
    [SerializeField] private float maxScaleXZ = 1f;
    [SerializeField] private float fixedYScale = 0.1f;

    private GameObject shadowInstance;
    private Transform playerTransform;

    void Start()
    {
        playerTransform = transform;
        if (shadowPrefab != null)
        {
            shadowInstance = Instantiate(shadowPrefab);
            // מונע מהצל להופיע ברשימת האובייקטים הראשית כדי לא להעמיס
            shadowInstance.transform.SetParent(null); 
        }
    }

    void LateUpdate()
    {
        // בדיקת בטיחות: אם האובייקט נמחק מסיבה כלשהי, אל תמשיך
        if (shadowInstance == null) return;

        RaycastHit hit;
        if (Physics.Raycast(playerTransform.position, Vector3.down, out hit, 100f, groundLayer))
        {
            if (!shadowInstance.activeSelf) shadowInstance.SetActive(true);
            
            shadowInstance.transform.position = hit.point + new Vector3(0, 0.01f, 0);
            
            float distance = hit.distance;
            float t = Mathf.Clamp01(distance / maxDistance);
            float currentXZ = Mathf.Lerp(minScaleXZ, maxScaleXZ, t);
            
            shadowInstance.transform.localScale = new Vector3(currentXZ, fixedYScale, currentXZ);
        }
        else
        {
            if (shadowInstance.activeSelf) shadowInstance.SetActive(false);
        }
    }

    // חשוב מאוד למנוע שגיאות Inspector: מחיקת הצל כשהשחקן נמחק
    private void OnDestroy()
    {
        if (shadowInstance != null)
        {
            Destroy(shadowInstance);
        }
    }
}