#if UNITY_EDITOR_WIN
using UnityEditor;
using UnityEngine;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

[InitializeOnLoad]
public class BringUnityToFrontAfterCompile
{
    static BringUnityToFrontAfterCompile()
    {
        EditorApplication.delayCall += BringUnityToFront;
    }

    [DllImport("user32.dll")]
    private static extern bool SetForegroundWindow(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    [DllImport("user32.dll")]
    private static extern IntPtr GetForegroundWindow();

    [DllImport("user32.dll")]
    private static extern uint GetWindowThreadProcessId(IntPtr hWnd, IntPtr ProcessId);

    [DllImport("kernel32.dll")]
    private static extern uint GetCurrentThreadId();

    [DllImport("user32.dll")]
    private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

    const int SW_RESTORE = 9;

    static void BringUnityToFront()
    {
        Process currentProcess = Process.GetCurrentProcess();
        IntPtr hWnd = currentProcess.MainWindowHandle;

        if (hWnd == IntPtr.Zero)
        {
            UnityEngine.Debug.LogWarning("Failed to get Unity window handle.");
            return;
        }

        IntPtr foregroundWindow = GetForegroundWindow();
        uint foregroundThreadId = GetWindowThreadProcessId(foregroundWindow, IntPtr.Zero);
        uint currentThreadId = GetCurrentThreadId();

        // Attach input threads so we can forcefully bring the window to front
        if (AttachThreadInput(currentThreadId, foregroundThreadId, true))
        {
            ShowWindow(hWnd, SW_RESTORE);
            SetForegroundWindow(hWnd);
            AttachThreadInput(currentThreadId, foregroundThreadId, false); // Detach afterwards
        }
        else
        {
            UnityEngine.Debug.LogWarning("Failed to attach thread input.");
        }
    }
}
#endif
