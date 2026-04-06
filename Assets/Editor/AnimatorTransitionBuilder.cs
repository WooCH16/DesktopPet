using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

/// <summary>
/// [COAT 도구] Animator.Play() 방식용 — 모든 트랜지션을 제거한다.
/// 파라미터도 불필요하므로 제거한다.
/// 메뉴: Tools > COAT > Clear All Transitions
/// </summary>
public static class AnimatorTransitionBuilder
{
    private const string ControllerPath = "Assets/Animations/PetAnimator.controller";

    [MenuItem("Tools/COAT/Clear All Transitions")]
    public static void ClearAllTransitions()
    {
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (ctrl == null)
        {
            EditorUtility.DisplayDialog("COAT", "PetAnimator.controller를 찾을 수 없습니다.", "확인");
            return;
        }

        var sm = ctrl.layers[0].stateMachine;

        // Any State 트랜지션 전부 제거
        var anyTrans = sm.anyStateTransitions;
        for (int i = anyTrans.Length - 1; i >= 0; i--)
            sm.RemoveAnyStateTransition(anyTrans[i]);

        // 각 상태의 트랜지션 전부 제거
        foreach (var cs in sm.states)
        {
            var trans = cs.state.transitions;
            for (int i = trans.Length - 1; i >= 0; i--)
                cs.state.RemoveTransition(trans[i]);
        }

        // 파라미터 전부 제거 (Animator.Play()는 파라미터 불필요)
        var paramsCopy = ctrl.parameters;
        for (int i = paramsCopy.Length - 1; i >= 0; i--)
            ctrl.RemoveParameter(paramsCopy[i]);

        EditorUtility.SetDirty(ctrl);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("[COAT] 모든 트랜지션 + 파라미터 제거 완료");
        EditorUtility.DisplayDialog("COAT", "모든 트랜지션 제거 완료!\n\nPlay Mode에서 다시 확인하세요.", "확인");
    }

    [MenuItem("Tools/COAT/Diagnose Animator")]
    public static void Diagnose()
    {
        var ctrl = AssetDatabase.LoadAssetAtPath<AnimatorController>(ControllerPath);
        if (ctrl == null) { Debug.LogError("[COAT] Controller 없음"); return; }

        var sm = ctrl.layers[0].stateMachine;

        Debug.Log($"[COAT] 상태 ({sm.states.Length}개):");
        foreach (var cs in sm.states)
            Debug.Log($"  [{cs.state.name}] motion={cs.state.motion?.name ?? "없음"} transitions={cs.state.transitions.Length}");

        Debug.Log($"[COAT] AnyState 트랜지션: {sm.anyStateTransitions.Length}개");
        Debug.Log($"[COAT] 파라미터: {ctrl.parameters.Length}개");
    }
}
