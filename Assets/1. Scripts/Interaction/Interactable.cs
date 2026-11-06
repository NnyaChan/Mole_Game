using UnityEngine;

//플레이어가 상호작용할 수 있는 오브젝트의 기반 클래스.
//타일맵은 그림만 담당하고, 이 컴포넌트를 트리거 콜라이더가 있는 빈 오브젝트에 붙여 판정 영역으로 쓴다.
public abstract class Interactable : MonoBehaviour
{
    public abstract void Interact(PlayerManager player);
}
