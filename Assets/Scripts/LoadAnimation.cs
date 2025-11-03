using UnityEngine;
using System.Threading.Tasks;

public class LoadAnimation : MonoBehaviour
{
    public Animator ChoiceLesson;
    public Animator lessonsAnim;
    public Animator animatorLoad;
    public DayManager dayManager;
    public Animator animatorGradePanels;
    public BottonButtons bottonButtons;
    public Lessons lessons;
    public Grades grades;
    private bool start = false;
    async Task Start()
    {
        await Task.Delay(5000);
        start = true;
    }
    public void On()
    {
        if (animatorLoad.GetBool("Show"))
        {
            animatorLoad.SetBool("Show", false);
            ChoiceLesson.SetTrigger("8");
            lessonsAnim.SetTrigger("On");
        }
        if (animatorLoad.GetBool("ShowGrades"))
        {
            animatorLoad.SetBool("ShowGrades", false);
            animatorGradePanels.SetTrigger("On");
        }
    }
    public void True()
    {
        dayManager.PushUpdateTime(true);
    }
    public void False()
    {
        dayManager.PushUpdateTime(false);
    }
    public void Reload()
    {
        return;
        if (!start) return;

        AnimatorStateInfo stateInfo = animatorLoad.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("Off"))
        {
            int now = bottonButtons.PageNow();
            if (now == 1) lessons.GetLessons(false);
            if (now == 2) grades.GetGrades(false);
        }
    }
}
