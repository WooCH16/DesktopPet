# Animator Controller 설정 가이드

> Unity Editor에서 수동으로 생성해야 하는 항목

## 파일 위치
`Assets/Animations/PetAnimator.controller`

## 파라미터

| 이름    | 타입    | 기본값 |
|---------|---------|--------|
| `State` | Integer | 0      |
| `React` | Bool    | false  |

## State 매핑 (AnimationStateMachine.PetState)

| 값 | 상태  | 애니메이션 클립 |
|----|-------|----------------|
| 0  | Idle  | `Pet_Idle`     |
| 1  | Walk  | `Pet_Walk`     |
| 2  | Run   | `Pet_Run`      |
| 3  | Sleep | `Pet_Sleep`    |
| 4  | Sit   | `Pet_Sit`      |

## 트랜지션 설정

```
Any State → React  (React = true)
React → Idle       (React = false, Exit Time 기반)

Idle  → Walk/Run/Sleep/Sit  (State 값 변경 시 즉시 전환, Has Exit Time: OFF)
Walk  → 모든 상태  (동일)
Run   → 모든 상태  (동일)
```

## 스프라이트 클립 설정

- Loop Time: ON (Idle, Walk, Run, Sleep, Sit)
- Loop Time: OFF (React)
- Sample Rate: 12 fps 권장

## 씬 설정 (Main.unity)

GameObject "DesktopPet"에 다음 컴포넌트 부착:
1. SpriteRenderer (Sprite Mode: Single, Order in Layer: 0)
2. Animator (Controller: PetAnimator)
3. BoxCollider2D (Is Trigger: ON, 크기: 128x128px에 맞게 조정)
4. WindowManager
5. PetController
6. AnimationStateMachine
7. MovementController
8. DragController
9. InteractionController

별도 GameObject "SystemManager":
1. TrayIconManager
2. StartupManager
3. UnityMainThreadDispatcher
4. PerformanceMonitor (개발 빌드 전용)

## 빌드 설정 (File > Build Settings)

- Platform: Windows, x86_64
- Scripting Backend: IL2CPP
- Api Compatibility Level: .NET Framework (System.Windows.Forms 사용)
- Resolution Dialog: Disabled
- Run in Background: ON
- Display Resolution Dialog: Hidden By Default
