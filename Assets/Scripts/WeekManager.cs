using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Threading.Tasks;

public class WeekManager : MonoBehaviour
{
    public Animator animator;
    public Lessons lessons;
    public ChoiceLessonManager choiceLessonManager;
    public Button leftweek;
    public Button rightweek;
    public DayManager dayManager;
    private List<string> weekNow = new List<string>();
    private List<string> weekAfter = new List<string>();
    private List<string> weekPast = new List<string>();

    private string now = "2";

    private DateTime referenceDate;

    // таблица переходов
    private Dictionary<string, (string trigger, string next)> transitions = new Dictionary<string, (string, string)>
    {
        { "22", ("2to3", "3") },
        { "31", ("3to2", "2") },
        { "21", ("2to1", "1") },
        { "12", ("1to2", "2") },
    };

    // Получаем список дат недели, начиная с понедельника, в формате yyyy-MM-dd
    private List<string> GetWeekDates(DateTime baseDate)
    {
        int currentDay = (int)baseDate.DayOfWeek;
        if (currentDay == 0) currentDay = 7; // воскресенье

        DateTime monday = baseDate.AddDays(-(currentDay - 1));

        List<string> weekDates = new List<string>();
        for (int i = 0; i < 7; i++)
        {
            weekDates.Add(monday.AddDays(i).ToString("yyyy-MM-dd"));
        }

        return weekDates;
    }

    public List<string> GetCurrentWeek() => GetWeekDates(referenceDate);
    public List<string> GetNextWeek() => GetWeekDates(referenceDate.AddDays(7));
    public List<string> GetPreviousWeek() => GetWeekDates(referenceDate.AddDays(-7));

    public List<string> GoToNextWeek()
    {
        referenceDate = referenceDate.AddDays(7);
        return GetCurrentWeek();
    }

    public List<string> GoToPreviousWeek()
    {
        referenceDate = referenceDate.AddDays(-7);
        return GetCurrentWeek();
    }

    // Пример использования
    public List<string> GetWeeks(int current)
    {
        List<string> weekNow = new List<string>();
        List<string> weekAfter = new List<string>();
        List<string> weekPast = new List<string>();

        referenceDate = DateTime.Today;

        weekNow = GetCurrentWeek();
        if (current == 2) return weekNow;

        weekAfter = GetNextWeek();
        if (current == 3) return weekAfter;

        weekPast = GetPreviousWeek();
        if (current == 1) return weekPast;

        return null;
    }

    void Awake()
    {
        referenceDate = DateTime.Today;
        leftweek.onClick.AddListener(() => Toggle("1"));
        rightweek.onClick.AddListener(() => Toggle("2"));
    }

    public async Task Toggle(string value)
    {
        string key = now + value;

        if (transitions.TryGetValue(key, out var transition))
        {
            animator.SetTrigger(transition.trigger);

            now = transition.next;

            if (now == "1") dayManager.Toggle(5);
            if (now == "2") dayManager.Toggle(dayManager.DayNowWeek());
            if (now == "3") dayManager.Toggle(1);

            lessons.GetLessons(true);
        }
    }
    public int WeekNow()
    {
        return int.Parse(now);
    }
}