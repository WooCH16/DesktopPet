using UnityEngine;

/// <summary>
/// Animator 없이 SpriteRenderer를 직접 제어하는 프레임 애니메이터.
/// AnimationStateMachine.PetState에 따라 스프라이트 배열을 순환한다.
/// </summary>
public class PetSpriteAnimator : MonoBehaviour
{
    [Header("스프라이트 배열 (상태별)")]
    [SerializeField] private Sprite[] idleSprites;
    [SerializeField] private Sprite[] walkSprites;
    [SerializeField] private Sprite[] runSprites;
    [SerializeField] private Sprite[] sleepSprites;
    [SerializeField] private Sprite[] sitSprites;
    [SerializeField] private Sprite[] reactSprites;

    [Header("재생 속도")]
    [SerializeField] private float fps = 8f;

    private SpriteRenderer _renderer;
    private AnimationStateMachine.PetState _currentState = AnimationStateMachine.PetState.Idle;
    private int   _frameIndex;
    private float _timer;

    private void Awake()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        Sprite[] frames = GetFrames(_currentState);
        if (frames == null || frames.Length == 0) return;

        _timer += Time.deltaTime;
        if (_timer >= 1f / fps)
        {
            _timer = 0f;
            _frameIndex = (_frameIndex + 1) % frames.Length;
            _renderer.sprite = frames[_frameIndex];
        }
    }

    /// <summary>
    /// AnimationStateMachine에서 호출 — 상태 변경 시 프레임 리셋
    /// </summary>
    public void SetState(AnimationStateMachine.PetState state)
    {
        if (_currentState == state) return;
        _currentState = state;
        _frameIndex   = 0;
        _timer        = 0f;

        // 즉시 첫 프레임 표시
        Sprite[] frames = GetFrames(state);
        if (frames != null && frames.Length > 0)
            _renderer.sprite = frames[0];
    }

    private Sprite[] GetFrames(AnimationStateMachine.PetState state) => state switch
    {
        AnimationStateMachine.PetState.Idle  => idleSprites,
        AnimationStateMachine.PetState.Walk  => walkSprites,
        AnimationStateMachine.PetState.Run   => runSprites,
        AnimationStateMachine.PetState.Sleep => sleepSprites,
        AnimationStateMachine.PetState.Sit   => sitSprites,
        AnimationStateMachine.PetState.React => reactSprites,
        _                                    => idleSprites,
    };
}
