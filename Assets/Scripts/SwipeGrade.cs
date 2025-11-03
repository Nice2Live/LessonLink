using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using System;
using System.Collections.Generic;

public class SwipeGrade : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rt;
    public RectTransform parent;
    private Vector2 lastPointerPos;
    private Vector2 startPointerPos;
    private float startTime;

    private Vector2 velocity;
    private bool isDragging;

    private float FullSize;
    private float pos;
    private float minY;
    private float maxY;
    private int numLes = 8;
    public TMP_Text Logtext;
    private int SwitchButton = -1;
    private int lastCallId = 0;

    [SerializeField] private float decelerationRate = 5f;
    private float maxSpeed;
    private float heightAveGradePanel;
    private float heightSwipeGrade;

    void Awake()
    {
        heightSwipeGrade = transform.GetComponent<RectTransform>().rect.width;
        heightAveGradePanel = transform.Find($"AverageGradePanel").GetComponent<RectTransform>().rect.height;
        rt = GetComponent<RectTransform>();
        maxSpeed = rt.rect.height;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (parent.rect.height > rt.rect.height) return;
        isDragging = true;
        velocity = Vector2.zero;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, eventData.position, eventData.pressEventCamera, out lastPointerPos);

        startPointerPos = lastPointerPos;
        startTime = Time.unscaledTime;
    }

    public void SizeSwipePanel(float value, int last)
    {
        rt.sizeDelta = new Vector2(rt.rect.width, rt.rect.height + value + heightAveGradePanel / 4f * last);
        rt = transform.GetComponent<RectTransform>();
        maxSpeed = rt.rect.height;
        float parentHeight = parent.rect.height;
        float panelHeight = rt.rect.height;

        float halfPanel = panelHeight * 0.5f;
        float halfParent = parentHeight * 0.5f;

        maxY = halfPanel - halfParent;
        minY = -(panelHeight - halfPanel - halfParent);
    }
    public void Pos(int count)
    {
        try
        {
            float firstHeight = transform.Find("AverageGradePanel1").GetComponent<RectTransform>().rect.height;
            float allHeight = rt.rect.height / 2f + firstHeight / 2f;
            allHeight = allHeight - firstHeight;
            transform.Find("AverageGradePanel1").GetComponent<RectTransform>().anchoredPosition = new Vector2(0, allHeight);

            for (int i = 0; i < count; i++)
            {
                Transform AveGradePanel = transform.Find($"AverageGradePanel{i + 2}");
                if (AveGradePanel == null) break;
                float height = transform.Find($"AverageGradePanel{i + 1}").GetComponent<RectTransform>().rect.height;
                float cof = height - AveGradePanel.GetComponent<RectTransform>().rect.height;
                allHeight = allHeight - height + cof / 2f - heightAveGradePanel / 4f;
                AveGradePanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, allHeight);
            }
            rt.anchoredPosition = new Vector2(0, minY);
        }
        catch (Exception ex)
        {
            //Debug.LogError($"{ex.Message}");
        }
    }
    public void SizeSwipePanelGrade(List<string> ListPanels)
    {
        rt = transform.GetComponent<RectTransform>();

        float allHeightGrade = 0f;
        float GradePanelText = transform.Find("GradePanelText").GetComponent<RectTransform>().rect.height;
        for (int i = 0; i < ListPanels.Count; i++)
            allHeightGrade = allHeightGrade + transform.Find($"Panel {ListPanels[i]}").GetComponent<RectTransform>().rect.height + GradePanelText / 2f;
        allHeightGrade = allHeightGrade - GradePanelText / 2f;
        //Debug.Log(allHeightGrade);
        rt.sizeDelta = new Vector2(rt.rect.width, allHeightGrade);
        rt = transform.GetComponent<RectTransform>();
        maxSpeed = rt.rect.height;

        float lastX = 0f;
        float lastHeight = 0f;
        for (int i = 0; i < ListPanels.Count; i++)
        {
            RectTransform Panel = transform.Find($"Panel {ListPanels[i]}").GetComponent<RectTransform>();
            if (i == 0)
            {
                lastX = -(rt.rect.height / 2f) + Panel.rect.height / 2f;
                lastHeight = Panel.rect.height;
                Panel.anchoredPosition = new Vector2(0, lastX);
            }
            else
            {
                //Debug.Log($"{lastX} + {Panel.rect.height} + {GradePanelText / 2f} - {(-(lastHeight - Panel.rect.height) / 2f)}");
                lastX = lastX + Panel.rect.height + GradePanelText / 2f - (-(lastHeight - Panel.rect.height) / 2f);
                lastHeight = Panel.rect.height;
                Panel.anchoredPosition = new Vector2(0, lastX);
            }
        }
        rt = transform.GetComponent<RectTransform>();
        float parentHeight = parent.rect.height;
        float panelHeight = rt.rect.height;

        float halfPanel = panelHeight * 0.5f;
        float halfParent = parentHeight * 0.5f;

        maxY = halfPanel - halfParent;
        minY = -(panelHeight - halfPanel - halfParent);
        rt.anchoredPosition = new Vector2(0, minY);
        rt = transform.GetComponent<RectTransform>();

    }
    public void ResetSize()
    {
        rt.sizeDelta = new Vector2(rt.rect.width, 0);
        rt = transform.GetComponent<RectTransform>();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parent.rect.height > rt.rect.height) return;
        Vector2 pointerPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, eventData.position, eventData.pressEventCamera, out pointerPos);

        Vector2 delta = pointerPos - lastPointerPos;
        lastPointerPos = pointerPos;

        MovePanel(delta.y);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (parent.rect.height > rt.rect.height) return;
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
        if (!isDragging && Mathf.Abs(velocity.y) > 0.01f)
        {
            MovePanel(velocity.y * Time.deltaTime);
            velocity = Vector2.Lerp(velocity, Vector2.zero, decelerationRate * Time.deltaTime);
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
}
