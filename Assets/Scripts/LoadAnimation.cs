using UnityEngine;

public class LoadAnimation : MonoBehaviour
{
    public Animator ChoiceLesson;
    public Animator lessons;
    public Animator animatorLoad;
    public DayManager dayManager;
    public void On()
    {
        if (animatorLoad.GetBool("Show"))
        {
            animatorLoad.SetBool("Show", false);
            ChoiceLesson.SetTrigger("8");
            lessons.SetTrigger("On");
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
}
