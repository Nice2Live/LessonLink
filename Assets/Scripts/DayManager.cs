using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;
using System.Threading.Tasks;

public class DayManager : MonoBehaviour
{
    public Animator dayanimator;
    public WeekManager Weekmanager;
    public Lessons lessons;
    public ChoiceLessonManager choiceLessonManager;
    public List<Button> dayButtons;
    public TMP_Text Logtext;
    public LoadAnimation loadAnimation;

    private int now = -1;
    private List<int> OTtimes = new List<int>() { 000, 915, 1015, 1120, 1220, 1320, 1425, 1525 };
    private List<int> DOtimes = new List<int>() { 915, 1015, 1120, 1220, 1320, 1425, 1525, 1800 };
    private List<string> weekdays = new List<string>() { "Monday", "Tuesday", "Wednesday", "Thursday", "Friday", "Saturday", "Sunday" };
    private int togglelday = -1;
    private string  toggleresetlday;
    private int togglelessons = -1;
    private int inttoggle;
    private List<string> weekNow;
    private bool start = true;
    private bool updateTime = true;
    private bool resetTime = true;


    void Awake()
    {
        for (int i = 0; i < dayButtons.Count; i++)
        {
            int index = i + 1;
            dayButtons[i].onClick.AddListener(() => Toggle(index));
        }
        weekNow = Weekmanager.GetWeeks(2);
        UpdateTime();
    }

    public async Task Toggle(int value)
    {
        if (!resetTime || now == value) return;
        resetTime = false;

        if (now == -1) dayanimator.SetTrigger($"3to{value}");
        else dayanimator.SetTrigger($"{now}to{value}");
        now = value;

        lessons.GetLessons(true);
        await Task.Delay(75);
        resetTime = true;
    }
    void Update()
    {
        if (updateTime) UpdateTime();
    }
    public async Task PushList(List<int> OT, List<int> DO)
    {
        OTtimes = new List<int>(OT);
        DOtimes = new List<int>(DO);
        await Reset();
        UpdateTime();
    }
    public int DayNow()
    {
        return now;
    }
    void Log(string s)
    {
        Debug.Log(s);
        if (Logtext != null) Logtext.text += s + "\n";
    }
    public async Task Reset()
    {
        DateTime datenow = DateTime.Now;
        DayOfWeek dayOfWeek = datenow.DayOfWeek;
        string day = dayOfWeek.ToString();
        if (weekdays.IndexOf(day) + 1 == now && (day == "Friday" || day == "Saturday" || day == "Sunday") && start) { togglelday = -1; start = false; }
        else start = false;
        //choiceLessonManager.ResetCurrentIndex();
        await choiceLessonManager.SwitchButtonSwipe(1);
        togglelessons = -1;
    }
    public void UpdateTime()
    {
        string today = DateTime.Now.ToString("yyyy-MM-dd");
        if (!weekNow.Contains(today))
        {
            togglelday = -1;
            weekNow = Weekmanager.GetWeeks(2);

            if (Weekmanager.WeekNow() == 3) Weekmanager.Toggle("1");
        }
        else
        {
            DateTime datenow = DateTime.Now;
            DayOfWeek dayOfWeek = datenow.DayOfWeek;

            int hours = datenow.Hour;
            int minutes = datenow.Minute;

            string day = dayOfWeek.ToString();

            int time = int.Parse(hours.ToString() + (minutes.ToString().Length == 1 ? "0" + minutes.ToString() : minutes.ToString()));

            for (int i = 0; i <= weekdays.Count - 1; i++)
            {
                if (togglelday != i && weekdays[i] == day)
                {
                    togglelday = i;

                    if (Weekmanager.WeekNow() == 2)
                    {
                        if (time >= DOtimes[DOtimes.Count - 1])
                        {
                            loadAnimation.Reload();
                            if (i <= 3) inttoggle = i + 2;
                            else { inttoggle = 1; Weekmanager.Toggle("2"); }
                        }
                        else if (i <= 4) inttoggle = i + 1;
                        else { inttoggle = 1; Weekmanager.Toggle("2"); }

                        Toggle(inttoggle);
                        break;
                    }
                }
            }

            if (weekdays.IndexOf(day) + 1 == now && Weekmanager.WeekNow() == 2)
            {
                for (int i = 0; i <= DOtimes.Count - 1; i++)
                {
                    if (togglelessons != i && OTtimes[i] <= time && DOtimes[i] > time)
                    {
                        loadAnimation.Reload();
                        togglelessons = i;
                        choiceLessonManager.SwitchButtonSwipe(i + 1);
                        break;
                    }
                }    
            }

            if (toggleresetlday != day && time >= DOtimes[DOtimes.Count - 1])
            {
                togglelday = -1;
                toggleresetlday = day;
            }
        }
    }
    public void PushUpdateTime(bool time)
    {
        updateTime = time;
    }
    public int DayNowWeek()
    {
        DateTime datenow = DateTime.Now;
        DayOfWeek dayOfWeek = datenow.DayOfWeek;
        string day = dayOfWeek.ToString();
        //Debug.Log(weekdays.IndexOf(day) + 1);
        return weekdays.IndexOf(day) + 1 > 5 ? 5 : weekdays.IndexOf(day) + 1;
    }
}
