using UnityEngine;

/// <summary>
/// Animator.Play()로 직접 클립을 재생한다.
/// Walk/Run → Idle 전환 시 현재 사이클이 끝날 때까지 대기한다.
/// </summary>
public class AnimationStateMachine : MonoBehaviour
{
    public enum PetState { Idle, Walk, Run, Sleep, Sit, React }

    private static readonly string[] StateNames = { "Idle", "Walk", "Run", "Sleep", "Sit", "React" };

    [SerializeField] private Animator _animator;           // 있으면 사용, 없어도 무방
    [SerializeField] private PetSpriteAnimator _spriteAnim; // 직접 프레임 제어

    private PetState _current      = PetState.Idle;
    private PetState _beforeReact  = PetState.Idle;
    private PetState? _pending     = null;   // 사이클 끝 후 전환 대기 상태
    private float    _reactCooldown;
    private const float ReactDuration  = 1.2f;
    private const float CycleEndThreshold = 0.85f; // normalizedTime 이 이 값 이상이면 "사이클 끝"으로 판단

    public PetState Current    => _current;
    public bool     IsReacting => _reactCooldown > 0f;

    private void Awake()
    {
        if (_animator    == null) _animator    = GetComponent<Animator>();
        if (_spriteAnim  == null) _spriteAnim  = GetComponent<PetSpriteAnimator>();
    }

    private void Start() => PlayState(PetState.Idle);

    private void Update()
    {
        // React 쿨다운
        if (_reactCooldown > 0f)
        {
            _reactCooldown -= Time.deltaTime;
            if (_reactCooldown <= 0f)
            {
                _pending = null;
                PlayState(_beforeReact);
            }
            return;
        }

        // 대기 중인 상태 전환: 현재 클립 사이클이 끝에 도달하면 전환
        if (_pending.HasValue)
        {
            var info = _animator.GetCurrentAnimatorStateInfo(0);
            float normalized = info.normalizedTime % 1f;
            // normalizedTime이 threshold를 넘었거나 루프가 1회 이상 돈 경우
            if (normalized >= CycleEndThreshold || info.normalizedTime >= 1f)
            {
                PlayState(_pending.Value);
                _pending = null;
            }
        }
    }

    /// <summary>
    /// 상태 전환 요청.
    /// Walk/Run → Idle/Sleep/Sit 전환은 현재 사이클 끝까지 대기한다.
    /// </summary>
    public void SetState(PetState next)
    {
        if (IsReacting && next != PetState.React) return;

        if (next == PetState.React)
        {
            _beforeReact   = _pending ?? _current; // 대기 중이면 대기 상태 기준
            _reactCooldown = ReactDuration;
            _pending       = null;
            PlayState(PetState.React);
            return;
        }

        if (_current == next && !_pending.HasValue) return;

        // Walk/Run 중 Idle/Sleep/Sit 요청 → 사이클 끝까지 대기
        bool isMoving  = _current == PetState.Walk || _current == PetState.Run;
        bool isResting = next == PetState.Idle || next == PetState.Sleep || next == PetState.Sit;

        if (isMoving && isResting)
        {
            _pending = next;  // 바로 전환하지 않고 대기
            return;
        }

        // 그 외 전환은 즉시 처리
        _pending = null;
        PlayState(next);
    }

    /// <summary>
    /// 강제 전환 — 대기 없이 즉시 적용 (드래그 등).
    /// </summary>
    public void ForceState(PetState next)
    {
        _reactCooldown = 0f;
        _pending       = null;
        PlayState(next);
    }

    private void PlayState(PetState state)
    {
        _current = state;
        _spriteAnim?.SetState(state);                               // 직접 프레임 제어
        _animator?.Play(StateNames[(int)state], 0, 0f);            // Animator도 동기화 (있으면)
    }
}
