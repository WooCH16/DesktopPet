using UnityEngine;

/// <summary>
/// Idle/Sleep 상태에서 targetFrameRate를 낮춰 CPU 사용량을 줄인다.
/// 목표: Idle 1% 미만 (T-05-a)
/// </summary>
public class PetSleepOptimizer : MonoBehaviour
{
    [SerializeField] private AnimationStateMachine _anim;
    [SerializeField] private int activeFps = 60;
    [SerializeField] private int idleFps   = 15;
    [SerializeField] private int sleepFps  = 8;

    private AnimationStateMachine.PetState _lastState;

    private void Awake()
    {
        if (_anim == null)
            _anim = GetComponent<AnimationStateMachine>();

        Application.targetFrameRate = activeFps;
        QualitySettings.vSyncCount  = 0;
    }

    private void Update()
    {
        var state = _anim.Current;
        if (state == _lastState) return;
        _lastState = state;

        Application.targetFrameRate = state switch
        {
            AnimationStateMachine.PetState.Sleep => sleepFps,
            AnimationStateMachine.PetState.Idle  => idleFps,
            AnimationStateMachine.PetState.Sit   => idleFps,
            _                                    => activeFps
        };
    }
}
