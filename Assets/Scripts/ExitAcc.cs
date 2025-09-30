using UnityEngine;
using UnityEngine.UI;

public class ExitAcc : MonoBehaviour
{
    public ProfilePanelToggle profileManager;
    public Lessons lessons;
    public Button entryBut;
    public WebViewMos webViewMos;
    void Awake()
    {
        if (SecureStorage.Load("Entry") != "true")
        {
            entryBut.gameObject.SetActive(true);
        }
    }

    public void Exit()
    {
        if (SecureStorage.Load("Entry") == "true")
        {
            entryBut.gameObject.SetActive(true);
            entryBut.interactable = false;
            lessons.Exit();
            profileManager.Toggle();
            webViewMos.Exit();
        }
    }
    public void ExitWebView()
    {
        PlayerPrefs.DeleteKey("WebView");
        PlayerPrefs.DeleteKey("Cookies");
        PlayerPrefs.DeleteKey("id");
        PlayerPrefs.DeleteKey("AuthToken");
        PlayerPrefs.DeleteKey("last_name");
        PlayerPrefs.DeleteKey("first_name");
        PlayerPrefs.DeleteKey("sex");
        PlayerPrefs.DeleteKey("short_name_school");
        PlayerPrefs.DeleteKey("class_name");
        PlayerPrefs.DeleteKey("last_name");
        PlayerPrefs.DeleteKey("Week");
        SecureStorage.ReSave("Entry", "false");    
    }
}
