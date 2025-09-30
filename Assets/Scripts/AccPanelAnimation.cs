using UnityEngine;

public class AccPanelAnimation : MonoBehaviour
{
    public GameObject profilePanelBG;
    public GameObject profileMainPanel;
    public Animator bgAnimator;

    public void Create()
    {
        profilePanelBG.SetActive(true);
        bgAnimator.SetTrigger("ProfileBGOn");
    }
    public void Destroy()
    {
        profilePanelBG.SetActive(false);
        profileMainPanel.SetActive(false);
    }
}
