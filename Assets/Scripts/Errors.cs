using TMPro;
using UnityEngine;

public class ErrorLogger : MonoBehaviour
{
    public TMP_Text errorText; // Assign this in the Inspector to your TMP_Text component
    void Awake()
    {
        //OnEnable();
    }
    private void OnEnable()
    {
        Application.logMessageReceived += HandleLog;
    }

    private void OnDisable()
    {
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (type == LogType.Error || type == LogType.Exception)
        {
            if (errorText != null)
            {
                errorText.text += $"{logString}\n{stackTrace}\n\n"; // Append error message and stack trace
            }
        }
    }
}