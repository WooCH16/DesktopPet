using UnityEngine;

/// <summary>
/// 개발 빌드 전용 성능 모니터 — CPU/메모리/FPS를 화면 좌상단에 표시한다.
/// </summary>
public class PerformanceMonitor : MonoBehaviour
{
    [Header("표시 설정")]
    [SerializeField] private bool showInDevelopmentBuild = true;

    private float _fpsTimer;
    private int   _frameCount;
    private float _fps;
    private float _updateInterval = 0.5f;

    private GUIStyle _style;

    private void Awake()
    {
        if (!Debug.isDebugBuild || !showInDevelopmentBuild)
        {
            enabled = false;
        }
    }

    private void Start()
    {
        _style = new GUIStyle
        {
            fontSize  = 14,
            fontStyle = FontStyle.Bold
        };
        _style.normal.textColor = Color.white;
    }

    private void Update()
    {
        _frameCount++;
        _fpsTimer += Time.unscaledDeltaTime;
        if (_fpsTimer >= _updateInterval)
        {
            _fps = _frameCount / _fpsTimer;
            _frameCount = 0;
            _fpsTimer = 0f;
        }
    }

    private void OnGUI()
    {
        long memoryMB = System.GC.GetTotalMemory(false) / (1024 * 1024);
        string info = $"FPS: {_fps:F1}\nMEM: {memoryMB} MB";
        GUI.Label(new Rect(10, 10, 200, 50), info, _style);
    }
}
