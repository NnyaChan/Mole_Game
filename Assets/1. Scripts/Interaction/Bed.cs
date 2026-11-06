using UnityEngine;

//침대. 상호작용하면 취침하며 일차를 종료한다. (정산 → 암전 → Day+1 → 자동저장)
public class Bed : Interactable
{
    public override void Interact(PlayerManager player)
    {
        if (GameManager.Instance != null) GameManager.Instance.EndDay(Zone.Surface);
        else Debug.LogWarning("[상호작용] GameManager가 씬에 없어 일차 종료를 처리하지 못했습니다.");
    }
}
