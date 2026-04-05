using UnityEngine;

/// <summary>
/// 마우스 드래그로 펫(윈도우)을 이동시킨다.
/// 드래그 임계값(threshold)으로 단순 클릭과 드래그를 구분한다.
/// </summary>
[RequireComponent(typeof(MovementController))]
public class DragController : MonoBehaviour
{
    [SerializeField] private int   petHeight      = 128;
    [SerializeField] private float dragThreshold  = 8f;   // px — 이 거리 이상 움직여야 드래그 시작

    private MovementController _movement;
    private PetSleepOptimizer  _optimizer;

    private bool    _mouseDown;     // 마우스 버튼 눌림
    private bool    _isDragging;    // 실제 드래그 중 (threshold 초과)
    private Vector2 _mouseDownPos;  // 마우스 버튼 누른 위치 (Windows 좌표계)
    private Vector2 _dragOffset;    // 마우스 ↔ 윈도우 좌상단 오프셋

    public bool IsDragging => _isDragging;

    private void Awake()
    {
        _movement  = GetComponent<MovementController>();
        _optimizer = GetComponent<PetSleepOptimizer>();
    }

    private void Update()
    {
        if (!_mouseDown) return;

        Vector2 currentMouse = MouseScreenPos();

        if (!_isDragging)
        {
            // threshold 초과 시 드래그 시작
            if (Vector2.Distance(currentMouse, _mouseDownPos) >= dragThreshold)
                BeginDrag(currentMouse);
        }
        else
        {
            HandleDrag(currentMouse);
        }
    }

    private void OnMouseDown()
    {
        _mouseDown    = true;
        _mouseDownPos = MouseScreenPos();

        // 드래그 오프셋은 미리 계산 (BeginDrag 시 재사용)
        Vector2 winPos = WindowPositioner.GetPosition();
        _dragOffset = winPos - _mouseDownPos;
    }

    private void OnMouseUp()
    {
        if (_isDragging)
            EndDrag();

        _mouseDown  = false;
        _isDragging = false;
    }

    // ── 드래그 시작/종료 ───────────────────────────────────────────────────────

    private void BeginDrag(Vector2 currentMouse)
    {
        _isDragging = true;
        // Optimizer 비활성화 후 60fps 고정
        if (_optimizer != null) _optimizer.enabled = false;
        Application.targetFrameRate = 60;
    }

    private void EndDrag()
    {
        // Optimizer에 프레임레이트 제어 반환
        if (_optimizer != null) _optimizer.enabled = true;

        // MovementController에 현재 위치 동기화
        Vector2 winPos  = WindowPositioner.GetPosition();
        int     screenH = Screen.currentResolution.height;
        float   unityY  = screenH - winPos.y - petHeight;
        _movement.SyncPosition(new Vector2(winPos.x, unityY));
    }

    private void HandleDrag(Vector2 currentMouse)
    {
        Vector2 newPos = currentMouse + _dragOffset;
        WindowPositioner.SetPosition(Mathf.RoundToInt(newPos.x), Mathf.RoundToInt(newPos.y));
    }

    // ── 유틸 ──────────────────────────────────────────────────────────────────

    /// <summary>Unity 마우스 위치를 Windows 화면 좌표계로 변환한다.</summary>
    private static Vector2 MouseScreenPos()
    {
        int     screenH = Screen.currentResolution.height;
        Vector3 mp      = Input.mousePosition;
        return new Vector2(mp.x, screenH - mp.y);
    }
}
