using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// WinAPI P/Invoke를 통해 Unity 윈도우를 투명·클릭통과·항상최상위로 설정한다.
/// Magenta(#FF00FF) colorkey를 완전히 제거하여 투명 배경을 구현한다.
/// </summary>
public class WindowManager : MonoBehaviour
{
    // ── WinAPI 상수 ──────────────────────────────────────────────────────────
    private const int GWL_STYLE      = -16;
    private const int GWL_EXSTYLE    = -20;
    private const long WS_POPUP      = 0x80000000L;
    private const long WS_VISIBLE    = 0x10000000L;
    private const long WS_EX_LAYERED    = 0x00080000L;
    private const long WS_EX_TRANSPARENT = 0x00000020L;
    private const long WS_EX_TOPMOST    = 0x00000008L;
    private const long WS_EX_TOOLWINDOW = 0x00000080L;
    private const int  LWA_COLORKEY  = 0x1;

    private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
    private const uint SWP_NOMOVE    = 0x0002;
    private const uint SWP_NOSIZE    = 0x0001;
    private const uint SWP_NOACTIVATE = 0x0010;

    // ── WinAPI P/Invoke ───────────────────────────────────────────────────────
    [DllImport("user32.dll")] private static extern IntPtr GetActiveWindow();
    [DllImport("user32.dll")] private static extern long   GetWindowLong(IntPtr hWnd, int nIndex);
    [DllImport("user32.dll")] private static extern long   SetWindowLong(IntPtr hWnd, int nIndex, long dwNewLong);
    [DllImport("user32.dll")] private static extern bool   SetLayeredWindowAttributes(IntPtr hWnd, uint crKey, byte bAlpha, uint dwFlags);
    [DllImport("user32.dll")] private static extern bool   SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    private IntPtr _hwnd;

    private void Awake()
    {
#if !UNITY_EDITOR
        _hwnd = GetActiveWindow();
        ApplyTransparentWindow();
        SetTopmost();
#endif
    }

    /// <summary>
    /// 투명 레이어드 윈도우 적용 + Magenta colorkey 제거
    /// </summary>
    private void ApplyTransparentWindow()
    {
        // 스타일: 팝업(타이틀바 없음)
        SetWindowLong(_hwnd, GWL_STYLE, WS_POPUP | WS_VISIBLE);

        // 확장 스타일: Layered + Transparent(클릭통과) + ToolWindow(작업표시줄 숨김)
        SetWindowLong(_hwnd, GWL_EXSTYLE,
            WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOPMOST | WS_EX_TOOLWINDOW);

        // Magenta(0x00FF00FF) = colorkey로 완전 투명 처리
        uint magenta = 0x00FF00FF;
        SetLayeredWindowAttributes(_hwnd, magenta, 0, LWA_COLORKEY);
    }

    /// <summary>
    /// 항상 최상위(HWND_TOPMOST) 고정
    /// </summary>
    private void SetTopmost()
    {
        SetWindowPos(_hwnd, HWND_TOPMOST, 0, 0, 0, 0,
            SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE);
    }

    /// <summary>
    /// 런타임 중 Topmost 재적용 (포커스 빼앗긴 후 복구용)
    /// </summary>
    public void ReapplyTopmost()
    {
#if !UNITY_EDITOR
        SetTopmost();
#endif
    }
}
