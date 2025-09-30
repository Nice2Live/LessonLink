using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ProfilePanelToggle : MonoBehaviour
{
    [Header("Animators")]
    public Animator panelAnimator; // Animator для главной панели
    public Animator bgAnimator;    // Animator для фона
    public Animator SupportAnimator;

    [Header("Objects to toggle")]
    public GameObject MainProfilePanel; // объекты, которые будут включаться/выключаться
    private bool isOpen = false;   // текущее состояние

    // метод можно привязать к ЛЮБОЙ кнопке
    public void Toggle()
    {
        if (SecureStorage.Load("Entry") == "true")
        {
            isOpen = !isOpen;

            if (isOpen)
            {
                MainProfilePanel.SetActive(true);
                panelAnimator.SetTrigger("ProfileMainPanelOnTrigger");
                SupportAnimator.SetTrigger("SupportOn");
            }
            else
            {
                panelAnimator.SetTrigger("ProfileMainPanelOffTrigger");
                bgAnimator.SetTrigger("ProfileBGOff");
                SupportAnimator.SetTrigger("SupportOff");
            }
        }
    }
}