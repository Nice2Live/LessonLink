using System;
using System.Collections;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Cookies : MonoBehaviour
{
    public TMP_Text logText;
    public WebViewMos webViewMos;
    private static readonly HttpClient client = new HttpClient();

    public IEnumerator CollectAuthTokenRoutine() // Изменено на public
    {
        SecureStorage.ReSave("Cookies", CookieUtils.MergeCookieStrings(CookieUtils.GetCookiesString("https://school.mos.ru/"), CookieUtils.GetCookiesString("https://dnevnik.mos.ru/")));
        SecureStorage.ReSave("AuthToken", ExtractTokenFromCookies(SecureStorage.Load("Cookies")));
        //Log(SecureStorage.Load("Cookies"));
        //Log($"[Token] from cookies: {SecureStorage.Load("AuthToken")}");
        yield break;
    }
    static string ExtractTokenFromCookies(string cookie)
    {
        // Возможные имена cookie с токеном
        var m = Regex.Match(cookie,
            @"(?:^|;\s*)(?:aupd_token)\s*=\s*([^;]+)",
            RegexOptions.IgnoreCase);
        return m.Success ? UnityWebRequestUnescape(m.Groups[1].Value) : null;
    }

    static string UnityWebRequestUnescape(string s)
    {
        try { return Uri.UnescapeDataString(s); } catch { return s; }
    }

    void Log(string s)
    {
        Debug.Log(s);
        if (logText != null) logText.text += s + "\n";
    }
}