using UnityEngine;

/// <summary>
/// 펫 최상위 컨트롤러 — 드래그·자율이동·클릭반응을 조율한다.
/// </summary>
[RequireComponent(typeof(AnimationStateMachine))]
[RequireComponent(typeof(MovementController))]
[RequireComponent(typeof(DragController))]
[RequireComponent(typeof(InteractionController))]
public class PetController : MonoBehaviour
{
    private AnimationStateMachine _anim;
    private MovementController    _movement;
    private DragController        _drag;
    private InteractionController _interaction;

    private void Awake()
    {
        _anim        = GetComponent<AnimationStateMachine>();
        _movement    = GetComponent<MovementController>();
        _drag        = GetComponent<DragController>();
        _interaction = GetComponent<InteractionController>();
    }

    private bool _wasDragging;

    private void Update()
    {
        bool dragging = _drag.IsDragging;

        if (dragging)
        {
            if (!_wasDragging)
            {
                _movement.Pause();
                _anim.ForceState(AnimationStateMachine.PetState.Idle);
            }
        }
        else if (_wasDragging)
        {
            _movement.Resume();
        }

        _wasDragging = dragging;
    }

    /// <summary>
    /// InteractionController에서 클릭 이벤트를 수신
    /// </summary>
    public void OnPetClicked()
    {
        _anim.SetState(AnimationStateMachine.PetState.React);
    }

    /// <summary>
    /// MovementController에서 상태 변경 요청 수신
    /// </summary>
    public void OnMovementStateChanged(AnimationStateMachine.PetState state)
    {
        _anim.SetState(state);
    }
}
