using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using TMPro;
using System.Threading;

public class VerticalDragPanel : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rt;
    public RectTransform parent;
    private Vector2 lastPointerPos;
    private Vector2 startPointerPos;
    private float startTime;

    private Vector2 velocity;
    private bool isDragging;
    private bool isAnimating;

    public RectTransform ChoiseLessonPanel;
    public ChoiceLessonManager choiceLessonManager;
    public DayManager dayManager;
    public List<RectTransform> LessonsPanel;
    private List<float> OTHeights;
    private List<float> DOHeights;

    private float lessonHeight;
    private float choiceLessonHeight;

    private float FullSize;
    private float pos;
    private float minY;
    private float maxY;
    private int numLes = 8;
    public TMP_Text Logtext;
    private int SwitchButton = -1;
    private int lastCallId = 0;

    private CancellationTokenSource getLessonsCTS;

    [SerializeField] private float decelerationRate = 5f;
    private float maxSpeed;

    void Awake()
    {
        OTHeights = new List<float>();
        DOHeights = new List<float>();

        lessonHeight = LessonsPanel[0].rect.height;
        choiceLessonHeight = ChoiseLessonPanel.rect.height / 3;
        rt = GetComponent<RectTransform>();
        maxSpeed = rt.rect.height;
    }
    public async UniTask NumLes(int NumLes, CancellationToken token = default)
    {
        numLes = NumLes;
        
        SetPanelSize();
        token.ThrowIfCancellationRequested();

        SetLessonsPanelPos();
        token.ThrowIfCancellationRequested();

        choiceLessonManager.Changebutton(NumLes);
        token.ThrowIfCancellationRequested();
    }

    public async Task Pos(int lesson)
    {
        int callId = ++lastCallId;
        // Запрещаем свайп и обнуляем скорость
        isDragging = false;
        velocity = Vector2.zero;

        isAnimating = true;

        // Вычисляем целевую позицию
        float lessonPos = minY + lessonHeight * (lesson - 1) + choiceLessonHeight * (lesson - 1);
        if (lesson == numLes - 1) lessonPos -= choiceLessonHeight * 4;
        if (lessonPos > maxY) lessonPos -= lessonHeight + choiceLessonHeight;

        lessonPos = Mathf.Clamp(lessonPos, minY, maxY);

        Vector2 startPos = rt.anchoredPosition;
        Vector2 targetPos = new Vector2(0, lessonPos);

        float duration = 0.35f; // длительность анимации
        float elapsed = 0f;

        isAnimating = true;

        while (elapsed < duration)
        {
            isAnimating = true;

            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // плавная ease-in-out анимация
            t = t * t * (3f - 2f * t);

            rt.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
            await Task.Delay(10);
        }

        if (callId == lastCallId)
        {
            rt.anchoredPosition = targetPos;
            velocity = Vector2.zero;
            isAnimating = false;
        }
    }

    void StartPos()
    {
        float parentHeight = parent.rect.height;
        float panelHeight = rt.rect.height;

        float halfPanel = panelHeight * 0.5f;
        float halfParent = parentHeight * 0.5f;

        maxY = halfPanel - halfParent;
        minY = -(panelHeight - halfPanel - halfParent);

        rt.anchoredPosition = new Vector2(0, minY);
    }

    public void SetPanelSize()
    {
        FullSize = lessonHeight * numLes + choiceLessonHeight * (numLes - 1);
        rt.sizeDelta = new Vector2(rt.rect.width, FullSize);
        StartPos();
    }

    public void SetLessonsPanelPos()
    {
        for (int i = 0; i <= 7; i++)
        {
            LessonsPanel[i].anchoredPosition = new Vector2(1000, 0);
        }
        if (OTHeights != null)
        {
            OTHeights.Clear();
            DOHeights.Clear();
        }
        float lessonPos1 = minY;
        float lessonPos2;
        lessonPos1 = lessonPos1;
        OTHeights.Add(lessonPos1);
        lessonPos2 = lessonPos1 + lessonHeight / 3 * 2;
        DOHeights.Add(lessonPos2);

        for (int i = 0; i < numLes - 3; i++)
        {
            lessonPos1 = lessonPos2;
            OTHeights.Add(lessonPos1);
            lessonPos2 = lessonPos1 + lessonHeight + choiceLessonHeight;
            DOHeights.Add(lessonPos2);
        }

        lessonPos1 = lessonPos2;
        OTHeights.Add(lessonPos1);
        lessonPos2 = lessonPos1 + choiceLessonHeight * 5;
        DOHeights.Add(lessonPos2);
        lessonPos1 = maxY - choiceLessonHeight;
        OTHeights.Add(lessonPos1);
        lessonPos2 = maxY + choiceLessonHeight * 3;
        DOHeights.Add(lessonPos2);

        pos = rt.rect.height / 2 - lessonHeight / 2;
        LessonsPanel[0].anchoredPosition = new Vector2(0, pos);
        for (int i = 0; i <= numLes - 2; i++)
        {
            int index = i + 1;
            pos = pos - lessonHeight - choiceLessonHeight;
            LessonsPanel[index].anchoredPosition = new Vector2(0, pos);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isAnimating) return; // <--- блокируем свайп во время анимации

        isDragging = true;
        velocity = Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, eventData.position, eventData.pressEventCamera, out lastPointerPos);

        startPointerPos = lastPointerPos;
        startTime = Time.unscaledTime;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isAnimating) return; // <--- тоже блокируем

        Vector2 pointerPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, eventData.position, eventData.pressEventCamera, out pointerPos);

        Vector2 delta = pointerPos - lastPointerPos;
        lastPointerPos = pointerPos;

        MovePanel(delta.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isAnimating) return; // <--- и здесь

        isDragging = false;

        Vector2 endPointerPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, eventData.position, eventData.pressEventCamera, out endPointerPos);

        float totalDistance = endPointerPos.y - startPointerPos.y;
        float totalTime = Mathf.Max(Time.unscaledTime - startTime, 0.01f);

        velocity = new Vector2(0, totalDistance / totalTime);
        velocity.y = Mathf.Clamp(velocity.y, -maxSpeed, maxSpeed);
    }

    void Update()
    {
        // если идёт анимация, отключаем инерцию
        if (isAnimating) return;

        if (!isDragging && Mathf.Abs(velocity.y) > 0.01f)
        {
            MovePanel(velocity.y * Time.deltaTime);
            velocity = Vector2.Lerp(velocity, Vector2.zero, decelerationRate * Time.deltaTime);
        }

        float y = rt.anchoredPosition.y;
        for (int i = 0; i < OTHeights.Count; i++)
        {
            if (SwitchButton != i && choiceLessonManager.GetCurrentIndex() != i && y >= OTHeights[i] && y < DOHeights[i])
            {
                if (isAnimating) break;
                SwitchButton = i;
                choiceLessonManager.SwitchButton(i + 1);
                break;
            }
        }
    }

    private void MovePanel(float deltaY)
    {
        Vector2 newPos = rt.anchoredPosition + new Vector2(0, deltaY);

        float parentHeight = parent.rect.height;
        float panelHeight = rt.rect.height;

        float halfPanel = panelHeight * 0.5f;
        float halfParent = parentHeight * 0.5f;

        maxY = halfPanel - halfParent;
        minY = -(panelHeight - halfPanel - halfParent);

        newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

        rt.anchoredPosition = newPos;

        if (newPos.y == minY || newPos.y == maxY)
            velocity = Vector2.zero;
    }
    void Log(string s)
    {
        Debug.Log(s);
        if (Logtext != null) Logtext.text += s + "\n";
    }
}
