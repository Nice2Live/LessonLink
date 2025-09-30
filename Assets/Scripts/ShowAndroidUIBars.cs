using UnityEngine;


public class ShowAndroidUIBars : MonoBehaviour
{
    void Awake()
    {
        Input.backButtonLeavesApp = true;
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                AndroidJavaObject window = activity.Call<AndroidJavaObject>("getWindow");
                AndroidJavaObject decorView = window.Call<AndroidJavaObject>("getDecorView");

                // SYSTEM_UI_FLAG_VISIBLE = 0
                int flags = 0;
                decorView.Call("setSystemUiVisibility", flags);
            }
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                var window = activity.Call<AndroidJavaObject>("getWindow");

                // Цвет статус-бара (верхняя панель)
                window.Call("setStatusBarColor", ToAndroidColor(new Color(13f / 255f, 18f / 255f, 30f / 255f))); 

                // Цвет навигационной панели (нижняя панель)
                window.Call("setNavigationBarColor", ToAndroidColor(new Color(13f / 255f, 18f / 255f, 30f / 255f)));
            }
        }
        catch (System.Exception e)
        {
            Debug.Log("Не удалось показать системные UI: " + e.Message);
        }
#endif
    }

    int ToAndroidColor(Color color)
    {
        int a = Mathf.RoundToInt(color.a * 255f) << 24;
        int r = Mathf.RoundToInt(color.r * 255f) << 16;
        int g = Mathf.RoundToInt(color.g * 255f) << 8;
        int b = Mathf.RoundToInt(color.b * 255f);
        return a | r | g | b;
    }
}
