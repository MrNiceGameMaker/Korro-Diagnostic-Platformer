using UnityEngine;
using UnityEngine.SceneManagement;
using KorroAI.Managers; // גישה ל-GameSession ולניהול המשחק

public class MainMenuController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string gameSceneName = "GameScene"; // וודא שזה השם המדויק של סצנת המשחק שלך

    private void Start()
    {
        // החזרת העכבר למצב נראה וחופשי לתפריטים
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void OnClick_StartCampaign()
{
    GameSession.ResetSession();
    GameSession.IsCampaignMode = true;
    
    // איפוס אנליטיקס בתחילת קמפיין חדש
    if (KorroAI.Analytics.AnalyticsManager.Instance != null)
        KorroAI.Analytics.AnalyticsManager.Instance.ResetAnalytics();

    SceneManager.LoadScene(gameSceneName);
}

public void OnClick_StartRandom()
{
    GameSession.ResetSession();
    GameSession.IsCampaignMode = false;
    GameSession.GenerateNewRandomSeed();
    
    // איפוס אנליטיקס בתחילת ריצה אקראית חדשה
    if (KorroAI.Analytics.AnalyticsManager.Instance != null)
        KorroAI.Analytics.AnalyticsManager.Instance.ResetAnalytics();

    SceneManager.LoadScene(gameSceneName);
}

    public void OnClick_Quit()
    {
        Debug.Log("👋 Quitting Game");
        Application.Quit();
    }
}