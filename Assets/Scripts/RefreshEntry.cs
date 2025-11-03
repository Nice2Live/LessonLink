using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using TMPro;
using Unity.VisualScripting;
using System.Xml;

public class RefreshEntry : MonoBehaviour
{
    public WebViewMos webviewmos;
    public TMP_Text logtext, errorText;
    private CancellationTokenSource getLessonsCTS;
    private UniTask currentGetLessonsTask = default;
    private bool isGetLessonsRunning;
    private Guid currentTaskId = Guid.Empty;
    private bool currentGetLessonsTaskForgotten = true;
    public async UniTask Entry(string Url = "https://school.mos.ru/v3/auth/sudir/login", bool cancel = false, bool diary = false, bool grade = false, bool profile = false, bool showDiary = false, bool accInfo = false, bool exit = false)
    {
        if (isGetLessonsRunning)
            CancelPreviousTask();
        getLessonsCTS = new CancellationTokenSource();
        var token = getLessonsCTS.Token;
        currentTaskId = Guid.NewGuid();
            
        if (!cancel)
        {
            currentGetLessonsTaskForgotten = false;
            currentGetLessonsTask = InternalEnrty(Url, diary, grade, profile, showDiary, accInfo, exit, token, currentTaskId);
            isGetLessonsRunning = true;

            ForgetSafely(currentGetLessonsTask, "новой задачи");
        }
    }

    private void CancelPreviousTask()
    {
        try
        {
            if (getLessonsCTS?.Token.IsCancellationRequested == false)
                getLessonsCTS.Cancel();

            if (!currentGetLessonsTaskForgotten && !currentGetLessonsTask.Equals(default(UniTask)))
                ForgetSafely(currentGetLessonsTask, "предыдущей задачи");

            currentGetLessonsTask = default;
            currentGetLessonsTaskForgotten = true;
        }
        catch (Exception ex) { }
    }

    private void ForgetSafely(UniTask task, string context)
    {
        task.Forget(ex => {});
        currentGetLessonsTaskForgotten = true;
    }

    private async UniTask InternalEnrty(string Url, bool diary, bool grade, bool profile, bool showDiary, bool accInfo, bool exit, CancellationToken token, Guid taskId)
    {
        if (SecureStorage.Load("Entry") != "true")
        {
            if (currentTaskId == taskId)
            {
                isGetLessonsRunning = false;
                currentGetLessonsTask = default;
                currentGetLessonsTaskForgotten = true;
            }
        }

        try
        {
            while (true)
            {
                token.ThrowIfCancellationRequested();
                webviewmos.RefreshToken(Url:Url, diary: diary, grade: grade, profile: profile, showDiary: showDiary, accInfo: accInfo, exit: exit);
                await Task.Delay(7500);
                token.ThrowIfCancellationRequested();
                if (SecureStorage.Load("Entry") != "true") break;
                errorText.text = "Не удалось получить данные с МЕШ";
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception ex) { }
        finally
        {
            if (currentTaskId == taskId)
            {
                isGetLessonsRunning = false;
                currentGetLessonsTask = default;
                currentGetLessonsTaskForgotten = true;
            }
        }
    }
    void Log(string s)
    {
        Debug.Log(s);
        if (logtext != null) logtext.text += s + "\n";
    }
}
