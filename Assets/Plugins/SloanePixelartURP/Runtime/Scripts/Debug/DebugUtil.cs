using System.Runtime.CompilerServices;
using UnityEngine;

public static class PixelartDebug
{
    public static bool EnableDebug = true;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Log(string message)
    {
        if (!EnableDebug) return;
        PrefixMessage(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogWarning(string message)
    {
        if (!EnableDebug) return;
        PrefixMessage(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void LogError(string message)
    {
        if (!EnableDebug) return;
        PrefixMessage(message);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PrefixMessage(string message)
    {
        Debug.Log($"[SloanePixelartURP] {message}");
    }
}
