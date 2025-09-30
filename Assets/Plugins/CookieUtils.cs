// CookieUtils.cs
// Утилиты для чтения cookie из нативного WebView (Android/iOS)
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

public static class CookieUtils
{
    /// Читает cookie-строку для домена/URL, где открыт WebView.
    public static string GetCookiesString(string urlOrDomain)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            using (var cookieManagerClass = new AndroidJavaClass("android.webkit.CookieManager"))
            {
                var cookieManager = cookieManagerClass.CallStatic<AndroidJavaObject>("getInstance");
                string s = cookieManager.Call<string>("getCookie", urlOrDomain);
                if (string.IsNullOrEmpty(s))
                {
                    // Попробуем без схемы
                    var host = urlOrDomain.Replace("https://", "").Replace("http://", "");
                    s = cookieManager.Call<string>("getCookie", host);
                }
                return s ?? "";
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning("[CookieUtils] Android getCookie failed: " + ex.Message);
            return "";
        }
#elif UNITY_IOS && !UNITY_EDITOR
        // UniWebViewCookieManager или собственные плагины — здесь оставим заглушку
        // В большинстве кейсов достаточно Android. Для iOS верни пусто, если нет своего менеджера.
        return "";
#else
        // В редакторе/других платформах доступа к нативным куки нет.
        return "";
#endif
    }

    /// Мерж двух cookie-строк вида "k=v; a=b"
    public static string MergeCookieStrings(string a, string b)
    {
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        void parse(string s)
        {
            if (string.IsNullOrEmpty(s)) return;
            var parts = s.Split(new char[] {';'}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var p in parts)
            {
                var t = p.Trim();
                if (t.Length == 0) continue;
                var idx = t.IndexOf('=');
                if (idx <= 0) continue;
                var key = t.Substring(0, idx).Trim();
                var val = t.Substring(idx + 1);
                dict[key] = val;
            }
        }
        parse(a);
        parse(b);

        var list = new List<string>();
        foreach (var kv in dict) list.Add(kv.Key + "=" + kv.Value);
        return string.Join("; ", list);
    }
}
