using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;
using System.Collections.Generic;

public class WebViewMos : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text logText;
    public Button entryBut;

    [Header("Reference")]
    public Cookies cookies;
    public AccInfo accinfo;
    public Lessons lessons;
    public ExitAcc exitAcc;

    private WebViewObject webView;
    private string startUrl = "https://school.mos.ru/v3/auth/sudir/login";
    public Animator animatorLoad;
    private bool hasCollectedCookie = false;
    private bool hasExit = false;
    private bool refresh_token = false;
    private bool LoadLesson = true;
    private bool refreshLessonNonInternet = true;

    void Awake()
    {
        entryBut.onClick.AddListener(() => Entry(true, false));
    }

    public void Exit()
    {
        startUrl = "https://school.mos.ru/v3/auth/logout";
        hasCollectedCookie = false;
        hasExit = true;
        Entry(false, false);
    }
    public void Entry(bool Load, bool refresh)
    {
        LoadLesson = Load;
        hasCollectedCookie = false;
        refresh_token = refresh;
        if (webView == null)
        {
            webView = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
            webView.Init(
                cb: async (msg) => { },
                err: async (msg) =>
                {
                    if (SecureStorage.Load("Entry") == "true")
                    {
                        if (refreshLessonNonInternet)
                        {
                            refreshLessonNonInternet = false;
                            await Task.Delay(5000);
                            lessons.GetLessons(false);
                            refreshLessonNonInternet = true;
                        }
                    }
                    else
                    {
                        await Task.Delay(5000);
                        Entry(false, true);
                    }    
                },
                started: (msg) => { },
                ld: OnPageLoaded
            );

            webView.SetMargins(0, 0, 0, 0);
        }
        if (Load)
        {
            webView.SetVisibility(true);
        }
        webView.LoadURL(startUrl);
        startUrl = "https://school.mos.ru/v3/auth/sudir/login";
    }

    private async void OnPageLoaded(string url)
    {
        if (!hasCollectedCookie && url.Contains("https://school.mos.ru/diary/"))
        {
            if (SecureStorage.Load("Entry") != "true")
            {
                entryBut.gameObject.SetActive(false);
                animatorLoad?.SetTrigger("Load");
                StartCoroutine(cookies.CollectAuthTokenRoutine());
                CloseWebView();
                if (!(await accinfo.GetAccInfo()))
                {
                    animatorLoad?.SetTrigger("CloseLoad");
                    entryBut.gameObject.SetActive(true);
                    return;
                }
                    
                if (!(await lessons.GetWeekLessons()))
                {
                    animatorLoad?.SetTrigger("CloseLoad");
                    entryBut.gameObject.SetActive(true);
                    return;
                }    
                entryBut.gameObject.SetActive(false);
                lessons.GetLessons(true);
                hasCollectedCookie = true;
            }
            else
            {
                StartCoroutine(cookies.CollectAuthTokenRoutine());
                if (refresh_token) { refresh_token = false; lessons.GetLessons(LoadLesson); }
                hasCollectedCookie = true;
                CloseWebView();
            }
        }
        if (hasExit && url.Contains("https://school.mos.ru/v3/auth/logout"))
        {
            await Task.Delay(3000);
            entryBut.interactable = true;
            hasExit = false;
            exitAcc.ExitWebView();
        }
    }

    public void CloseWebView()
    {
        if (webView != null)
        {
            Destroy(webView.gameObject);
            webView = null;
        }
    }

    void Log(string s)
    {
        Debug.Log(s);
        if (logText != null) logText.text += s + "\n";
    }
}
