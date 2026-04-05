using System;
using System.Runtime.InteropServices;
using UnityEngine;

/// <summary>
/// 다중 모니터 환경에서 펫이 올바른 경계를 계산하도록 지원한다.
/// 가상 데스크탑 전체 영역(모든 모니터 합산)을 반환한다.
/// </summary>
public static class MultiMonitorSupport
{
    [DllImport("user32.dll")] private static extern int GetSystemMetrics(int nIndex);

    private const int SM_XVIRTUALSCREEN  = 76;  // 가상 데스크탑 좌상단 X
    private const int SM_YVIRTUALSCREEN  = 77;  // 가상 데스크탑 좌상단 Y
    private const int SM_CXVIRTUALSCREEN = 78;  // 가상 데스크탑 전체 폭
    private const int SM_CYVIRTUALSCREEN = 79;  // 가상 데스크탑 전체 높이

    public struct VirtualDesktop
    {
        public int x, y, width, height;
    }

    /// <summary>
    /// 모든 모니터를 포괄하는 가상 데스크탑 영역을 반환한다.
    /// </summary>
    public static VirtualDesktop GetBounds()
    {
#if UNITY_EDITOR
        return new VirtualDesktop
        {
            x = 0, y = 0,
            width  = Screen.currentResolution.width,
            height = Screen.currentResolution.height
        };
#else
        return new VirtualDesktop
        {
            x      = GetSystemMetrics(SM_XVIRTUALSCREEN),
            y      = GetSystemMetrics(SM_YVIRTUALSCREEN),
            width  = GetSystemMetrics(SM_CXVIRTUALSCREEN),
            height = GetSystemMetrics(SM_CYVIRTUALSCREEN)
        };
#endif
    }
}
