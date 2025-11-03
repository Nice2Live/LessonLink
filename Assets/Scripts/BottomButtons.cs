using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System.Threading.Tasks;
using TMPro;

public class BottonButtons : MonoBehaviour
{
    public DayManager dayManager;
    public RefreshEntry refreshEntry;
    public Lessons lessons;
    public TMP_Text currentStateText;
    public Grades grades;
    public Animator animatorLoad;
    public ChoiceGrade choiceGrade;
    public List<RectTransform> rectPanels;
    public List<Sprite> diaryIcons;
    public List<Sprite> gradeIcons;
    public List<Sprite> profileIcons;
    public List<Image> bbuttonsImage;
    public List<Button> bbuttonsButton;
    public List<TMP_Text> bbuttonsText;

    private int now = -1;
    private bool resetTime = true;
    void Awake()
    {
        currentStateText.text = "";
        Toggle(1);
    }
    async Task Start()
    {
        await Task.Delay(250);
        for (int i = 0; i < bbuttonsButton.Count; i++)
        {
            int index = i;
            bbuttonsButton[i].onClick.AddListener(() => Toggle(index + 1));
        }
        //grades.GetGrades(true);
    }
    public async Task Toggle(int value)
    {
        if (SecureStorage.Load("Entry") != "true")
        {
            currentStateText.text = "";

            rectPanels[0].anchoredPosition = new Vector2(10000, 0);
            rectPanels[1].anchoredPosition = new Vector2(10000, 0);
            rectPanels[2].anchoredPosition = new Vector2(10000, 0);

            bbuttonsImage[0].sprite = diaryIcons[0];
            bbuttonsImage[1].sprite = gradeIcons[0];
            bbuttonsImage[2].sprite = profileIcons[0];

            bbuttonsText[0].color = (new Color(70 / 255f, 81 / 255f, 96 / 255f));
            bbuttonsText[1].color = (new Color(70 / 255f, 81 / 255f, 96 / 255f));
            bbuttonsText[2].color = (new Color(70 / 255f, 81 / 255f, 96 / 255f));

            dayManager.PushUpdateTime(false);
            refreshEntry.Entry(cancel: true);
            lessons.GetLessons(cancel: true);

            return;
        }
        
        if (!resetTime || now == value) return;
        resetTime = false;

        now = value;

        bbuttonsImage[0].sprite = diaryIcons[Diary(value)];
        bbuttonsImage[1].sprite = gradeIcons[Grade(value)];
        bbuttonsImage[2].sprite = profileIcons[Profile(value)];

        bbuttonsText[0].color = (value == 1 ? new Color(20 / 255f, 184 / 255f, 166 / 255f) : new Color(70 / 255f, 81 / 255f, 96 / 255f));
        bbuttonsText[1].color = (value == 2 ? new Color(20 / 255f, 184 / 255f, 166 / 255f) : new Color(70 / 255f, 81 / 255f, 96 / 255f));
        bbuttonsText[2].color = (value == 3 ? new Color(20 / 255f, 184 / 255f, 166 / 255f) : new Color(70 / 255f, 81 / 255f, 96 / 255f));
        
        await Task.Delay(75);
        resetTime = true;
    }
    private int Diary(int value)
    {
        if (value != 1)
        {
            rectPanels[0].anchoredPosition = new Vector2(10000, 0);
            return 0; 
        }
        currentStateText.text = "Дневник";
        rectPanels[0].anchoredPosition = new Vector2(0, 0);
        grades.GetGrades(cancel: true);
        dayManager.PushUpdateTime(true);
        lessons.GetLessons(true);
        return 1;
    }
    private int Grade(int value)
    {
        if (value != 2)
        {
            rectPanels[1].anchoredPosition = new Vector2(10000, 0);
            return 0;
        }
        currentStateText.text = "Оценки";
        //grades.Off();
        rectPanels[1].anchoredPosition = new Vector2(0, 0);
        dayManager.PushUpdateTime(false);
        refreshEntry.Entry(cancel: true);
        lessons.GetLessons(cancel: true);
        choiceGrade.Toggle(0);
        return 1;
    }
    private int Profile(int value)
    {
        if (value != 3)
        {
            rectPanels[2].anchoredPosition = new Vector2(10000, 0);
            return 0;
        } 

        AnimatorStateInfo stateInfo = animatorLoad.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName("Load") || stateInfo.IsName("CloseLoad"))
            animatorLoad.SetTrigger("Off");

        currentStateText.text = "Профиль";
        rectPanels[2].anchoredPosition = new Vector2(0, 0);
        dayManager.PushUpdateTime(false);
        grades.GetGrades(cancel: true);
        refreshEntry.Entry(cancel: true);
        lessons.GetLessons(cancel: true);
        return 1;
    }
    public int PageNow()
    {
        return now;
    }
}