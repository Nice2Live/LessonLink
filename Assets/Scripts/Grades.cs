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
using NUnit.Framework.Constraints;
using Cysharp.Threading.Tasks.Triggers;
using System.Threading.Tasks;

public class Grades : MonoBehaviour
{
    public WeekManager weekManager;
    public Animator animatorLoad;
    public TMP_Text Logtext, errorText;
    public RefreshEntry refreshEntry;
    public BottonButtons bottonButtons;
    public DayManager dayManager;
    public List<Sprite> sprites;
    public SwipeGrade swipeGrade;
    public Animator animatorGradePanels;
    public SendToServer sendToServer;
    public ChoiceGrade choiceGrade;

    private static readonly HttpClient client = new HttpClient();

    private CancellationTokenSource getLessonsCTS;
    private UniTask currentGetLessonsTask = default;
    private bool isGetLessonsRunning;
    private Guid currentTaskId = Guid.Empty;
    private bool currentGetLessonsTaskForgotten = true;
    

    List<float> GradePanelList;
    List<float> objList;
    List<float> AveradeGradeIconList;
    List<float> AveradeGradeIconAvgTextList;
    List<float> LessonNameList;
    List<float> AveradeGradesList;
    List<float> GradesInfoList;


    List<float> GradesPanelList;
    List<float> GradeIconList;
    List<float> GradeTextList;
    List<float> GradeLessonNameList;
    float HeightGradeText;

    private void Awake()
    {
        Transform obj = transform.Find("SwipePanel/AverageGradePanel");
        RectTransform objRect = obj.GetComponent<RectTransform>();
        objList = new List<float>() { objRect.rect.width, objRect.rect.height };

        RectTransform AveradeGradeIconRect = obj.Find("AverageGradeIcon").GetComponent<RectTransform>();
        AveradeGradeIconList = new List<float>() {AveradeGradeIconRect.offsetMin.x, AveradeGradeIconRect.offsetMin.y,
            AveradeGradeIconRect.offsetMax.x, AveradeGradeIconRect.offsetMax.y};

        RectTransform AveradeGradeIconAvgTextRect = obj.Find("AverageGradeIcon/AvgText").GetComponent<RectTransform>();
        AveradeGradeIconAvgTextList = new List<float>() { AveradeGradeIconAvgTextRect.offsetMin.y };

        RectTransform LessonNameRect = obj.Find("LessonName").GetComponent<RectTransform>();
        LessonNameList = new List<float>() {LessonNameRect.offsetMin.x, LessonNameRect.offsetMin.y,
            LessonNameRect.offsetMax.x, LessonNameRect.offsetMax.y};

        RectTransform AveradeGradesRect = obj.Find("AverageGrades").GetComponent<RectTransform>();
        AveradeGradesList = new List<float>() {AveradeGradesRect.offsetMin.x, AveradeGradesRect.offsetMin.y,
            AveradeGradesRect.offsetMax.x, AveradeGradesRect.offsetMax.y};

        RectTransform AveradeGradesRect1 = obj.Find("AverageGrades/Grade").GetComponent<RectTransform>();
        RectTransform AveradeGradesRect2 = obj.Find("AverageGrades/Grade1").GetComponent<RectTransform>();
        GradesInfoList = new List<float>() {(Math.Abs(AveradeGradesRect1.anchoredPosition.x) - Math.Abs(AveradeGradesRect2.anchoredPosition.x)) / 2f + AveradeGradesRect1.rect.height,
            AveradeGradesRect1.rect.height * 1.2f, AveradeGradesRect1.rect.width, AveradeGradesRect1.anchoredPosition.x};

        RectTransform GradePanel = transform.Find("SwipePanel/AverageGradePanel").GetComponent<RectTransform>();
        GradePanelList = new List<float>() { GradePanel.rect.width, GradePanel.rect.height };



        HeightGradeText = transform.Find("SwipePanel/GradePanelText").GetComponent<RectTransform>().rect.height;

        Transform Grade = transform.Find("SwipePanel/GradePanel");
        RectTransform GradeRect = Grade.GetComponent<RectTransform>();
        GradesPanelList = new List<float>() { GradeRect.rect.width, GradeRect.rect.height };

        RectTransform GradeIconRect = Grade.Find("GradeIcon").GetComponent<RectTransform>();
        GradeIconList = new List<float>() {GradeIconRect.offsetMin.x, GradeIconRect.offsetMin.y,
            GradeIconRect.offsetMax.x, GradeIconRect.offsetMax.y};

        RectTransform GradeLessonNameRect = Grade.Find("LessonName").GetComponent<RectTransform>();
        GradeLessonNameList = new List<float>() {GradeLessonNameRect.offsetMin.x, GradeLessonNameRect.offsetMin.y,
            GradeLessonNameRect.offsetMax.x, GradeLessonNameRect.offsetMax.y};

        RectTransform GradesRect = Grade.Find("GradeText").GetComponent<RectTransform>();
        GradeTextList = new List<float>() {GradesRect.offsetMin.x, GradesRect.offsetMin.y,
            GradesRect.offsetMax.x, GradesRect.offsetMax.y};



        client.DefaultRequestHeaders.ConnectionClose = false; // включаем Keep-Alive
        client.DefaultRequestHeaders.ExpectContinue = false;  // чтобы не было 100-Continue задержек
        client.Timeout = TimeSpan.FromSeconds(2); // если хочешь, можешь снизить до 1-1.5 сек
    }
    async UniTask Start()
    {
        //sendToServer.Push(await GetJsonAsync("https://school.mos.ru/api/family/web/v1/marks?student_id=36239136&from=2025-10-13&to=2025-10-19", isMarks: false));
        //CreateGrade(1, transform.Find("SwipePanel").GetComponent<RectTransform>());
        //Log((await GetJsonAsync("https://dnevnik.mos.ru/core/api/marks?created_at_from=01-10-2025&created_at_to=10-10-2025&student_profile_id=44246210", isMarks: true)).ToString());
        //Log((await GetJsonAsync("https://school.mos.ru/api/family/web/v1/marks?student_id=36239136&from=2025-10-13&to=2025-10-19", isMarks: false)).ToString());
        //Log((await GetJsonAsync($"https://dnevnik.mos.ru/core/api/marks?created_at_from={DateTime.ParseExact(GetDate(-1), "yyyy-MM-dd", null).ToString("dd-MM-yyyy")}&created_at_to={DateTime.ParseExact(GetDate(1), "yyyy-MM-dd", null).ToString("dd-MM-yyyy")}&student_profile_id={SecureStorage.Load("id")}", isMarks: true)).ToString());
        
        //string json = "{\"payload\":[{\"id\":2891210365,\"value\":\"5\",\"values\":[{\"name\":\"5-балльная\",\"nmax\":6.0,\"grade\":{\"origin\":\"5\",\"five\":5.0,\"ten\":10.0,\"hundred\":100.0},\"grade_system_id\":4140720,\"grade_system_type\":\"five\"}],\"comment\":\"\",\"weight\":1,\"point_date\":null,\"control_form_name\":\"Словарный ассоциативный ряд\",\"comment_exists\":false,\"created_at\":\"2025-10-15T12:03:00\",\"updated_at\":\"2025-10-15T12:03:00\",\"criteria\":null,\"date\":\"2025-10-13\",\"subject_name\":\"Иностранный (английский) язык\",\"subject_id\":37175860,\"has_files\":false,\"is_point\":false,\"is_exam\":false,\"original_grade_system_type\":\"five\"},{\"id\":2890915137,\"value\":\"4\",\"values\":[{\"name\":\"5-балльная\",\"nmax\":6.0,\"grade\":{\"origin\":\"4\",\"five\":4.0,\"ten\":8.0,\"hundred\":80.0},\"grade_system_id\":4140720,\"grade_system_type\":\"five\"}],\"comment\":\"3 закон Ньютона\",\"weight\":1,\"point_date\":null,\"control_form_name\":\"Терминологический диктант\",\"comment_exists\":true,\"created_at\":\"2025-10-15T11:03:00\",\"updated_at\":\"2025-10-15T11:03:00\",\"criteria\":null,\"date\":\"2025-10-15\",\"subject_name\":\"Физика\",\"subject_id\":33623584,\"has_files\":false,\"is_point\":false,\"is_exam\":false,\"original_grade_system_type\":\"five\"},{\"id\":2890218832,\"value\":\"4\",\"values\":[{\"name\":\"5-балльная\",\"nmax\":6.0,\"grade\":{\"origin\":\"4\",\"five\":4.0,\"ten\":8.0,\"hundred\":80.0},\"grade_system_id\":4140720,\"grade_system_type\":\"five\"}],\"comment\":\"\",\"weight\":2,\"point_date\":null,\"control_form_name\":\"Контрольная работа\",\"comment_exists\":false,\"created_at\":\"2025-10-15T08:03:00\",\"updated_at\":\"2025-10-15T08:03:00\",\"criteria\":null,\"date\":\"2025-10-15\",\"subject_name\":\"Информатика\",\"subject_id\":33623623,\"has_files\":true,\"is_point\":false,\"is_exam\":true,\"original_grade_system_type\":\"five\"},{\"id\":2891365378,\"value\":\"2\",\"values\":[{\"name\":\"5-балльная\",\"nmax\":6.0,\"grade\":{\"origin\":\"2\",\"five\":2.0,\"ten\":4.0,\"hundred\":30.0},\"grade_system_id\":4140720,\"grade_system_type\":\"five\"}],\"comment\":\"1 верное из 5\",\"weight\":1,\"point_date\":null,\"control_form_name\":\"Тест\",\"comment_exists\":true,\"created_at\":\"2025-10-15T12:32:00\",\"updated_at\":\"2025-10-15T12:32:00\",\"criteria\":null,\"date\":\"2025-10-15\",\"subject_name\":\"Геометрия\",\"subject_id\":33623650,\"has_files\":false,\"is_point\":false,\"is_exam\":false,\"original_grade_system_type\":\"five\"},{\"id\":2893109663,\"value\":\"5\",\"values\":[{\"name\":\"5-балльная шкала\",\"nmax\":6.0,\"grade\":{\"origin\":\"5\",\"five\":5.0,\"ten\":10.0,\"hundred\":100.0},\"grade_system_id\":3673212,\"grade_system_type\":\"five\"}],\"comment\":\"\",\"weight\":1,\"point_date\":null,\"control_form_name\":\"Цифровое домашнее задание\",\"comment_exists\":false,\"created_at\":\"2025-10-16T09:23:00\",\"updated_at\":\"2025-10-16T09:23:00\",\"criteria\":null,\"date\":\"2025-10-15\",\"subject_name\":\"Химия\",\"subject_id\":33623577,\"has_files\":false,\"is_point\":false,\"is_exam\":false,\"original_grade_system_type\":\"five\"},{\"id\":2893121827,\"value\":\"5\",\"values\":[{\"name\":\"5-балльная шкала\",\"nmax\":6.0,\"grade\":{\"origin\":\"5\",\"five\":5.0,\"ten\":10.0,\"hundred\":100.0},\"grade_system_id\":3673214,\"grade_system_type\":\"five\"}],\"comment\":\"\",\"weight\":1,\"point_date\":null,\"control_form_name\":\"Цифровое домашнее задание\",\"comment_exists\":false,\"created_at\":\"2025-10-16T09:27:00\",\"updated_at\":\"2025-10-16T09:27:00\",\"criteria\":null,\"date\":\"2025-10-15\",\"subject_name\":\"Биология\",\"subject_id\":33623636,\"has_files\":false,\"is_point\":false,\"is_exam\":false,\"original_grade_system_type\":\"five\"},{\"id\":2893945042,\"value\":\"5\",\"values\":[{\"name\":\"5-балльная\",\"nmax\":6.0,\"grade\":{\"origin\":\"5\",\"five\":5.0,\"ten\":10.0,\"hundred\":100.0},\"grade_system_id\":4140720,\"grade_system_type\":\"five\"}],\"comment\":\"\",\"weight\":1,\"point_date\":null,\"control_form_name\":\"Решение задач\",\"comment_exists\":false,\"created_at\":\"2025-10-16T12:25:00\",\"updated_at\":\"2025-10-16T12:25:00\",\"criteria\":null,\"date\":\"2025-10-16\",\"subject_name\":\"Информатика\",\"subject_id\":33623623,\"has_files\":false,\"is_point\":false,\"is_exam\":false,\"original_grade_system_type\":\"five\"},{\"id\":2895530475,\"value\":\"3\",\"values\":[{\"name\":\"5-балльная\",\"nmax\":6.0,\"grade\":{\"origin\":\"3\",\"five\":3.0,\"ten\":6.0,\"hundred\":60.0},\"grade_system_id\":4140720,\"grade_system_type\":\"five\"}],\"comment\":\"Анализ 4 главы \",\"weight\":1,\"point_date\":null,\"control_form_name\":\"Письменный ответ\",\"comment_exists\":true,\"created_at\":\"2025-10-17T08:47:00\",\"updated_at\":\"2025-10-17T08:47:00\",\"criteria\":null,\"date\":\"2025-10-17\",\"subject_name\":\"Литература\",\"subject_id\":33623617,\"has_files\":false,\"is_point\":false,\"is_exam\":false,\"original_grade_system_type\":\"five\"},{\"id\":2896555661,\"value\":\"5\",\"values\":[{\"name\":\"5-балльная\",\"nmax\":6.0,\"grade\":{\"origin\":\"5\",\"five\":5.0,\"ten\":10.0,\"hundred\":100.0},\"grade_system_id\":4140720,\"grade_system_type\":\"five\"}],\"comment\":\"\",\"weight\":1,\"point_date\":null,\"control_form_name\":\"Учебное задание\",\"comment_exists\":false,\"created_at\":\"2025-10-17T12:22:00\",\"updated_at\":\"2025-10-17T12:22:00\",\"criteria\":null,\"date\":\"2025-10-17\",\"subject_name\":\"Геометрия\",\"subject_id\":33623650,\"has_files\":false,\"is_point\":false,\"is_exam\":false,\"original_grade_system_type\":\"five\"}]}";
        //string json = @"[ { ""subject_name"" : ""Алгебра и начала математического анализа"", ""subject_id"" : 33623649, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2858017437, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""05.09.2025"", ""is_point"" : false, ""control_form_id"" : 29409571, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Комбинированная работа"" }, { ""id"" : 2856772106, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""22.09.2025"", ""is_point"" : false, ""control_form_id"" : 14095812, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Опрос"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00"" } ], ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00""}, { ""subject_name"" : ""Биология"", ""subject_id"" : 33623636, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2878111802, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""01.10.2025"", ""is_point"" : false, ""control_form_id"" : 4627202, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Цифровое домашнее задание"" }, { ""id"" : 2893121827, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""15.10.2025"", ""is_point"" : false, ""control_form_id"" : 4627202, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Цифровое домашнее задание"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00"" } ], ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00""}, { ""subject_name"" : ""Вероятность и статистика"", ""subject_id"" : 33623651, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2851952285, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""12.09.2025"", ""is_point"" : false, ""control_form_id"" : 29409648, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Решение задач"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00"" } ], ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00""}, { ""subject_name"" : ""География"", ""subject_id"" : 33623620, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2840330918, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""16.09.2025"", ""is_point"" : false, ""control_form_id"" : 14094801, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Устный ответ"" }, { ""id"" : 2851862992, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""23.09.2025"", ""is_point"" : false, ""control_form_id"" : 14094822, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Работа с картой"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00"" } ], ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00""}, { ""subject_name"" : ""Геометрия"", ""subject_id"" : 33623650, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2891365378, ""comment"" : ""1 верное из 5"", ""values"" : [ { ""five"" : 2.0, ""hundred"" : 30.0, ""ten"" : 4.0, ""original"" : ""2"", ""nmax"" : 2.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""15.10.2025"", ""is_point"" : false, ""control_form_id"" : 14095864, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Тест"" }, { ""id"" : 2896555661, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""17.10.2025"", ""is_point"" : false, ""control_form_id"" : 14095878, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Учебное задание"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""3.50"", ""avg_hundred"" : ""65.00"", ""avg_ten"" : ""6.50"" } ], ""avg_five"" : ""3.50"", ""avg_hundred"" : ""65.00"", ""avg_ten"" : ""6.50""}, { ""subject_name"" : ""Индивидуальный проект"", ""subject_id"" : 40050071, ""periods"" : [ ], ""avg_five"" : ""0.00"", ""avg_hundred"" : ""0.00"", ""avg_ten"" : ""0.00""}, { ""subject_name"" : ""Инженерный практикум"", ""subject_id"" : 33623609, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2849910350, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""22.09.2025"", ""is_point"" : false, ""control_form_id"" : 27037564, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Практическая работа"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00"" } ], ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00""}, { ""subject_name"" : ""Иностранный (английский) язык"", ""subject_id"" : 37175860, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2841951315, ""comment"" : """", ""values"" : [ { ""five"" : 3.0, ""hundred"" : 60.0, ""ten"" : 6.0, ""original"" : ""3"", ""nmax"" : 3.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""10.09.2025"", ""is_point"" : false, ""control_form_id"" : 180058844, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Тест"" }, { ""id"" : 2853528356, ""comment"" : """", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""22.09.2025"", ""is_point"" : false, ""control_form_id"" : 180058737, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Учебное задание"" }, { ""id"" : 2891210365, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""13.10.2025"", ""is_point"" : false, ""control_form_id"" : 180058780, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Словарный ассоциативный ряд"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""4.00"", ""avg_hundred"" : ""80.00"", ""avg_ten"" : ""8.00"" } ], ""avg_five"" : ""4.00"", ""avg_hundred"" : ""80.00"", ""avg_ten"" : ""8.00""}, { ""subject_name"" : ""Иностранный язык"", ""subject_id"" : 33623621, ""periods"" : [ ], ""avg_five"" : ""0.00"", ""avg_hundred"" : ""0.00"", ""avg_ten"" : ""0.00""}, { ""subject_name"" : ""Информатика"", ""subject_id"" : 33623623, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2827776581, ""comment"" : """", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""04.09.2025"", ""is_point"" : false, ""control_form_id"" : 14095608, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Виртуальный практикум"" }, { ""id"" : 2834828904, ""comment"" : ""4 из 8"", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""11.09.2025"", ""is_point"" : false, ""control_form_id"" : 29409265, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Комбинированная работа"" }, { ""id"" : 2851009492, ""comment"" : """", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""19.09.2025"", ""is_point"" : false, ""control_form_id"" : 29409265, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Комбинированная работа"" }, { ""id"" : 2859747654, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""26.09.2025"", ""is_point"" : false, ""control_form_id"" : 29409265, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Комбинированная работа"" }, { ""id"" : 2890218832, ""comment"" : """", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 2, ""is_exam"" : true, ""date"" : ""15.10.2025"", ""is_point"" : false, ""control_form_id"" : 29409267, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Контрольная работа"" }, { ""id"" : 2893945042, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""16.10.2025"", ""is_point"" : false, ""control_form_id"" : 14095612, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Решение задач"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""4.29"", ""avg_hundred"" : ""85.71"", ""avg_ten"" : ""8.57"" } ], ""avg_five"" : ""4.29"", ""avg_hundred"" : ""85.71"", ""avg_ten"" : ""8.57""}, { ""subject_name"" : ""История"", ""subject_id"" : 33623645, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2871140754, ""comment"" : """", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""23.09.2025"", ""is_point"" : false, ""control_form_id"" : 14095632, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Тест"" }, { ""id"" : 2871162009, ""comment"" : """", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""30.09.2025"", ""is_point"" : false, ""control_form_id"" : 14095633, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Опрос"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""4.00"", ""avg_hundred"" : ""80.00"", ""avg_ten"" : ""8.00"" } ], ""avg_five"" : ""4.00"", ""avg_hundred"" : ""80.00"", ""avg_ten"" : ""8.00""}, { ""subject_name"" : ""Литература"", ""subject_id"" : 33623617, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2892749742, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""02.10.2025"", ""is_point"" : false, ""control_form_id"" : 4627211, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Цифровое домашнее задание"" }, { ""id"" : 2895530475, ""comment"" : ""Анализ 4 главы "", ""values"" : [ { ""five"" : 3.0, ""hundred"" : 60.0, ""ten"" : 6.0, ""original"" : ""3"", ""nmax"" : 3.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""17.10.2025"", ""is_point"" : false, ""control_form_id"" : 14095721, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Письменный ответ"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""4.00"", ""avg_hundred"" : ""80.00"", ""avg_ten"" : ""8.00"" } ], ""avg_five"" : ""4.00"", ""avg_hundred"" : ""80.00"", ""avg_ten"" : ""8.00""}, { ""subject_name"" : ""Математика"", ""subject_id"" : 33623635, ""periods"" : [ ], ""avg_five"" : ""0.00"", ""avg_hundred"" : ""0.00"", ""avg_ten"" : ""0.00""}, { ""subject_name"" : ""Обществознание"", ""subject_id"" : 33623605, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2848107456, ""comment"" : ""тест по теме человек, мировоззрение, деятельность "", ""values"" : [ { ""five"" : 3.0, ""hundred"" : 60.0, ""ten"" : 6.0, ""original"" : ""3"", ""nmax"" : 3.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""08.09.2025"", ""is_point"" : false, ""control_form_id"" : 14096005, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Диалог/Полилог"" }, { ""id"" : 2860435340, ""comment"" : ""самостоятельная работа по теме познание, истина"", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""22.09.2025"", ""is_point"" : false, ""control_form_id"" : 14096005, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Диалог/Полилог"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""4.00"", ""avg_hundred"" : ""80.00"", ""avg_ten"" : ""8.00"" } ], ""avg_five"" : ""4.00"", ""avg_hundred"" : ""80.00"", ""avg_ten"" : ""8.00""}, { ""subject_name"" : ""Основы безопасности и защиты Родины"", ""subject_id"" : 37173310, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2844862672, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""18.09.2025"", ""is_point"" : false, ""control_form_id"" : 180059249, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Комбинированная работа"" }, { ""id"" : 2882197545, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""25.09.2025"", ""is_point"" : false, ""control_form_id"" : 180058727, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Учебное задание"" }, { ""id"" : 2882197577, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""02.10.2025"", ""is_point"" : false, ""control_form_id"" : 180058727, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Учебное задание"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00"" } ], ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00""}, { ""subject_name"" : ""Русский язык"", ""subject_id"" : 33623590, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2841273354, ""comment"" : ""Задание 4 - ударения (блок 1-3) "", ""values"" : [ { ""five"" : 3.0, ""hundred"" : 60.0, ""ten"" : 6.0, ""original"" : ""3"", ""nmax"" : 3.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""11.09.2025"", ""is_point"" : false, ""control_form_id"" : 14096196, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Словарный диктант"" }, { ""id"" : 2860321265, ""comment"" : ""Задание 5 - паронимы "", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""23.09.2025"", ""is_point"" : false, ""control_form_id"" : 14096198, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Письменный ответ"" }, { ""id"" : 2871704180, ""comment"" : ""Задание 4 (ударение, блоки 4-5) "", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""23.09.2025"", ""is_point"" : false, ""control_form_id"" : 19273036, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Грамматическое задание"" }, { ""id"" : 2857885342, ""comment"" : ""Задание 16_теория "", ""values"" : [ { ""five"" : 3.0, ""hundred"" : 60.0, ""ten"" : 6.0, ""original"" : ""3"", ""nmax"" : 3.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""25.09.2025"", ""is_point"" : false, ""control_form_id"" : 14096198, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Письменный ответ"" }, { ""id"" : 2887854256, ""comment"" : """", ""values"" : [ { ""five"" : 2.0, ""hundred"" : 30.0, ""ten"" : 4.0, ""original"" : ""2"", ""nmax"" : 2.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""09.10.2025"", ""is_point"" : true, ""point_date"" : ""15.10.2025"", ""control_form_id"" : 4627282, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Цифровое домашнее задание"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""3.20"", ""avg_hundred"" : ""62.00"", ""avg_ten"" : ""6.20"" } ], ""avg_five"" : ""3.20"", ""avg_hundred"" : ""62.00"", ""avg_ten"" : ""6.20""}, { ""subject_name"" : ""Технологии современного производства"", ""subject_id"" : 33623566, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2833486596, ""comment"" : """", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""11.09.2025"", ""is_point"" : false, ""control_form_id"" : 27037447, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Практическая работа"" }, { ""id"" : 2855689853, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""25.09.2025"", ""is_point"" : false, ""control_form_id"" : 27037445, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Устный ответ"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""4.50"", ""avg_hundred"" : ""90.00"", ""avg_ten"" : ""9.00"" } ], ""avg_five"" : ""4.50"", ""avg_hundred"" : ""90.00"", ""avg_ten"" : ""9.00""}, { ""subject_name"" : ""Физика"", ""subject_id"" : 33623584, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2841452310, ""comment"" : """", ""values"" : [ { ""five"" : 3.0, ""hundred"" : 60.0, ""ten"" : 6.0, ""original"" : ""3"", ""nmax"" : 3.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""15.09.2025"", ""is_point"" : false, ""control_form_id"" : 14096257, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Терминологический диктант"" }, { ""id"" : 2839669000, ""comment"" : """", ""values"" : [ { ""five"" : 3.0, ""hundred"" : 60.0, ""ten"" : 6.0, ""original"" : ""3"", ""nmax"" : 3.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""16.09.2025"", ""is_point"" : false, ""control_form_id"" : 14096265, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Домашнее задание"" }, { ""id"" : 2872307732, ""comment"" : """", ""values"" : [ { ""five"" : 3.0, ""hundred"" : 60.0, ""ten"" : 6.0, ""original"" : ""3"", ""nmax"" : 3.0 } ], ""weight"" : 2, ""is_exam"" : true, ""date"" : ""26.09.2025"", ""is_point"" : false, ""control_form_id"" : 29408911, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Контрольная работа"" }, { ""id"" : 2868474218, ""comment"" : """", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""29.09.2025"", ""is_point"" : false, ""control_form_id"" : 14096257, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Терминологический диктант"" }, { ""id"" : 2890915137, ""comment"" : ""3 закон Ньютона"", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""15.10.2025"", ""is_point"" : false, ""control_form_id"" : 14096257, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Терминологический диктант"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""3.33"", ""avg_hundred"" : ""66.67"", ""avg_ten"" : ""6.67"" } ], ""avg_five"" : ""3.33"", ""avg_hundred"" : ""66.67"", ""avg_ten"" : ""6.67""}, { ""subject_name"" : ""Физическая культура"", ""subject_id"" : 33623580, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2836530208, ""comment"" : """", ""values"" : [ { ""five"" : 4.0, ""hundred"" : 80.0, ""ten"" : 8.0, ""original"" : ""4"", ""nmax"" : 4.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""11.09.2025"", ""is_point"" : false, ""control_form_id"" : 14096332, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Учебное задание"" }, { ""id"" : 2844440245, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""18.09.2025"", ""is_point"" : false, ""control_form_id"" : 14096332, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Учебное задание"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""4.50"", ""avg_hundred"" : ""90.00"", ""avg_ten"" : ""9.00"" } ], ""avg_five"" : ""4.50"", ""avg_hundred"" : ""90.00"", ""avg_ten"" : ""9.00""}, { ""subject_name"" : ""Химия"", ""subject_id"" : 33623577, ""periods"" : [ { ""name"" : ""1 полугодие"", ""marks"" : [ { ""id"" : 2893109663, ""comment"" : """", ""values"" : [ { ""five"" : 5.0, ""hundred"" : 100.0, ""ten"" : 10.0, ""original"" : ""5"", ""nmax"" : 5.0 } ], ""weight"" : 1, ""is_exam"" : false, ""date"" : ""15.10.2025"", ""is_point"" : false, ""control_form_id"" : 4627200, ""grade_system_type"" : ""five"", ""control_form_name"" : ""Цифровое домашнее задание"" } ], ""start"" : ""01.09"", ""end"" : ""30.12"", ""start_iso"" : ""2025-09-01"", ""end_iso"" : ""2025-12-30"", ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00"" } ], ""avg_five"" : ""5.00"", ""avg_hundred"" : ""100.00"", ""avg_ten"" : ""10.00""} ]";;
        //PushGrades(json, false);
        //await Task.Delay(000);
        //PushGrades(json, false);
    }
    public async UniTask GetGrades(bool show = false, bool cancel = false)
    {
        if (bottonButtons.PageNow() != 2) return;
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
            currentGetLessonsTask = InternalGetGrades(show, token, currentTaskId);
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

    private async UniTask InternalGetGrades(bool load, CancellationToken token, Guid taskId)
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
                    animatorGradePanels.SetTrigger("Off");
                    animatorLoad?.SetTrigger("Load");
                }

                token.ThrowIfCancellationRequested();

                string marksJson = null;
                if (choiceGrade.Now() == 0) marksJson = await GetJsonAsync($"https://dnevnik.mos.ru/reports/api/progress/json?academic_year_id=13&student_profile_id={SecureStorage.Load("id")}", token, isMarks: true);
                if (choiceGrade.Now() == 1) marksJson = await GetJsonAsync($"https://school.mos.ru/api/family/web/v1/marks?student_id=36239136&from={GetDate(-1)}&to={GetDate(1)}", isMarks: false);

                token.ThrowIfCancellationRequested();
                //Log(lessonJson);
                if (await PushGrades(marksJson, load, token))
                {
                    token.ThrowIfCancellationRequested();

                    refreshEntry.Entry(cancel: true);
                    errorText.text = "";
                    
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
            if (load && await PushGrades(null, load, token))
                CloseLoad();
            errorText.text = "Не удалось получить данные с МЕШ";
            ShowMeshError();
        }
        catch (Exception ex)
        {
            //Log($"Ошибка в InternalGetLessons: {ex.Message}");
            if (load && await PushGrades(null, load, token))
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

    private async UniTask<string> GetJsonAsync(string url, CancellationToken token = default, bool isMarks = false)
    {
        try
        {
            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Add("User-Agent", "Dnevnik/4.37.0 (Android 13; Samsung SM-A525F) okhttp/4.9.3");
            request.Headers.Add("Accept", "application/json");
            request.Headers.Add("Auth-Token", SecureStorage.Load("AuthToken"));
            request.Headers.Add("Cookie", SecureStorage.Load("Cookies"));
            if (!isMarks)
            {
                request.Headers.Add("X-Mes-Subsystem", "familyweb");
                request.Headers.Add("profile-type", "student");
                request.Headers.Add("Authorization", $"Bearer {SecureStorage.Load("AuthToken")}");
                request.Headers.Add("Profile-Id", SecureStorage.Load("id"));
            }
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
        refreshEntry.Entry(grade: true);
    }

    async UniTask<bool> PushGrades(string marksJson, bool load, CancellationToken token = default)
    {
        if (marksJson == null) return false;

        for (int i = 0; i < (transform.Find("SwipePanel").childCount) - 3; i++)
            Destroy(transform.Find("SwipePanel").GetChild(i + 3).gameObject);
                
        await Task.Delay(35);
        token.ThrowIfCancellationRequested();

        if (choiceGrade.Now() == 0)
        {
            swipeGrade.ResetSize();
            if (marksJson == null) return false;

            JArray arr = JArray.Parse(marksJson);
            int arrCount = arr.Count;
            //Debug.Log(arrCount);
            int index = 0;
            int col = 0;

            token.ThrowIfCancellationRequested();

            for (int i = 0; i < arr.Count; i++)
            {
                token.ThrowIfCancellationRequested();

                index = i - col;
                JObject subject = (JObject)arr[i];

                // Берём первый период
                if ((subject["periods"]).ToList().Count == 0)
                {
                    col++;
                    arrCount = arrCount - 1;
                    continue;
                }
                JObject period = (JObject)subject["periods"].First();

                string subjectName = (string)subject["subject_name"];
                var marks = period["marks"].ToList();
                int marksCount = marks.Count;
                var Grades = marks
                    .Select(m => Convert.ToInt32((double)m["values"][0]["five"]))
                    .ToList();
                float avgFive = float.Parse((string)period["avg_five"], CultureInfo.InvariantCulture);
                int gradeIcon = avgFive switch
                {
                    >= 4.6f => 5,
                    >= 3.6f => 4,
                    >= 2.6f => 3,
                    _ => 2
                };
                int RD = (marksCount - 1) / 6;
                HeightUP(index + 1, RD + 1);
                token.ThrowIfCancellationRequested();

                swipeGrade.SizeSwipePanel(transform.Find($"SwipePanel/AverageGradePanel{index + 1}").GetComponent<RectTransform>().rect.height, index == arrCount - 1 ? 0 : 1);
                for (int n = 0; n < Grades.Count; n++)
                    PushGradeToPanel(index + 1, n + 1, Grades[n], RD + 1);
                token.ThrowIfCancellationRequested();

                Color AvgGradeColor = avgFive switch
                {
                    >= 4.6f => new Color(10f / 255f, 1f, 165f / 255f),
                    >= 3.6f => new Color(207f / 255f, 111f / 255f, 222f / 255f),
                    >= 2.6f => new Color(1f, 130f / 255f, 84f / 255f),
                    _ => new Color(222f / 255f, 62f / 255f, 97f / 255f)
                };
                transform.Find($"SwipePanel/AverageGradePanel{index + 1}/AverageGradeIcon").GetComponent<TMP_Text>().text = gradeIcon.ToString();
                transform.Find($"SwipePanel/AverageGradePanel{index + 1}/AverageGradeIcon").GetComponent<TMP_Text>().color = AvgGradeColor;
                transform.Find($"SwipePanel/AverageGradePanel{index + 1}/AverageGradeIcon/BG").GetComponent<Image>().sprite = sprites[6 - gradeIcon];
                transform.Find($"SwipePanel/AverageGradePanel{index + 1}/AverageGradeIcon/AvgText").GetComponent<TMP_Text>().text = (string)period["avg_five"];
                transform.Find($"SwipePanel/AverageGradePanel{index + 1}/AverageGradeIcon/AvgText").GetComponent<TMP_Text>().color = AvgGradeColor;
                transform.Find($"SwipePanel/AverageGradePanel{index + 1}/LessonName").GetComponent<TMP_Text>().text = subjectName;
            }
            swipeGrade.Pos(arrCount);

            token.ThrowIfCancellationRequested();
        }
        else
        {
            swipeGrade.ResetSize();
            JObject obj = JObject.Parse(marksJson);

            List<string> dates = new List<string>() { };
            token.ThrowIfCancellationRequested();

            for (int i = 0; i < (obj["payload"].ToList()).Count; i++)
            {
                token.ThrowIfCancellationRequested();
                JObject first = (JObject)obj["payload"][i];

                string value = first["value"]?.ToString();
                string controlForm = first["control_form_name"]?.ToString();
                string date = first["date"]?.ToString();
                string subject = first["subject_name"]?.ToString();

                dates.Add(date);

                RectTransform PanelRect;
                RectTransform PanelTextRect;
                TMP_Text PanelTextText;

                if (transform.Find($"SwipePanel/Panel {date}") == null)
                {
                    GameObject Panel = new GameObject($"Panel {date}", typeof(RectTransform));
                    Panel.layer = LayerMask.NameToLayer("UI");
                    Panel.transform.SetParent(transform.Find("SwipePanel").GetComponent<RectTransform>(), true);
                    PanelRect = Panel.GetComponent<RectTransform>();

                    PanelRect.anchoredPosition = new Vector2(0, 0);
                    PanelRect.sizeDelta = new Vector2(GradesPanelList[0], HeightGradeText);
                    PanelRect.localScale = new Vector3(1, 1, 1);



                    GameObject PanelText = new GameObject($"Text", typeof(RectTransform));
                    PanelText.layer = LayerMask.NameToLayer("UI");
                    PanelText.transform.SetParent(PanelRect, true);
                    PanelTextRect = PanelText.GetComponent<RectTransform>();

                    PanelTextRect.sizeDelta = new Vector2(0, HeightGradeText);
                    PanelTextRect.anchorMin = new Vector2(0f, 0.5f);
                    PanelTextRect.anchorMax = new Vector2(1f, 0.5f);
                    PanelTextRect.anchoredPosition = new Vector2(0, 0);
                    PanelTextRect.localScale = new Vector3(1, 1, 1);

                    PanelTextText = PanelText.AddComponent<TextMeshProUGUI>();
                    PanelTextText.alignment = TextAlignmentOptions.MidlineLeft;
                    PanelTextText.enableAutoSizing = true;
                    PanelTextText.fontSizeMin = 15;
                    PanelTextText.fontSizeMax = 50;
                }
                token.ThrowIfCancellationRequested();

                PanelTextRect = transform.Find($"SwipePanel/Panel {date}/Text").GetComponent<RectTransform>();
                PanelTextText = transform.Find($"SwipePanel/Panel {date}/Text").GetComponent<TextMeshProUGUI>();
                PanelRect = transform.Find($"SwipePanel/Panel {date}").GetComponent<RectTransform>();
                Transform PanelTR = transform.Find($"SwipePanel/Panel {date}");
                
                DateTime dateText = DateTime.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                string formatted = dateText.ToString("d MMMM yyyy dddd", new CultureInfo("ru-RU"));
                formatted = char.ToUpper(formatted[0]) + formatted.Substring(1);
                PanelTextText.text = formatted;

                CreateGrade(dates.Count(x => x == date), PanelTR, subject, value, controlForm);
                token.ThrowIfCancellationRequested();

                PanelRect.sizeDelta = new Vector2(PanelRect.rect.width, HeightGradeText + (PanelTR.childCount - 1) * (GradePanelList[1] + GradePanelList[1] / 4f) - GradePanelList[1] / 4f);
                PanelTextRect.anchoredPosition = new Vector2(0, PanelRect.rect.height / 2f - HeightGradeText / 2f);
                float allHeightGrade = PanelRect.rect.height / 2f - HeightGradeText + PanelTR.GetChild(1).GetComponent<RectTransform>().rect.height / 2f;

                for (int n = 0; n < PanelTR.childCount - 1; n++)
                {
                    token.ThrowIfCancellationRequested();
                    RectTransform childRect = PanelTR.GetChild(n + 1).GetComponent<RectTransform>();

                    allHeightGrade = allHeightGrade - GradePanelList[1];
                    childRect.anchoredPosition = new Vector2(0, allHeightGrade);
                    allHeightGrade = allHeightGrade - GradePanelList[1] / 4f;
                }
            }
            swipeGrade.SizeSwipePanelGrade(dates.Distinct().ToList());
        }
        return true;
    }

    void CreateAveGrade(int value)
    {
        GameObject AverageGradePanel = new GameObject($"AverageGradePanel{value}", typeof(RectTransform));
        AverageGradePanel.layer = LayerMask.NameToLayer("UI");
        AverageGradePanel.transform.SetParent(transform.Find("SwipePanel").GetComponent<RectTransform>(), true);
        RectTransform AverageGradePanelRect = AverageGradePanel.GetComponent<RectTransform>();

        AverageGradePanelRect.anchoredPosition = new Vector2(0, 0);
        AverageGradePanelRect.sizeDelta = new Vector2(objList[0], objList[1]);
        AverageGradePanelRect.localScale = new Vector3(1, 1, 1);

        Image AverageGradePanelImg = AverageGradePanel.AddComponent<Image>();
        AverageGradePanelImg.sprite = sprites[0];
        AverageGradePanelImg.type = Image.Type.Sliced;




        GameObject AverageGradeIcon = new GameObject("AverageGradeIcon", typeof(RectTransform));
        AverageGradeIcon.layer = LayerMask.NameToLayer("UI");
        AverageGradeIcon.transform.SetParent(AverageGradePanelRect, true);
        RectTransform AverageGradeIconRect = AverageGradeIcon.GetComponent<RectTransform>();

        TMP_Text AverageGradeIconText = AverageGradeIcon.AddComponent<TextMeshProUGUI>();
        AverageGradeIconText.fontSize = 100;
        AverageGradeIconText.alignment = TextAlignmentOptions.Center;

        AverageGradeIconRect.anchorMin = new Vector2(0f, 0f);
        AverageGradeIconRect.anchorMax = new Vector2(1f, 1f);
        AverageGradeIconRect.pivot = new Vector2(0.5f, 0.5f);
        AverageGradeIconRect.offsetMin = new Vector2(AveradeGradeIconList[0], AveradeGradeIconList[1]);  // Left / Bottom
        AverageGradeIconRect.offsetMax = new Vector2(AveradeGradeIconList[2], AveradeGradeIconList[3]); // Right / Top
        AverageGradeIconRect.localScale = new Vector3(1, 1, 1);



        GameObject AverageGradeIconBG = new GameObject("BG", typeof(RectTransform));
        AverageGradeIconBG.layer = LayerMask.NameToLayer("UI");
        AverageGradeIconBG.transform.SetParent(AverageGradeIconRect, true);
        RectTransform AverageGradeIconBGRect = AverageGradeIconBG.GetComponent<RectTransform>();

        Image AverageGradeIconBGImg = AverageGradeIconBG.AddComponent<Image>();

        AverageGradeIconBGRect.anchorMin = new Vector2(0f, 0f);
        AverageGradeIconBGRect.anchorMax = new Vector2(1f, 1f);
        AverageGradeIconBGRect.pivot = new Vector2(0.5f, 0.5f);
        AverageGradeIconBGRect.anchoredPosition = new Vector2(0, 0);
        AverageGradeIconBGRect.sizeDelta = new Vector2(0, 0);
        AverageGradeIconBGRect.localScale = new Vector3(1, 1, 1);

        AverageGradeIconBGImg.color = new Color(1, 1, 1, 90 / 255f);
        AverageGradeIconBGImg.type = Image.Type.Sliced;

        GameObject AverageGradeIconAvgText = new GameObject("AvgText", typeof(RectTransform));
        AverageGradeIconAvgText.layer = LayerMask.NameToLayer("UI");
        AverageGradeIconAvgText.transform.SetParent(AverageGradeIconRect, true);
        RectTransform AverageGradeIconAvgTextRect = AverageGradeIconAvgText.GetComponent<RectTransform>();

        TMP_Text AverageGradeIconAvgTextText = AverageGradeIconAvgText.AddComponent<TextMeshProUGUI>();
        AverageGradeIconAvgTextText.fontSize = 35;
        AverageGradeIconAvgTextText.alignment = TextAlignmentOptions.Bottom;

        AverageGradeIconAvgTextRect.anchorMin = new Vector2(0f, 0f);
        AverageGradeIconAvgTextRect.anchorMax = new Vector2(1f, 1f);
        AverageGradeIconAvgTextRect.pivot = new Vector2(0.5f, 0.5f);
        AverageGradeIconAvgTextRect.offsetMin = new Vector2(0, AveradeGradeIconAvgTextList[0]);  // Left / Bottom
        AverageGradeIconAvgTextRect.offsetMax = new Vector2(0, 0); // Right / Top
        AverageGradeIconAvgTextRect.localScale = new Vector3(1, 1, 1);



        GameObject LessonName = new GameObject("LessonName", typeof(RectTransform));
        LessonName.layer = LayerMask.NameToLayer("UI");
        LessonName.transform.SetParent(AverageGradePanelRect, true);
        RectTransform LessonNameRect = LessonName.GetComponent<RectTransform>();

        TMP_Text LessonNameText = LessonName.AddComponent<TextMeshProUGUI>();
        LessonNameText.fontSize = 65;
        LessonNameText.alignment = TextAlignmentOptions.MidlineLeft;
        LessonNameText.enableAutoSizing = true;
        LessonNameText.fontSizeMin = 15;
        LessonNameText.fontSizeMax = 65;

        LessonNameRect.anchorMin = new Vector2(0f, 0f);
        LessonNameRect.anchorMax = new Vector2(1f, 1f);
        LessonNameRect.pivot = new Vector2(0.5f, 0.5f);
        LessonNameRect.offsetMin = new Vector2(LessonNameList[0], LessonNameList[1]);  // Left / Bottom
        LessonNameRect.offsetMax = new Vector2(LessonNameList[2], LessonNameList[3]); // Right / Top
        LessonNameRect.localScale = new Vector3(1, 1, 1);



        GameObject AverageGrades = new GameObject("AverageGrades", typeof(RectTransform));
        AverageGrades.layer = LayerMask.NameToLayer("UI");
        AverageGrades.transform.SetParent(AverageGradePanelRect, true);
        RectTransform AverageGradesRect = AverageGrades.GetComponent<RectTransform>();

        AverageGradesRect.anchorMin = new Vector2(0f, 0f);
        AverageGradesRect.anchorMax = new Vector2(1f, 1f);
        AverageGradesRect.pivot = new Vector2(0.5f, 0.5f);
        AverageGradesRect.offsetMin = new Vector2(AveradeGradesList[0], AveradeGradesList[1]);  // Left / Bottom
        AverageGradesRect.offsetMax = new Vector2(AveradeGradesList[2], AveradeGradesList[3]); // Right / Top
        AverageGradesRect.localScale = new Vector3(1, 1, 1);
    }
    void CreateGrade(int value, Transform TR, string name, string grade, string control_form_name)
    {
        GameObject GradePanel = new GameObject($"GradePanel{value}", typeof(RectTransform));
        GradePanel.layer = LayerMask.NameToLayer("UI");
        GradePanel.transform.SetParent(TR, true);
        RectTransform GradePanellRect = GradePanel.GetComponent<RectTransform>();

        GradePanellRect.anchoredPosition = new Vector2(0, 0);
        GradePanellRect.sizeDelta = new Vector2(GradesPanelList[0], GradesPanelList[1]);
        GradePanellRect.localScale = new Vector3(1, 1, 1);

        Image GradePanelImg = GradePanel.AddComponent<Image>();
        GradePanelImg.sprite = sprites[0];
        GradePanelImg.type = Image.Type.Sliced;
        


        GameObject GradeIcon = new GameObject("GradeIcon", typeof(RectTransform));
        GradeIcon.layer = LayerMask.NameToLayer("UI");
        GradeIcon.transform.SetParent(GradePanellRect, true);
        RectTransform GradeIconRect = GradeIcon.GetComponent<RectTransform>();

        TMP_Text GradeIconText = GradeIcon.AddComponent<TextMeshProUGUI>();
        GradeIconText.fontSize = 100;
        GradeIconText.alignment = TextAlignmentOptions.Center;
        GradeIconText.text = grade;
        GradeIconText.color = grade switch
            {
                "5" => new Color(10f / 255f, 1f, 165f / 255f),
                "4" => new Color(207f / 255f, 111f / 255f, 222f / 255f),
                "3" => new Color(1f, 130f / 255f, 84f / 255f),
                _ => new Color(222f / 255f, 62f / 255f, 97f / 255f)
            };

        GradeIconRect.anchorMin = new Vector2(0f, 0f);
        GradeIconRect.anchorMax = new Vector2(1f, 1f);
        GradeIconRect.pivot = new Vector2(0.5f, 0.5f);
        GradeIconRect.offsetMin = new Vector2(GradeIconList[0], GradeIconList[1]);  // Left / Bottom
        GradeIconRect.offsetMax = new Vector2(GradeIconList[2], GradeIconList[3]); // Right / Top
        GradeIconRect.localScale = new Vector3(1, 1, 1);



        GameObject GradeIconBG = new GameObject("BG", typeof(RectTransform));
        GradeIconBG.layer = LayerMask.NameToLayer("UI");
        GradeIconBG.transform.SetParent(GradeIconRect, true);
        RectTransform GradeIconBGRect = GradeIconBG.GetComponent<RectTransform>();

        Image GradeIconBGImage = GradeIconBG.AddComponent<Image>();

        GradeIconBGRect.anchorMin = new Vector2(0f, 0f);
        GradeIconBGRect.anchorMax = new Vector2(1f, 1f);
        GradeIconBGRect.pivot = new Vector2(0.5f, 0.5f);
        GradeIconBGRect.anchoredPosition = new Vector2(0, 0);
        GradeIconBGRect.sizeDelta = new Vector2(0, 0);
        GradeIconBGRect.localScale = new Vector3(1, 1, 1);

        GradeIconBGImage.color = new Color(1, 1, 1, 90 / 255f);
        GradeIconBGImage.type = Image.Type.Sliced;
        GradeIconBGImage.sprite = sprites[6 - int.Parse(grade)];



        GameObject LessonName = new GameObject("LessonName", typeof(RectTransform));
        LessonName.layer = LayerMask.NameToLayer("UI");
        LessonName.transform.SetParent(GradePanellRect, true);
        RectTransform LessonNameRect = LessonName.GetComponent<RectTransform>();

        TMP_Text LessonNameText = LessonName.AddComponent<TextMeshProUGUI>();
        LessonNameText.alignment = TextAlignmentOptions.MidlineLeft;
        LessonNameText.enableAutoSizing = true;
        LessonNameText.fontSizeMin = 15;
        LessonNameText.fontSizeMax = 65;
        LessonNameText.text = name;

        LessonNameRect.anchorMin = new Vector2(0f, 0f);
        LessonNameRect.anchorMax = new Vector2(1f, 1f);
        LessonNameRect.pivot = new Vector2(0.5f, 0.5f);
        LessonNameRect.offsetMin = new Vector2(GradeLessonNameList[0], GradeLessonNameList[1]);  // Left / Bottom
        LessonNameRect.offsetMax = new Vector2(GradeLessonNameList[2], GradeLessonNameList[3]); // Right / Top
        LessonNameRect.localScale = new Vector3(1, 1, 1);



        GameObject GradeText = new GameObject("GradeText", typeof(RectTransform));
        GradeText.layer = LayerMask.NameToLayer("UI");
        GradeText.transform.SetParent(GradePanellRect, true);
        RectTransform GradeTextRect = GradeText.GetComponent<RectTransform>();

        GradeTextRect.anchorMin = new Vector2(0f, 0f);
        GradeTextRect.anchorMax = new Vector2(1f, 1f);
        GradeTextRect.pivot = new Vector2(0.5f, 0.5f);
        GradeTextRect.offsetMin = new Vector2(GradeTextList[0], GradeTextList[1]);  // Left / Bottom
        GradeTextRect.offsetMax = new Vector2(GradeTextList[2], GradeTextList[3]); // Right / Top
        GradeTextRect.localScale = new Vector3(1, 1, 1);

        TMP_Text GradeTextText = GradeText.AddComponent<TextMeshProUGUI>();
        GradeTextText.enableAutoSizing = true;
        GradeTextText.fontSizeMin = 15;
        GradeTextText.fontSizeMax = 40;
        GradeTextText.alignment = TextAlignmentOptions.MidlineLeft;
        GradeTextText.text = control_form_name;
    }

    void HeightUP(int number, int valueR)
    {
        Transform PanelTransform = transform.Find($"SwipePanel/AverageGradePanel{number}");
        if (PanelTransform == null) CreateAveGrade(number);

        RectTransform Panel = transform.Find($"SwipePanel/AverageGradePanel{number}").GetComponent<RectTransform>();
        Panel.sizeDelta = new Vector2(GradePanelList[0], GradePanelList[1] + GradesInfoList[0] * (valueR - 1));

        RectTransform LessonNameRect = transform.Find($"SwipePanel/AverageGradePanel{number}/LessonName").GetComponent<RectTransform>();
        LessonNameRect.offsetMin = new Vector2(LessonNameList[0], LessonNameList[1] + GradesInfoList[0] * (valueR - 1));  // Left / Bottom
    }

    void PushGradeToPanel(int number, int pos, int grade, int allRD)
    {
        float x = 0;
        float y = 0;
        float posfloat = allRD;
        int RD = (pos - 1) / 6 + 1;

        y = (allRD - 1) * 0.5f * GradesInfoList[0] - (RD - 1) * GradesInfoList[0];
        x = GradesInfoList[3] + GradesInfoList[1] * Mathf.Abs((RD - 1) * 6 - pos + 1);



        Transform AverageGrades = transform.Find($"SwipePanel/AverageGradePanel{number}/AverageGrades");

        TMP_Text GradeText;
        RectTransform GradeRect;

        Image GradeBGImg;
        RectTransform GradeBGRect;

        if (AverageGrades.Find($"Grade{pos}") == null)
        {
            RectTransform AverageGradesRect = AverageGrades.GetComponent<RectTransform>();

            GameObject Grade = new GameObject($"Grade{pos}", typeof(RectTransform));
            Grade.layer = LayerMask.NameToLayer("UI");
            Grade.transform.SetParent(AverageGradesRect, true);
            GradeRect = Grade.GetComponent<RectTransform>();
            GradeRect.localScale = new Vector3(1, 1, 1);

            GradeText = Grade.AddComponent<TextMeshProUGUI>();
            GradeText.fontSize = 50;
            GradeText.alignment = TextAlignmentOptions.Center;

            GameObject GradeBG = new GameObject("BG", typeof(RectTransform));
            GradeBG.layer = LayerMask.NameToLayer("UI");
            GradeBG.transform.SetParent(GradeRect, true);
            GradeBGRect = GradeBG.GetComponent<RectTransform>();
            GradeBGRect.localScale = new Vector3(1, 1, 1);

            GradeBGImg = GradeBGRect.AddComponent<Image>();
        }
        else
        {
            GradeText = AverageGrades.Find($"Grade{pos}").GetComponent<TMP_Text>();
            GradeRect = AverageGrades.Find($"Grade{pos}").GetComponent<RectTransform>();

            GradeBGRect = AverageGrades.Find($"Grade{pos}/BG").GetComponent<RectTransform>();
            GradeBGImg = AverageGrades.Find($"Grade{pos}/BG").GetComponent<Image>();
        }   

        GradeText.text = grade.ToString();
        switch (grade)
        {
            case 5:
                GradeText.color = new Color(10f / 255f, 1f, 165f / 255f);
                break;
            case 4:
                GradeText.color = new Color(207f / 255f, 111f / 255f, 222f / 255f);
                break;
            case 3:
                GradeText.color = new Color(1f, 127f / 255f, 80f / 255f);
                break;
            case 2:
                GradeText.color = new Color(222f / 255f, 62f / 255f, 97f / 255f);
                break;
        }

        GradeRect.anchorMin = new Vector2(0.5f, 0.5f);
        GradeRect.anchorMax = new Vector2(0.5f, 0.5f);
        GradeRect.pivot = new Vector2(0.5f, 0.5f);
        //GradeRect.anchoredPosition = new Vector2(GradesInfoList[3] + GradesInfoList[1] * pos, GradesInfoList[0]);
        GradeRect.anchoredPosition = new Vector2(x, y);
        GradeRect.sizeDelta = new Vector2(GradesInfoList[2], GradesInfoList[2]);



        GradeBGRect.anchorMin = new Vector2(0f, 0f);
        GradeBGRect.anchorMax = new Vector2(1f, 1f);
        GradeBGRect.pivot = new Vector2(0.5f, 0.5f);
        GradeBGRect.anchoredPosition = new Vector2(0, 0);
        GradeBGRect.sizeDelta = new Vector2(0, 0);

        GradeBGImg.color = new Color(1, 1, 1, 60 / 255f);
        GradeBGImg.sprite = sprites[6 - grade];

    }

    List<JToken> JsonToList(string json, string find)
    {
        return ((JArray)(JToken.Parse(json))[find]).ToObject<List<JToken>>();
    }

    string GetDate(int day)
    {
        List<List<string>> masterList = new List<List<string>>();

        masterList.Add(weekManager.GetWeeks(1));
        masterList.Add(weekManager.GetWeeks(2));

        if (day == 1) return masterList[1][dayManager.DayNow() - 1];
        return masterList[0][0];
    }

    private void CloseLoad()
    {
        animatorLoad.SetTrigger("CloseLoad");
        animatorLoad.SetBool("ShowGrades", true);
    }
    public void Off()
    {
        animatorGradePanels.SetTrigger("Off");
    }

    void Log(string s)
    {
        Debug.Log(s);
        if (Logtext != null) Logtext.text += s + " ";
    }
}
