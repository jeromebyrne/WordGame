using UnityEngine;
using TMPro;

public class ExceptionDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text exceptionText;

    private void Awake()
    {
        // Subscribe to the unhandled exception event
        Application.logMessageReceived += HandleLog;
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to prevent memory leaks
        Application.logMessageReceived -= HandleLog;
    }

    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        // Only display exceptions
        if (type == LogType.Exception)
        {
            DisplayException(logString, stackTrace);
        }
    }

    private void DisplayException(string logString, string stackTrace)
    {
        // Display the exception message and stack trace
        exceptionText.text = $"Exception: {logString}\n{stackTrace}";
    }
}