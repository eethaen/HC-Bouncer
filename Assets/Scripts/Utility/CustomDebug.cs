using System.Diagnostics;
using Debug = UnityEngine.Debug;

public static class CustomDebug
{
    [Conditional("DEBUG")]
    public static void Log(object message)
    {
        Debug.Log(message);
    }

    [Conditional("DEBUG")]
    public static void LogError(string message)
    {
        Debug.LogError(message);
    }

    [Conditional("DEBUG")]
    public static void LogWarning(string message)
    {
        Debug.LogWarning(message);
    }

    [Conditional("DEBUG")]
    public static void Assert(bool condition)
    {
        Debug.Assert(condition);
    }
}
