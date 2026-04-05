using UnityEngine;

/// <summary>
/// 일부 전체화면 앱이 Topmost를 빼앗아가는 케이스를 주기적으로 복구한다.
/// </summary>
public class TopmostKeeper : MonoBehaviour
{
    [SerializeField] private float checkInterval = 3f;
    [SerializeField] private WindowManager windowManager;

    private float _timer;

    private void Awake()
    {
        if (windowManager == null)
            windowManager = Object.FindFirstObjectByType<WindowManager>();
    }

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= checkInterval)
        {
            _timer = 0f;
            windowManager?.ReapplyTopmost();
        }
    }
}
