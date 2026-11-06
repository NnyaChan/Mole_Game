using UnityEngine;

//동굴 입구. 상호작용하면 플레이어를 목적지로 이동시킨다.
//지상 동굴과 광산 안 동굴에 하나씩 붙이고, 서로의 도착 지점을 목적지로 연결한다.
public class CaveEntrance : Interactable
{
    [SerializeField] Transform destination; //이동할 위치

    public override void Interact(PlayerManager player)
    {
        if (destination == null)
        {
            Debug.LogWarning($"[상호작용] {name}의 destination이 연결되지 않았습니다.");
            return;
        }

        player.TeleportTo(destination.position);
        Debug.Log($"[상호작용] {name} → {destination.name}");
    }
}
