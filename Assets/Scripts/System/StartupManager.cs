using Microsoft.Win32;
using UnityEngine;

/// <summary>
/// Windows 레지스트리를 통해 시작프로그램 등록/해제를 처리한다.
/// HKCU\Software\Microsoft\Windows\CurrentVersion\Run
/// </summary>
public class StartupManager : MonoBehaviour
{
    private const string RegistryKey  = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";
    private const string AppName      = "DesktopPet";

    private void Start()
    {
        // 첫 실행 시 자동 등록 여부를 사용자에게 묻지 않음.
        // TrayIconManager의 메뉴에서 사용자가 직접 제어한다.
    }

    /// <summary>현재 시작프로그램 등록 여부 확인</summary>
    public static bool IsRegistered()
    {
#if !UNITY_EDITOR
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, false);
        return key?.GetValue(AppName) != null;
#else
        return false;
#endif
    }

    /// <summary>시작프로그램에 등록</summary>
    public static void Register()
    {
#if !UNITY_EDITOR
        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
        key?.SetValue(AppName, $"\"{exePath}\"");
#endif
    }

    /// <summary>시작프로그램에서 제거</summary>
    public static void Unregister()
    {
#if !UNITY_EDITOR
        using var key = Registry.CurrentUser.OpenSubKey(RegistryKey, true);
        key?.DeleteValue(AppName, throwOnMissingValue: false);
#endif
    }
}
