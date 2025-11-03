using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ChoiceGrade : MonoBehaviour
{
    public Button aveGradeButton;
    public Button lastGradeButton;
    public Grades grades;
    public BottonButtons bottonButtons;
    private int now = -1;
    private bool resetToggle = true;
    public List<Sprite> sprites;
    public List<Image> images;
    void Awake()
    {
        aveGradeButton.onClick.AddListener(() => Toggle(0));
        lastGradeButton.onClick.AddListener(() => Toggle(1));
    }
    public async Task Toggle(int value)
    {
        if (!resetToggle && now == value && bottonButtons.PageNow() != 2 ) return;

        resetToggle = false;

        now = value;

        grades.GetGrades(true);

        images[0].sprite = sprites[AveGrade(value)];
        images[1].sprite = sprites[LastGrade(value)];

        await Task.Delay(75);

        resetToggle = true;
    }


    int AveGrade(int value)
    {
        return value == 0 ? 1 : 0;
    }
    int LastGrade(int value)
    {
        return value == 1 ? 1 : 0;
    }
    public int Now()
    {
        return now;
    }
}
