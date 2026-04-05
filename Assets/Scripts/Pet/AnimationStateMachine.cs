using UnityEngine;

/// <summary>
/// 펫 애니메이션 상태 정의 및 Animator 파라미터 제어
/// </summary>
public class AnimationStateMachine : MonoBehaviour
{
    public enum PetState
    {
        Idle,
        Walk,
        Run,
        Sleep,
        Sit,
        React   // 클릭 반응 (일시적)
    }

    // Animator 파라미터 이름 (Animator Controller와 일치해야 함)
    private static readonly int ParamState   = Animator.StringToHash("State");
    private static readonly int ParamReact   = Animator.StringToHash("React");

    [SerializeField] private Animator _animator;

    private PetState _current = PetState.Idle;
    private float    _reactCooldown;
    private const float ReactDuration = 1.2f;

    public PetState Current => _current;
    public bool IsReacting  => _reactCooldown > 0f;

    private void Awake()
    {
        if (_animator == null)
            _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_reactCooldown > 0f)
        {
            _reactCooldown -= Time.deltaTime;
            if (_reactCooldown <= 0f)
            {
                _animator.SetBool(ParamReact, false);
            }
        }
    }

    /// <summary>
    /// 상태를 전환한다. React 중에는 Idle/Walk/Run 전환을 무시한다.
    /// </summary>
    public void SetState(PetState next)
    {
        if (IsReacting && next != PetState.React)
            return;

        if (next == PetState.React)
        {
            _reactCooldown = ReactDuration;
            _animator.SetBool(ParamReact, true);
            return;
        }

        if (_current == next) return;
        _current = next;
        _animator.SetInteger(ParamState, (int)next);
    }

    /// <summary>
    /// 외부에서 강제로 상태를 설정한다 (React 도중도 허용).
    /// </summary>
    public void ForceState(PetState next)
    {
        _reactCooldown = 0f;
        _animator.SetBool(ParamReact, false);
        _current = next;
        _animator.SetInteger(ParamState, (int)next);
    }
}
