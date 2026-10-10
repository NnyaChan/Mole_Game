using UnityEngine;
using UnityEngine.InputSystem;

public class TileBreaker : MonoBehaviour
{
    public GameObject player; //플레이어를 담을 변수
    Vector2 mouseTrans;
    Vector3 worldMouseTrans; //로컬좌표로 변환한 마우스좌표
    Vector3 direction; // 방향벡터를 담을 변수
    float angle; //방향계산을 위한 각도를 담을 변수
    public int directionIndex;

    void Update()
    {
        worldMouseTrans = Camera.main.ScreenToWorldPoint(mouseTrans); //월드좌표로 좌표변경
        direction = worldMouseTrans - player.transform.position; //방향벡터 계산
        angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg; //각도 계산
        if (angle < 0) { angle += 360f; } //-180~180이 아닌 360도 기준으로 변경
        directionIndex = Mathf.RoundToInt(angle / 45f) % 8; // 0이 위쪽방향, 그로부터 차례대로 시계방향으로.
    }

    public void MouseXY(InputAction.CallbackContext context)
    {
        mouseTrans = context.ReadValue<Vector2>();
    }
}
