using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;


public class AccInfo : MonoBehaviour
{
    public TMP_Text logText1;
    public TMP_Text logText2;
    public TMP_Text logText3;
    public TMP_Text Logtext;
    public TMP_Text errorText;
    public Image image;
    public Sprite spritemale;
    public Sprite spritefemale;
    public WebViewMos webviewmos;
    private string academic_year_id;

    private static readonly HttpClient client = new HttpClient();

    void Awake()
    {
        PushAccInfo();
        GetAccInfo();
        PushAccInfo();
    }
    public async Task<bool> GetAccInfo()
    {
        try
        {
            if (SecureStorage.Load("Cookies") != null)
            {
                string profileUrl = $"https://school.mos.ru/api/family/web/v1/profile";
                var profile = new HttpRequestMessage(HttpMethod.Get, profileUrl);
                profile.Headers.Add("Auth-Token", SecureStorage.Load("AuthToken"));
                profile.Headers.Add("X-Mes-Subsystem", "familyweb");
                profile.Headers.Add("Cookie", SecureStorage.Load("Cookies"));
                profile.Headers.Add("User-Agent", "Mozilla/5.0 (Linux; Android 12; Mobile) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/124.0 Mobile Safari/537.36");
                profile.Headers.Add("Accept", "application/json");

                var profileResponce = await client.SendAsync(profile);
                var profileJson = await profileResponce.Content.ReadAsStringAsync();

                if (profileResponce.IsSuccessStatusCode)
                {
                    JToken root = JToken.Parse(profileJson);
                    SecureStorage.ReSave("id", root["profile"]?["id"]?.ToString());
                    SecureStorage.ReSave("last_name", root["profile"]?["last_name"]?.ToString());
                    SecureStorage.ReSave("first_name", root["profile"]?["first_name"]?.ToString());
                    SecureStorage.ReSave("sex", root["profile"]?["sex"]?.ToString());

                    JToken profileroot = JToken.Parse(profileJson);
                    JArray profileArray = (JArray)root["children"];
                    List<JToken> profileList = profileArray.ToObject<List<JToken>>();
                    JToken accList = profileList[0];

                    SecureStorage.ReSave("short_name_school", accList["school"]?["short_name"].ToString());
                    SecureStorage.ReSave("class_name", accList["class_name"].ToString());
                    PushAccInfo();
                }
                else
                {
                    errorText.text = "Не удалось получить данные с МЕШ";

                    webviewmos.Entry(false, true);
                    return false;
                }
            }
            else
            {
                errorText.text = "Не удалось получить данные с МЕШ";

                webviewmos.Entry(false, true);
                return false;
            }
        }
        catch (Exception ex)
        {
            errorText.text = "Не удалось получить данные с МЕШ";

            webviewmos.Entry(false, true);
            return false;
        }

        return true;
    }
    void PushAccInfo()
    {
        logText1.text = SecureStorage.Load("last_name") + " " + SecureStorage.Load("first_name");
        logText2.text = SecureStorage.Load("short_name_school");
        logText3.text = "Класс " + SecureStorage.Load("class_name");
        switch (SecureStorage.Load("sex"))
        {
            case "male": image.sprite = spritemale;   break;
            default:     image.sprite = spritefemale; break;
        }
    }
    void Log(string s)
    {
        Debug.Log(s);
        if (Logtext != null) Logtext.text += s + "\n";
    }
}
