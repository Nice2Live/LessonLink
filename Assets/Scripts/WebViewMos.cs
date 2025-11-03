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
    public Grades grades;
    public BottonButtons bottonButtons;

    private WebViewObject webView;
    public Animator animatorLoad;
    private bool hasCollectedCookie = false;
    // private bool hasExit = false;

    void Awake()
    {
        entryBut.onClick.AddListener(() => Entry());
    }
    public void RefreshToken(string Url = "https://school.mos.ru/v3/auth/sudir/login", bool diary = false, bool grade = false, bool profile = false, bool showDiary = false, bool accInfo = false, bool exit = false)
    {
        hasCollectedCookie = false;
        CloseWebView();
        if (webView == null)
            CreateWebView(diary: diary, grade: grade, profile: profile, showDiary: showDiary, accInfo: accInfo, exit: exit);
        webView.LoadURL(Url);
    }
    public void Entry()
    {
        hasCollectedCookie = false;
        if (webView == null)
            CreateWebView();
        webView.LoadURL("https://school.mos.ru/v3/auth/sudir/login");
        webView.SetVisibility(true);
    }
    private void CreateWebView(bool diary = false, bool grade = false, bool profile = false, bool showDiary = false, bool accInfo = false, bool exit = false)
    {
        webView = (new GameObject("WebViewObject")).AddComponent<WebViewObject>();
        webView.Init(
            cb: async (msg) => { },
            err: async (msg) => { },
            started: (msg) => { },
            ld: (url) => OnPageLoaded(url, diary, grade, profile, showDiary, accInfo, exit)
        );
        webView.SetMargins(0, 0, 0, 0);
    }

    private async void OnPageLoaded(string url, bool diary, bool grade, bool profile, bool showDiary, bool accInfo, bool exit)
    {
        if (!hasCollectedCookie && url.Contains("https://school.mos.ru/diary/"))
        {
            if (SecureStorage.Load("Entry") != "true")
            {
                entryBut.gameObject.SetActive(false);
                animatorLoad?.SetTrigger("Load");
                StartCoroutine(cookies.CollectAuthTokenRoutine());
                CloseWebView();
                //Log("Entry1");
                if (!(await accinfo.GetAccInfo()))
                {
                    animatorLoad?.SetTrigger("CloseLoad");
                    entryBut.gameObject.SetActive(true);
                    return;
                }
                //Log("Entry2");
                if (!(await lessons.GetWeekLessons()))
                {
                    animatorLoad?.SetTrigger("CloseLoad");
                    entryBut.gameObject.SetActive(true);
                    return;
                }
                //Log("Entry3");
                entryBut.gameObject.SetActive(false);
                bottonButtons.Toggle(1);
                hasCollectedCookie = true;
                //Log("Entry4");
                if (diary)
                    lessons.GetLessons(true);
                return;
            }
            
            StartCoroutine(cookies.CollectAuthTokenRoutine());
            if (diary)
                lessons.GetLessons(showDiary);
            if (accInfo)
                accinfo.GetAccInfo();
            if (grade)
                grades.GetGrades(true);
            hasCollectedCookie = true;
            CloseWebView();
        }
        if (exit && url.Contains("https://school.mos.ru/"))
        {
            animatorLoad.SetTrigger("CloseLoad");
            await Task.Delay(100);
            entryBut.gameObject.SetActive(true);
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
