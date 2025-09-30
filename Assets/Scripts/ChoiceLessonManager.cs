using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System.Threading.Tasks;

public class ChoiceLessonManager : MonoBehaviour
{
    [Header("Верхние Image кнопок")]
    public List<Image> buttonImages;

    [Header("Панели внутри кнопок")]
    public List<Image> panelImages;
    public List<Button> buttons;
    public List<TMP_Text> textColor;

    public VerticalDragPanel swipePanel;
    [SerializeField] private float animationDuration = 0.1f;

    private int currentIndex = -1;
    private Coroutine currentCoroutine = null;
    private int lessonsCount = 8;
    public TMP_Text Logtext;

    void Awake()
    {
        for (int i = 0; i < buttons.Count; i++)
        {
            int index = i + 1;
            buttons[i].onClick.AddListener(() => SwitchButtonSwipe(index));
        }

        // Выставляем дефолтные альфы
        ForceFinalState();
    }

    public int LessonsCount() { return lessonsCount; }

    public void Changebutton(int NumLes)
    {
        for (int i = 0; i <= NumLes - 1; i++)
        {
            textColor[i].color = new Color(1f, 1f, 1f);
            buttons[i].enabled = true;
        }

        lessonsCount = NumLes;
        NumLes = 7 - NumLes;

        for (int i = 0; i <= NumLes; i++)
        {
            textColor[i].color = new Color(0.3891f, 0.4237f, 0.4705f);
            buttons[buttons.Count - 1 - i].enabled = false;
        }
    }
    public async Task SwitchButtonSwipe(int index)
    {
        swipePanel.Pos(index);
        SwitchButton(index);
    }
    public async Task SwitchButton(int index)
    {
        index -= 1;
        if (index < 0 || index >= buttonImages.Count) return;

        // Если нажата та же кнопка → ничего не делаем
        if (index == currentIndex) return;

        // Если есть активная анимация — прерываем её и выставляем корректные альфы
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
            ForceFinalState();
            // Обновляем Canvas мгновенно, чтобы Unity применил альфы прямо сейчас
            Canvas.ForceUpdateCanvases();
        }
        // Запускаем новую анимацию
        currentCoroutine = StartCoroutine(AnimateSwitch(index));
    }

    private IEnumerator AnimateSwitch(int newIndex)
    {
        int oldIndex = currentIndex;
        currentIndex = newIndex;

        // Считываем стартовые значения
        float oldBtnStartAlpha = oldIndex >= 0 ? buttonImages[oldIndex].color.a : 1f;
        float oldPanelStartAlpha = oldIndex >= 0 ? panelImages[oldIndex].color.a : 0f;

        float newBtnStartAlpha = buttonImages[newIndex].color.a;
        float newPanelStartAlpha = panelImages[newIndex].color.a;

        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / animationDuration);

            // Закрываем старую кнопку
            if (oldIndex >= 0)
            {
                SetAlpha(buttonImages[oldIndex], Mathf.Lerp(oldBtnStartAlpha, 1f, t));
                SetAlpha(panelImages[oldIndex], Mathf.Lerp(oldPanelStartAlpha, 0f, t));
            }

            // Открываем новую кнопку
            SetAlpha(buttonImages[newIndex], Mathf.Lerp(newBtnStartAlpha, 0f, t));
            SetAlpha(panelImages[newIndex], Mathf.Lerp(newPanelStartAlpha, 1f, t));

            yield return null;
        }

        // В конце точно выставляем финальные значения
        if (oldIndex >= 0)
        {
            SetAlpha(buttonImages[oldIndex], 1f);
            SetAlpha(panelImages[oldIndex], 0f);
        }
        SetAlpha(buttonImages[newIndex], 0f);
        SetAlpha(panelImages[newIndex], 1f);

        // Принудительно обновляем Canvas — убираем задержки и артефакты
        Canvas.ForceUpdateCanvases();

        currentCoroutine = null;
    }

    private void ForceFinalState()
    {
        for (int i = 0; i < buttonImages.Count; i++)
        {
            if (i == currentIndex)
            {
                SetAlpha(buttonImages[i], 0f);
                SetAlpha(panelImages[i], 1f);
            }
            else
            {
                SetAlpha(buttonImages[i], 1f);
                SetAlpha(panelImages[i], 0f);
            }
        }
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }
    public int GetCurrentIndex()
    {
        return currentIndex;
    }
    public void ResetCurrentIndex()
    {
        currentIndex = -1;
    }
    void Log(string s)
    {
        Debug.Log(s);
        if (Logtext != null) Logtext.text += s + "\n";
    }
}
