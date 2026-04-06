using UnityEngine;

/// <summary>
/// 마우스 클릭 이벤트를 감지해 PetController에 전달한다.
/// </summary>
[RequireComponent(typeof(PetController))]
public class InteractionController : MonoBehaviour
{
    private PetController _pet;

    private void Awake()
    {
        _pet = GetComponent<PetController>();
    }

    /// <summary>
    /// Collider2D가 있는 경우 Unity 이벤트로 클릭 감지.
    /// OnMouseDown은 DragController와 공유된다.
    /// </summary>
    private void OnMouseDown()
    {
        _pet.OnPetClicked();
    }
}
