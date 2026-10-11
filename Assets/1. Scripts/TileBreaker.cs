using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class TileBreaker : MonoBehaviour
{
    public GameObject player; //플레이어를 담을 변수
    public Tilemap tilemap;
    public Image pointer;
    Vector2 mouseTrans;
    Vector3 worldMouseTrans; //로컬좌표로 변환한 마우스좌표
    Vector3 direction; // 방향벡터를 담을 변수
    Vector3Int targetCell;
    float angle; //방향계산을 위한 각도를 담을 변수
    public int directionIndex;

    void Update()
    {
        worldMouseTrans = Camera.main.ScreenToWorldPoint(mouseTrans); //월드좌표로 좌표변경
        direction = worldMouseTrans - player.transform.position; //방향벡터 계산
        angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg; //각도 계산
        if (angle < 0) { angle += 360f; } //-180~180이 아닌 360도 기준으로 변경
        directionIndex = Mathf.RoundToInt(angle / 45f) % 8; // 0이 위쪽방향, 그로부터 차례대로 시계방향으로.

        targetCell = tilemap.WorldToCell(player.transform.position); //중앙 셀 좌표(기준좌표)

        switch (directionIndex)
        {
            //인덱스 값에 따라 셀에 연산 더하기
            case 0:
                targetCell += new Vector3Int(0, 1, 0);
                break;
            case 1:
                targetCell += new Vector3Int(1, 1, 0);
                break;
            case 2:
                targetCell += new Vector3Int(1, 0, 0);
                break;
            case 3:
                targetCell += new Vector3Int(1, -1, 0);
                break;
            case 4:
                targetCell += new Vector3Int(0, -1, 0);
                break;
            case 5:
                targetCell += new Vector3Int(-1, -1, 0);
                break;
            case 6:
                targetCell += new Vector3Int(-1, 0, 0);
                break;
            case 7:
                targetCell += new Vector3Int(-1, 1, 0);
                break;
            default:
                break;
        }

        //테스트 후 Canvas로 옮길 것.
        pointer.rectTransform.position = Camera.main.WorldToScreenPoint(tilemap.GetCellCenterWorld(targetCell));
        pointer.color = (tilemap.GetTile(targetCell) != null) ? new Color(0, 0, 1f, 1f) : new Color(1f, 0, 0, 1f);

    }

    public void MouseXY(InputAction.CallbackContext context)
    {
        mouseTrans = context.ReadValue<Vector2>();
    }

    public void ClickBlock(InputAction.CallbackContext context)
    {
        if ((tilemap.GetTile(targetCell) != null) && context.performed)
        {
            tilemap.SetTile(targetCell, null);
        }
    }
}
