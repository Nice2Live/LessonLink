using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using Cysharp.Threading.Tasks;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Globalization;
using UnityEditor;
using Unity.VisualScripting;

public class Lessons : MonoBehaviour
{
    public Animator animatorLoad, ChoiceLesson, lessons;
    public TMP_Text Logtext, errorText;
    public WebViewMos webviewmos;
    public WeekManager weekManager;
    public DayManager dayManager;
    public VerticalDragPanel swipePanel;
    public RefreshEntry refreshEntry;
    public BottonButtons bottonButtons;
    public SendToServer sendToServer;
    public Sprite NoneGrade;
    public GameObject lessonPanel;
    public List<Sprite> LessonIcons, LessonGrades;

    private static readonly HttpClient client = new HttpClient();
    private readonly List<string> DayLessonNumber = new()
    {
        "Monday_Lesson_", "Tuesday_Lesson_", "Wednesday_Lesson_", "Thursday_Lesson_", "Friday_Lesson_"
    };

    private CancellationTokenSource getLessonsCTS;
    private UniTask currentGetLessonsTask = default;
    private bool isGetLessonsRunning;
    private Guid currentTaskId = Guid.Empty;
    private bool currentGetLessonsTaskForgotten = true;

    private void Awake()
    {
        client.DefaultRequestHeaders.ConnectionClose = false; // включаем Keep-Alive
        client.DefaultRequestHeaders.ExpectContinue = false;  // чтобы не было 100-Continue задержек
        client.Timeout = TimeSpan.FromSeconds(2); // если хочешь, можешь снизить до 1-1.5 сек
    }
    public async UniTask GetLessons(bool show = false, bool cancel = false)
    {
        if (bottonButtons.PageNow() != 1) return;
        //Log("GetLessons called");

        if (isGetLessonsRunning)
        {
            CancelPreviousTask();
        }
        if (!cancel)
        {
            getLessonsCTS = new CancellationTokenSource();
            var token = getLessonsCTS.Token;
            currentTaskId = Guid.NewGuid();
            //Log("GetLessonsPreInternalGetLessons");

            currentGetLessonsTaskForgotten = false;
            currentGetLessonsTask = InternalGetLessons(show, token, currentTaskId);
            isGetLessonsRunning = true;

            ForgetSafely(currentGetLessonsTask, "новой задачи"); 
        }
    }

    private void CancelPreviousTask()
    {
        //Log("Cancelling previous GetLessons task");

        try
        {
            if (getLessonsCTS?.Token.IsCancellationRequested == false)
                getLessonsCTS.Cancel();

            if (!currentGetLessonsTaskForgotten && !currentGetLessonsTask.Equals(default(UniTask)))
                ForgetSafely(currentGetLessonsTask, "предыдущей задачи");

            currentGetLessonsTask = default;
            currentGetLessonsTaskForgotten = true;
        }
        catch (Exception ex)
        {
            //Log($"Ошибка при отмене: {ex.Message}\nСтек: {ex.StackTrace}");
        }
    }

    private void ForgetSafely(UniTask task, string context)
    {
        task.Forget(ex =>
        {
            //if (ex is not OperationCanceledException)
                //Log($"Исключение в {context}: {ex}");
        });
        currentGetLessonsTaskForgotten = true;
    }

    private async UniTask InternalGetLessons(bool load, CancellationToken token, Guid taskId)
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
            //Log("InternalGetLessons started");
            if (SecureStorage.Load("Entry") == "true")
            {
                if (load)
                {
                    animatorLoad?.SetTrigger("Load");
                    ChoiceLesson?.SetTrigger("Off");
                    lessons?.SetTrigger("Off");
                }

                token.ThrowIfCancellationRequested();

                var (lessonJson, marksJson) = await UniTask.WhenAll(
                GetJsonAsync(
                    $"https://school.mos.ru/api/family/web/v1/schedule?student_id={SecureStorage.Load("id")}&date={GetDate(-1)}",
                    token),
                GetJsonAsync(
                    $"https://dnevnik.mos.ru/reports/api/progress/json?academic_year_id=13&student_profile_id={SecureStorage.Load("id")}",
                    token, isMarks: true)
                    );

                token.ThrowIfCancellationRequested();
                //Log(lessonJson);
                if (await PushLessons(lessonJson, marksJson, load, token))
                {
                    token.ThrowIfCancellationRequested();
                    if (lessonJson != null && marksJson != null)
                    {
                        refreshEntry.Entry(cancel: true);
                        errorText.text = "";
                    }
                    else
                        ShowMeshError();

                    if (load)
                        CloseLoad();
                }
                else
                    ShowMeshError();
            }
        }
        catch (OperationCanceledException) { }//Log("InternalGetLessons cancelled");
        catch (HttpRequestException ex) when (ex.InnerException is SocketException)
        {
            //Log($"Ошибка в InternalGetLessons: {ex.Message}");
            if (load && await PushLessons(null, null, load, token))
                CloseLoad();
            errorText.text = "Не удалось получить данные с МЕШ";
            ShowMeshError();
        }
        catch (Exception ex)
        {
            //Log($"Ошибка в InternalGetLessons: {ex.Message}");
            if (load && await PushLessons(null, null, load, token))
                CloseLoad();
            errorText.text = "Не удалось получить данные с МЕШ";
            ShowMeshError();
        }
        finally
        {
            if (currentTaskId == taskId)
            {
                isGetLessonsRunning = false;
                currentGetLessonsTask = default;
                currentGetLessonsTaskForgotten = true;
                //Log("END");
            }
        }
    }

    private async UniTask<string> GetJsonAsync(string url, CancellationToken token, bool isMarks = false)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Dnevnik/4.37.0 (Android 13; Samsung SM-A525F) okhttp/4.9.3");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Auth-Token", SecureStorage.Load("AuthToken"));
            request.Headers.Add("Cookie", SecureStorage.Load("Cookies"));
            if (!isMarks) request.Headers.Add("X-Mes-Subsystem", "familyweb");
            else
            {
                request.Headers.Add("profile-type", "student");
                request.Headers.Add("Authorization", $"Bearer {SecureStorage.Load("AuthToken")}");
                request.Headers.Add("Profile-Id", SecureStorage.Load("id"));
            }

            var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token);
            string content = await response.Content.ReadAsStringAsync();
        
            if (!response.IsSuccessStatusCode)
            {
                //Log($"❌ Сервер вернул ошибку: {(int)response.StatusCode} {response.StatusCode}\nОтвет: {content}");
                return null;
            }
            return content;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
    }

    private void ShowMeshError()
    {
        //errorText.text = "Не удалось получить данные с МЕШ";
        refreshEntry.Entry(diary: true);
    }

    public async UniTask<bool> GetWeekLessons(CancellationToken token = default)
    {
        //Log("GetWeekLessons");

        var (d1, d2, d3, d4, d5) = await UniTask.WhenAll(
            InternalGetWeekLessons(0, token),
            InternalGetWeekLessons(1, token),
            InternalGetWeekLessons(2, token),
            InternalGetWeekLessons(3, token),
            InternalGetWeekLessons(4, token));

        if (!d1 || !d2 || !d3 || !d4 || !d5)
        {
            return false;
        }
        SecureStorage.ReSave("Entry", "true");
        return true;
    }

    private async UniTask<bool> InternalGetWeekLessons(int day, CancellationToken token = default)
    {
        try
        {
            token.ThrowIfCancellationRequested();

            int index = 0;

            var lessonJson = await GetJsonAsync(
                $"https://school.mos.ru/api/family/web/v1/schedule?student_id={SecureStorage.Load("id")}&date={GetDate(day)}",
                token);

            if (lessonJson != null)
            {
                SecureStorage.ReSave(DayLessonNumber[day] + "number_lessons", (JToken.Parse(lessonJson))["summary"]?.ToString()?.Split(' ')[0]);

                List<JToken> lessonsList = JsonToList(lessonJson, "activities");

                for (int i = 0; i <= lessonsList.Count - 1; i++)
                {
                    token.ThrowIfCancellationRequested();

                    JToken subject = lessonsList[i];

                    if (subject["lesson"]?.ToString() != null)
                    {
                        index += 1;

                        SecureStorage.ReSave(DayLessonNumber[day] + (index).ToString() + "_subject_name", subject["lesson"]?["subject_name"]?.ToString());
                        SecureStorage.ReSave(DayLessonNumber[day] + (index).ToString() + "_begin_time", subject["begin_time"]?.ToString());
                        SecureStorage.ReSave(DayLessonNumber[day] + (index).ToString() + "_end_time", subject["end_time"]?.ToString());
                        SecureStorage.ReSave(DayLessonNumber[day] + (index).ToString() + "_room_number", subject["room_number"]?.ToString());
                        SecureStorage.ReSave(DayLessonNumber[day] + (index).ToString() + "_last_name", subject["lesson"]?["teacher"]?["last_name"]?.ToString());
                        SecureStorage.ReSave(DayLessonNumber[day] + (index).ToString() + "_first_name", subject["lesson"]?["teacher"]?["first_name"]?.ToString());
                        SecureStorage.ReSave(DayLessonNumber[day] + (index).ToString() + "_middle_name", subject["lesson"]?["teacher"]?["middle_name"]?.ToString());

                        if (index == 8) break;
                    }
                }
                index = 0;
            }
            else
            {
                return false;
            }
            return true;
        }
        catch (HttpRequestException ex) when (ex.InnerException is SocketException)
        {
            if (token.CanBeCanceled)
            {
                //Log("Нет подключения к интернету в GetWeekLessons: " + ex.Message);
                ShowMeshError();
            }
            return false;
        }
        catch (Exception ex)
        {
            if (token.CanBeCanceled)
            {
                //Log("Ошибка в GetWeekLessons: " + ex.Message);
                ShowMeshError();
            }
            return false;
        }
    }

    async UniTask<bool> PushLessons(string lessonJson, string marksJson, bool load, CancellationToken token = default)
    {
        token.ThrowIfCancellationRequested();

        if (marksJson != null)
            sendToServer.Push(marksJson);

        token.ThrowIfCancellationRequested();

        int dayNow = dayManager.DayNow() - 1;

        int numberLessons = int.Parse(SecureStorage.Load(DayLessonNumber[dayNow] + "number_lessons"));
        if (numberLessons > 8) numberLessons = 8;

        List<int> OTtimeLessons = new(numberLessons);
        List<int> DOtimeLessons = new(numberLessons);

        token.ThrowIfCancellationRequested();

        var lessonObjects = new Transform[8];
        if (lessonPanel != null)
        {
            for (int i = 0; i < lessonObjects.Length; i++)
                lessonObjects[i] = lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}");
        }

        token.ThrowIfCancellationRequested();

        if (SecureStorage.Load(DayLessonNumber[dayNow] + "number_lessons") == null)
        {
            token.ThrowIfCancellationRequested();
            if (!(await GetWeekLessons(token))) return false;
        }

        token.ThrowIfCancellationRequested();

        string endTime = "00:05";

        List<JToken> lessonsList = null;
        if (lessonJson != null)
        {
            token.ThrowIfCancellationRequested();

            var lessonRoot = JToken.Parse(lessonJson);
            token.ThrowIfCancellationRequested();

            lessonsList = lessonRoot["activities"].ToObject<List<JToken>>();
            token.ThrowIfCancellationRequested();

            var summary = lessonRoot["summary"]?.ToString().Split(' ')[0];
            if (summary == "0" || summary == "1")
            {
                ShowMeshError();
                return false;
            }
            token.ThrowIfCancellationRequested();

            if (summary != SecureStorage.Load(DayLessonNumber[dayNow] + "number_lessons"))
                if (!(await GetWeekLessons(token))) return false;

            numberLessons = int.Parse(summary);
            if (numberLessons > 8) numberLessons = 8;
            if (load) swipePanel.NumLes(numberLessons, token);

            OTtimeLessons = new(numberLessons);
            DOtimeLessons = new(numberLessons);

            int index = 0;
            foreach (var subject in lessonsList)
            {
                token.ThrowIfCancellationRequested();

                string lesson = subject["lesson"]?["subject_name"]?.ToString();
                if (lesson == null) continue;

                if (weekManager.WeekNow() == 2 && !(bool.Parse(subject["lesson"]?["replaced"]?.ToString()))
                    && subject["lesson"]?["teacher"]?["last_name"].ToString() != SecureStorage.Load(DayLessonNumber[dayNow] + (index + 1).ToString() + "_last_name"))
                {
                    if (!(await GetWeekLessons(token))) return false;
                }

                token.ThrowIfCancellationRequested();

                Transform lessonObj = lessonObjects[index];
                if (lessonObj != null)
                {
                    token.ThrowIfCancellationRequested();

                    var hw = lessonObj.Find("BottomPanel/HomeWork");
                    if (hw) hw.GetComponent<TMP_Text>().text = subject["lesson"]?["homework"]?.ToString();
                    token.ThrowIfCancellationRequested();

                    if (!lessonObj) continue;

                    string beginTime = endTime;
                    endTime = subject["end_time"]?.ToString();

                    token.ThrowIfCancellationRequested();

                    int parsedBegin = int.Parse(beginTime.Replace(":", ""));
                    int parsedEnd = int.Parse(endTime.Replace(":", ""));
                    OTtimeLessons.Add(parsedBegin - 5);
                    DOtimeLessons.Add(parsedEnd - 5);
                    token.ThrowIfCancellationRequested();

                    string lessonName = subject["lesson"]?["subject_name"]?.ToString();
                    string roomNumber = subject["room_number"]?.ToString();

                    lessonObj.Find("TopPanel/LessonName")?.GetComponent<TMP_Text>().SetText(lessonName);
                    lessonObj.Find("TimePanel/OpenTime")?.GetComponent<TMP_Text>().SetText(subject["begin_time"]?.ToString());
                    lessonObj.Find("TimePanel/CloseTime")?.GetComponent<TMP_Text>().SetText(endTime);

                    if (roomNumber.Length > 0 && roomNumber.Substring(roomNumber.Length - 1, 1) == ".")
                        lessonObj.Find("TopPanel/ClassRoom")?.GetComponent<TMP_Text>().SetText(roomNumber.Substring(0, roomNumber.Length - 1));
                    else lessonObj.Find("TopPanel/ClassRoom")?.GetComponent<TMP_Text>().SetText(roomNumber);
                    token.ThrowIfCancellationRequested();

                    var teacher = $"{subject["lesson"]?["teacher"]?["first_name"]?.ToString()} " +
                                  $"{subject["lesson"]?["teacher"]?["middle_name"]?.ToString()} " +
                                  $"{subject["lesson"]?["teacher"]?["last_name"]?.ToString()}";

                    lessonObj.Find("TopPanel/TeacherName")?.GetComponent<TMP_Text>().SetText(teacher);
                    token.ThrowIfCancellationRequested();

                    string grade = subject["lesson"]?["marks"]?.FirstOrDefault()?["value"]?.ToString();
                    var grade1BG = lessonObj.Find("TopPanel/GradePanel/Grade/Grade1/GradeBG");
                    var grade1Text = lessonObj.Find("TopPanel/GradePanel/Grade/Grade1");
                    var grade2BG = lessonObj.Find("TopPanel/GradePanel/Grade/Grade2/GradeBG");
                    var grade2Text = lessonObj.Find("TopPanel/GradePanel/Grade/Grade2");
                    token.ThrowIfCancellationRequested();

                    if (!string.IsNullOrEmpty(grade))
                    {
                        if (grade1BG)
                        {
                            grade1BG.gameObject.SetActive(true);
                            grade1BG.GetComponent<Image>().sprite =
                            LessonGrades[Mathf.Clamp(int.Parse(grade) - 2, 0, LessonGrades.Count - 1)];
                        }
                        token.ThrowIfCancellationRequested();

                        if (grade1Text)
                        {
                            grade1Text.gameObject.SetActive(true);
                            var tmp = grade1Text.GetComponent<TMP_Text>();
                            tmp.text = grade;
                            tmp.color = grade switch
                            {
                                "2" => new Color(222f / 255f, 62f / 255f, 97f / 255f),
                                "3" => new Color(1f, 127f / 255f, 80f / 255f),
                                "4" => new Color(207f / 255f, 111f / 255f, 222f / 255f),
                                "5" => new Color(10f / 255f, 1f, 165f / 255f),
                                _ => tmp.color
                            };
                            token.ThrowIfCancellationRequested();
                        }
                    }
                    else
                    {
                        if (grade1BG) grade1BG.GetComponent<Image>().sprite = NoneGrade;
                        if (grade1Text) grade1Text.GetComponent<TMP_Text>().text = "";
                    }

                    if (grade2BG) grade2BG.GetComponent<Image>().sprite = NoneGrade;
                    if (grade2Text) grade2Text.GetComponent<TMP_Text>().text = "";

                    var averageMarksBG = lessonObj.Find("TopPanel/GradePanel/AverageGrade/Grade/GradeBG");
                    var averageMarksText = lessonObj.Find("TopPanel/GradePanel/AverageGrade/Grade");
                    if (averageMarksText) averageMarksText.GetComponent<TMP_Text>().text = "";
                    if (averageMarksBG) averageMarksBG.gameObject.SetActive(true);

                    token.ThrowIfCancellationRequested();

                    if (marksJson != null)
                    {
                        var marksArray = JArray.Parse(marksJson);
                        foreach (var mark in marksArray)
                        {
                            token.ThrowIfCancellationRequested();
                            
                            if (mark["subject_name"]?.ToString() == lessonName)
                            {
                                string averageMarks = mark["avg_five"]?.ToString();
                                if (float.TryParse(averageMarks, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatMarks) &&
                                    floatMarks > 0f && averageMarksText)
                                {
                                    var tmp = averageMarksText.GetComponent<TMP_Text>();
                                    tmp.text = averageMarks;
                                    if (averageMarksBG) averageMarksBG.gameObject.SetActive(false);

                                    tmp.color = floatMarks switch
                                    {
                                        >= 4.6f => new Color(10f / 255f, 1f, 165f / 255f),
                                        >= 3.6f => new Color(207f / 255f, 111f / 255f, 222f / 255f),
                                        >= 2.6f => new Color(1f, 127f / 255f, 80f / 255f),
                                        _ => new Color(222f / 255f, 62f / 255f, 97f / 255f)
                                    };
                                    token.ThrowIfCancellationRequested();
                                }
                            }
                        }
                        token.ThrowIfCancellationRequested();

                        Icons(lessonName, index);
                    }
                    index++;
                    if (index == 8) break;
                }
            }
            token.ThrowIfCancellationRequested();
        }
        else
        {
            token.ThrowIfCancellationRequested();
                
            if (load) swipePanel.NumLes(numberLessons, token);

            for (int i = 0; i < numberLessons; i++)
            {
                token.ThrowIfCancellationRequested();

                Transform lessonObj = lessonObjects[i];
                if (!lessonObj) continue;

                lessonObj.Find("BottomPanel/HomeWork").GetComponent<TMP_Text>().SetText("");
                lessonObj.Find("TopPanel/GradePanel/Grade/Grade1/GradeBG").GetComponent<Image>().sprite = NoneGrade;
                lessonObj.Find("TopPanel/GradePanel/Grade/Grade1").GetComponent<TMP_Text>().SetText("");
                lessonObj.Find("TopPanel/GradePanel/Grade/Grade2/GradeBG").GetComponent<Image>().sprite = NoneGrade;
                lessonObj.Find("TopPanel/GradePanel/Grade/Grade2").GetComponent<TMP_Text>().SetText("");

                token.ThrowIfCancellationRequested();

                string beginTime = endTime;
                endTime = SecureStorage.Load(DayLessonNumber[dayNow] + (i + 1) + "_end_time");
                token.ThrowIfCancellationRequested();

                int parsedBegin = int.Parse(beginTime.Replace(":", ""));
                int parsedEnd = int.Parse(endTime.Replace(":", ""));

                OTtimeLessons.Add(parsedBegin - 5);
                DOtimeLessons.Add(parsedEnd - 5);
                token.ThrowIfCancellationRequested();

                string lessonName = SecureStorage.Load(DayLessonNumber[dayNow] + (i + 1) + "_subject_name");
                string roomNumber = SecureStorage.Load(DayLessonNumber[dayNow] + (i + 1) + "_room_number");

                lessonObj.Find("TopPanel/LessonName")?.GetComponent<TMP_Text>().SetText(lessonName);
                lessonObj.Find("TimePanel/OpenTime")?.GetComponent<TMP_Text>().SetText(SecureStorage.Load(DayLessonNumber[dayNow] + (i + 1) + "_begin_time"));
                lessonObj.Find("TimePanel/CloseTime")?.GetComponent<TMP_Text>().SetText(endTime);

                if (roomNumber.Length > 0 && roomNumber.Substring(roomNumber.Length - 1, 1) == ".")
                    lessonObj.Find("TopPanel/ClassRoom")?.GetComponent<TMP_Text>().SetText(roomNumber.Substring(0, roomNumber.Length - 1));
                else lessonObj.Find("TopPanel/ClassRoom")?.GetComponent<TMP_Text>().SetText(roomNumber);
                token.ThrowIfCancellationRequested();

                var teacher = $"{SecureStorage.Load(DayLessonNumber[dayNow] + (i + 1) + "_first_name")} " +
                              $"{SecureStorage.Load(DayLessonNumber[dayNow] + (i + 1) + "_middle_name")} " +
                              $"{SecureStorage.Load(DayLessonNumber[dayNow] + (i + 1) + "_last_name")}";

                lessonObj.Find("TopPanel/TeacherName")?.GetComponent<TMP_Text>().SetText(teacher);
                token.ThrowIfCancellationRequested();

                var averageMarksBG = lessonObj.Find("TopPanel/GradePanel/AverageGrade/Grade/GradeBG");
                var averageMarksText = lessonObj.Find("TopPanel/GradePanel/AverageGrade/Grade");
                if (averageMarksText) averageMarksText.GetComponent<TMP_Text>().text = "";
                if (averageMarksBG) averageMarksBG.gameObject.SetActive(true);
                token.ThrowIfCancellationRequested();

                if (marksJson != null)
                {
                    var marksArray = JArray.Parse(marksJson);
                    foreach (var mark in marksArray)
                    {
                        token.ThrowIfCancellationRequested();

                        if (mark["subject_name"]?.ToString() == lessonName)
                        {
                            string averageMarks = mark["avg_five"]?.ToString();
                            if (float.TryParse(averageMarks, NumberStyles.Float, CultureInfo.InvariantCulture, out float floatMarks) &&
                                floatMarks > 0f && averageMarksText)
                            {
                                var tmp = averageMarksText.GetComponent<TMP_Text>();
                                tmp.text = averageMarks;
                                if (averageMarksBG) averageMarksBG.gameObject.SetActive(false);

                                tmp.color = floatMarks switch
                                {
                                    >= 4.6f => new Color(10f / 255f, 1f, 165f / 255f),
                                    >= 3.6f => new Color(207f / 255f, 111f / 255f, 222f / 255f),
                                    >= 2.6f => new Color(1f, 127f / 255f, 80f / 255f),
                                    _ => new Color(222f / 255f, 62f / 255f, 97f / 255f)
                                };
                                token.ThrowIfCancellationRequested();
                            }
                        }
                    }
                }
                token.ThrowIfCancellationRequested();

                Icons(lessonName, i);
            }
        }
        token.ThrowIfCancellationRequested();

        if (load)
            await dayManager.PushList(OTtimeLessons, DOtimeLessons);
        return true;
    }

    List<JToken> JsonToList(string json, string find)
    {
        return ((JArray)(JToken.Parse(json))[find]).ToObject<List<JToken>>();
    }

    void Icons(string icon, int i)
    {
        switch (icon)
        {
            case "Литература":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[4];
                break;
            case "Обществознание":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[13];
                break;
            case "Физика":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[11];
                break;
            case "Иностранный (английский) язык":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[6];
                break;
            case "Геометрия":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[2];
                break;
            case "Русский язык":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[3];
                break;
            case "География":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[9];
                break;
            case "История":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[10];
                break;
            case "Химия":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[12];
                break;
            case "Алгебра и начала математического анализа":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[1];
                break;
            case "Информатика":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[14];
                break;
            case "Биология":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[8];
                break;
            case "Физическая культура":
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[5];
                break;
            default:
                lessonPanel.transform.Find($"DragPanel/Lesson{i + 1}/TopPanel/LessonImage").GetComponent<Image>().sprite = LessonIcons[7];
                break;
        }
    }

    string GetDate(int day)
    {
        List<List<string>> masterList = new List<List<string>>();

        masterList.Add(weekManager.GetWeeks(1));
        masterList.Add(weekManager.GetWeeks(2));
        masterList.Add(weekManager.GetWeeks(3));

        if (day == -1) return masterList[weekManager.WeekNow() - 1][dayManager.DayNow() - 1];
        return masterList[1][day];
    }

    private void CloseLoad()
    {
        animatorLoad.SetTrigger("CloseLoad");
        animatorLoad.SetBool("Show", true);
    }

    public void Exit()
    {
        animatorLoad?.SetTrigger("Load");
        ChoiceLesson?.SetTrigger("Off");
        lessons?.SetTrigger("Off");
    }

    void Log(string s)
    {
        //Debug.Log(s);
        if (Logtext != null) Logtext.text += s + " ";
    }
}
