using UnityEngine;
using UnityEngine.InputSystem;

//상호작용 키를 누르면 플레이어 주변의 상호작용 오브젝트를 찾아 실행한다.
public class PlayerInteractor : MonoBehaviour
{
    [SerializeField] PlayerManager player;
    [SerializeField] LayerMask interactableLayer;  //상호작용 판정 영역의 레이어
    [SerializeField] float interactRadius = 0.5f;  //플레이어 위치에서 상호작용 영역을 찾는 반경

    void Awake()
    {
        if (player == null) player = GetComponent<PlayerManager>();
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (!context.performed || player.InputLocked) return;

        Collider2D hit = Physics2D.OverlapCircle(transform.position, interactRadius, interactableLayer);
        if (hit != null && hit.TryGetComponent(out Interactable interactable))
        {
            interactable.Interact(player);
            Debug.Log("e키 눌림");
        }
    }
}
