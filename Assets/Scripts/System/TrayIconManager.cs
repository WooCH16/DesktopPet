using UnityEngine;

/// <summary>
/// Windows 시스템 트레이에 아이콘을 등록하고 우클릭 메뉴를 제공한다.
/// STA 스레드에서 System.Windows.Forms.NotifyIcon을 실행한다.
/// </summary>
public class TrayIconManager : MonoBehaviour
{
    private System.Windows.Forms.NotifyIcon _trayIcon;
    private bool _disposed;

    private void Start()
    {
#if !UNITY_EDITOR
        var thread = new System.Threading.Thread(RunTrayThread);
        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.IsBackground = true;
        thread.Start();
#endif
    }

    private void OnApplicationQuit()
    {
#if !UNITY_EDITOR
        DisposeTray();
#endif
    }

    private void OnDestroy()
    {
#if !UNITY_EDITOR
        DisposeTray();
#endif
    }

    // 중복 Dispose 방지
    private void DisposeTray()
    {
        if (_disposed) return;
        _disposed = true;
        if (_trayIcon != null)
        {
            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }
    }

    // ── STA 스레드 ────────────────────────────────────────────────────────────

    private void RunTrayThread()
    {
        _trayIcon = new System.Windows.Forms.NotifyIcon();

        try
        {
            var iconPath = System.IO.Path.Combine(
                Application.streamingAssetsPath, "TrayIcon.ico");
            _trayIcon.Icon = System.IO.File.Exists(iconPath)
                ? new System.Drawing.Icon(iconPath)
                : System.Drawing.SystemIcons.Application;
        }
        catch
        {
            _trayIcon.Icon = System.Drawing.SystemIcons.Application;
        }

        _trayIcon.Text    = "Desktop Pet";
        _trayIcon.Visible = true;
        _trayIcon.ContextMenuStrip = BuildContextMenu();

        System.Windows.Forms.Application.Run();
    }

    private System.Windows.Forms.ContextMenuStrip BuildContextMenu()
    {
        var menu = new System.Windows.Forms.ContextMenuStrip();

        // 시작프로그램 토글
        var startupItem = new System.Windows.Forms.ToolStripMenuItem("시작 시 자동 실행")
        {
            Checked = StartupManager.IsRegistered()
        };
        startupItem.Click += (s, e) =>
        {
            bool next = !startupItem.Checked;
            if (next) StartupManager.Register();
            else      StartupManager.Unregister();
            startupItem.Checked = next;
        };

        // 종료
        var exitItem = new System.Windows.Forms.ToolStripMenuItem("종료");
        exitItem.Click += (s, e) =>
        {
            DisposeTray();
            System.Windows.Forms.Application.ExitThread();
            UnityMainThreadDispatcher.Enqueue(() => Application.Quit());
        };

        menu.Items.Add(startupItem);
        menu.Items.Add(new System.Windows.Forms.ToolStripSeparator());
        menu.Items.Add(exitItem);
        return menu;
    }
}
