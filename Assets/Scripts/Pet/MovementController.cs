using UnityEngine;

/// <summary>
/// 펫의 자율 이동 AI — 랜덤 방향·속도로 바탕화면 경계 내에서 이동한다.
/// </summary>
[RequireComponent(typeof(PetController))]
public class MovementController : MonoBehaviour
{
    [Header("이동 속도")]
    [SerializeField] private float walkSpeed = 60f;   // px/s
    [SerializeField] private float runSpeed  = 140f;  // px/s

    [Header("상태 전환 시간")]
    [SerializeField] private float minIdleTime = 2f;
    [SerializeField] private float maxIdleTime = 6f;
    [SerializeField] private float minMoveTime = 2f;
    [SerializeField] private float maxMoveTime = 5f;

    [Header("펫 크기 (px)")]
    [SerializeField] private int petWidth  = 128;
    [SerializeField] private int petHeight = 128;

    private PetController _pet;
    private bool   _paused;
    private float  _stateTimer;
    private bool   _isMoving;
    private bool   _isRunning;
    private Vector2 _direction;
    private Vector2 _screenPos;   // 화면 기준 픽셀 좌표

    // 화면 경계
    private int _screenW;
    private int _screenH;

    private void Awake()
    {
        _pet = GetComponent<PetController>();
    }

    private void Start()
    {
        // 다중 모니터 가상 데스크탑 기준으로 경계 설정
        var desktop = MultiMonitorSupport.GetBounds();
        _screenW = desktop.width;
        _screenH = desktop.height;

        // 시작 위치: 주 모니터 중앙
        _screenPos = new Vector2(
            Screen.currentResolution.width  / 2f - petWidth  / 2f,
            Screen.currentResolution.height / 2f - petHeight / 2f);

        EnterIdle();
    }

    private void Update()
    {
        if (_paused) return;

        _stateTimer -= Time.deltaTime;

        if (_stateTimer <= 0f)
            SwitchState();

        if (_isMoving)
            Move();
    }

    public void Pause()  => _paused = true;
    public void Resume() => _paused = false;

    // ── 상태 전환 ──────────────────────────────────────────────────────────────

    private void SwitchState()
    {
        if (_isMoving)
            EnterIdle();
        else
            EnterMove();
    }

    private void EnterIdle()
    {
        _isMoving  = false;
        _stateTimer = Random.Range(minIdleTime, maxIdleTime);

        // Sleep/Sit을 가끔 재생
        float roll = Random.value;
        var state = roll < 0.3f
            ? AnimationStateMachine.PetState.Sleep
            : roll < 0.5f
                ? AnimationStateMachine.PetState.Sit
                : AnimationStateMachine.PetState.Idle;

        _pet.OnMovementStateChanged(state);
    }

    private void EnterMove()
    {
        _isMoving  = true;
        _isRunning = Random.value < 0.25f;
        _stateTimer = Random.Range(minMoveTime, maxMoveTime);
        _direction  = RandomDirection();

        var state = _isRunning
            ? AnimationStateMachine.PetState.Run
            : AnimationStateMachine.PetState.Walk;
        _pet.OnMovementStateChanged(state);
    }

    // ── 이동 ──────────────────────────────────────────────────────────────────

    private void Move()
    {
        float speed = _isRunning ? runSpeed : walkSpeed;
        _screenPos += _direction * (speed * Time.deltaTime);
        ClampToBounds();
        ApplyWindowPosition();
    }

    private void ClampToBounds()
    {
        bool bounced = false;

        // 좌/우 경계
        if (_screenPos.x < 0f)
        {
            _screenPos.x = 0f;
            _direction.x = Mathf.Abs(_direction.x);
            bounced = true;
        }
        else if (_screenPos.x > _screenW - petWidth)
        {
            _screenPos.x = _screenW - petWidth;
            _direction.x = -Mathf.Abs(_direction.x);
            bounced = true;
        }

        // 상/하 경계
        if (_screenPos.y < 0f)
        {
            _screenPos.y = 0f;
            _direction.y = Mathf.Abs(_direction.y);
            bounced = true;
        }
        else if (_screenPos.y > _screenH - petHeight)
        {
            _screenPos.y = _screenH - petHeight;
            _direction.y = -Mathf.Abs(_direction.y);
            bounced = true;
        }

        if (bounced)
            _direction = _direction.normalized;
    }

    private void ApplyWindowPosition()
    {
        // Unity 스크린 좌표계 → Windows 픽셀 좌표계 변환 (Y 반전)
        int winX = Mathf.RoundToInt(_screenPos.x);
        int winY = Mathf.RoundToInt(_screenH - _screenPos.y - petHeight);
        WindowPositioner.SetPosition(winX, winY);
    }

    private static Vector2 RandomDirection()
    {
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
    }

    /// <summary>드래그 종료 시 위치 동기화 (DragController에서 호출)</summary>
    public void SyncPosition(Vector2 screenPos)
    {
        _screenPos = screenPos;
    }
}
