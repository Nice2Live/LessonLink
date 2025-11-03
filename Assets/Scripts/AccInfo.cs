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
    //public TMP_Text Logtext;
    public List<TMP_Text> accInfo;
    public List<Sprite> icons;
    public Image image;
    public WebViewMos webviewmos;
    public RefreshEntry refreshEntry;
    private static readonly HttpClient client = new HttpClient();

    void Awake()
    {   
        if (SecureStorage.Load("Entry") == "true")
        {
            PushAccInfo();
            GetAccInfo();
        }
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
                    refreshEntry.Entry(accInfo: true);
                    return false;
                }
            }
            else
            {
                refreshEntry.Entry(accInfo: true);
                return false;
            }
        }
        catch (Exception ex)
        {
            refreshEntry.Entry(accInfo: true);
            return false;
        }

        return true;
    }
    private void PushAccInfo()
    {
        accInfo[0].text = SecureStorage.Load("last_name") + " " + SecureStorage.Load("first_name");
        accInfo[1].text = SecureStorage.Load("short_name_school") + " " + SecureStorage.Load("class_name");
        if (SecureStorage.Load("sex") == "male")
            image.sprite = icons[0];
        else
            image.sprite = icons[1];
    }
    // void Log(string s)
    // {
    //     Debug.Log(s);
    //     if (Logtext != null) Logtext.text += s + "\n";
    // }
}
