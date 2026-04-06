using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 윈도우 위치 이동·조회 전용 유틸 — MovementController와 DragController가 공유한다.
/// </summary>
public static class WindowPositioner
{
    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOSIZE     = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;

    [DllImport("user32.dll")] private static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] private static extern bool   SetWindowPos(
        IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);
    [DllImport("user32.dll")] private static extern bool   GetWindowRect(
        IntPtr hWnd, ref WinRect lpRect);

    private static IntPtr _hwnd = IntPtr.Zero;

    // DragController에서도 접근하므로 internal로 노출
    internal static IntPtr Hwnd
    {
        get
        {
            if (_hwnd == IntPtr.Zero)
                _hwnd = GetActiveWindow();
            return _hwnd;
        }
    }

    /// <summary>윈도우를 (x, y) 픽셀 위치로 이동한다. (Windows 화면 좌표계 기준)</summary>
    public static void SetPosition(int x, int y)
    {
#if !UNITY_EDITOR
        SetWindowPos(Hwnd, HWND_TOPMOST, x, y, 0, 0, SWP_NOSIZE | SWP_NOACTIVATE);
#endif
    }

    /// <summary>현재 윈도우의 Windows 화면 좌표계 기준 좌상단 위치를 반환한다.</summary>
    public static Vector2 GetPosition()
    {
#if UNITY_EDITOR
        return Vector2.zero;
#else
        WinRect rect = default;
        GetWindowRect(Hwnd, ref rect);
        return new Vector2(rect.left, rect.top);
#endif
    }

    private struct WinRect
    {
        public int left, top, right, bottom;
    }
}
