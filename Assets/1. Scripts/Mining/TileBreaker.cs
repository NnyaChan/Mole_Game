using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

//조준 담당: 마우스 방향으로 타겟 셀을 정하고 포인터를 표시한다.
//실제 채굴(시간, 취소, 파괴)은 MiningController가 처리한다.
public class TileBreaker : MonoBehaviour
{
    public GameObject player; //플레이어를 담을 변수
    public Tilemap frontTilemap; //광물 타일맵
    public Tilemap backTilemap; //뒷배경 타일맵
    public Image pointer;
    public RandomMapGenerator generator; //(기존 인스펙터 연결 유지용. 현재는 MiningController가 사용)
    Vector2 mouseTrans;
    Vector3 worldMouseTrans; //월드좌표로 변환한 마우스좌표
    Vector3 direction; // 방향벡터를 담을 변수
    Vector3Int targetCell;
    float angle; //방향계산을 위한 각도를 담을 변수
    public int directionIndex;
    public Floor floor; //플로어 데이터 가져오기

    //플레이 시작 시 눌려 있던 클릭이 채굴로 이어지는 것을 막는 플래그.
    //마우스 버튼이 한 번 떼어진 것을 확인한 뒤부터 입력을 받는다.
    bool armed = false;

    public Vector3Int TargetCell => targetCell; //MiningController가 읽는 타겟 셀
    public bool IsArmed => armed;

    //8방향(0=위, 시계방향)에 대응하는 셀 오프셋
    static readonly Vector3Int[] Directions =
    {
        new Vector3Int(0, 1, 0),
        new Vector3Int(1, 1, 0),
        new Vector3Int(1, 0, 0),
        new Vector3Int(1, -1, 0),
        new Vector3Int(0, -1, 0),
        new Vector3Int(-1, -1, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int(-1, 1, 0),
    };

    void Awake()
    {
        if (generator == null) generator = FindAnyObjectByType<RandomMapGenerator>();
    }

    void Update()
    {
        if (!armed && Mouse.current != null && !Mouse.current.leftButton.isPressed)
        {
            armed = true;
        }

        //일차 종료 연출(정산/암전) 중에는 조준 포인터를 숨기고 조준 갱신을 멈춘다.
        bool locked = GameManager.Instance != null && !GameManager.Instance.IsPlaying;
        if (pointer.enabled == locked) pointer.enabled = !locked;
        if (locked) return;

        worldMouseTrans = Camera.main.ScreenToWorldPoint(mouseTrans); //월드좌표로 좌표변경
        direction = worldMouseTrans - player.transform.position; //방향벡터 계산
        angle = Mathf.Atan2(direction.x, direction.y) * Mathf.Rad2Deg; //각도 계산
        if (angle < 0) { angle += 360f; } //-180~180이 아닌 360도 기준으로 변경
        directionIndex = Mathf.RoundToInt(angle / 45f) % 8; // 0이 위쪽방향, 그로부터 차례대로 시계방향으로.

        targetCell = frontTilemap.WorldToCell(player.transform.position) + Directions[directionIndex];

        //현 시점에서는 단순히 타일의 유무로 표기되어있으나 추후 필요에 따라 태그로 변경이 필요.
        pointer.rectTransform.position = Camera.main.WorldToScreenPoint(frontTilemap.GetCellCenterWorld(targetCell));
        pointer.color = (frontTilemap.GetTile(targetCell) != null) ? new Color(0, 0, 1f, 1f) : new Color(1f, 0, 0, 1f);
    }

    public void MouseXY(InputAction.CallbackContext context)
    {
        mouseTrans = context.ReadValue<Vector2>();
    }

    //이전에는 여기서 바로 블록을 부쉈지만, 이제 MiningController가 마우스 버튼 상태를 직접 읽어 처리한다.
    //PlayerInput의 이벤트 연결이 끊기지 않도록 함수만 남겨둔다. (나중에 이벤트 연결을 지우고 이 함수도 삭제해도 됨)
    public void ClickBlock(InputAction.CallbackContext context) { }
}