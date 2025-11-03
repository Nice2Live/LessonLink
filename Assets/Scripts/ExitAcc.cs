using UnityEngine;
using UnityEngine.UI;

public class ExitAcc : MonoBehaviour
{
    public Lessons lessons;
    public Button entryBut;
    public WebViewMos webViewMos;
    public RefreshEntry refreshEntry;
    public BottonButtons bottonButtons;
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
            lessons.Exit();
            Clear();
            bottonButtons.Toggle(1);
            refreshEntry.Entry(Url: "https://school.mos.ru/v3/auth/logout", exit: true);
             
        }
    }
    private void Clear()
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