using System;
using System.Xml.Serialization;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] CapsuleCollider2D collider; //플레이어의 콜라이더
    [SerializeField] Rigidbody2D rigid; //플레이어의 리지드바디
    [SerializeField] SpriteRenderer spriteRenderer; // 플레이어의 스프라이트랜더러
    [SerializeField] private LayerMask groundLayer; //바닥으로 인식할 레이어.
    [SerializeField] Sprite normalSprite; //비워두면 시작 시점의 스프라이트를 사용
    [SerializeField] Sprite climbSprite;  //벽타기 스프라이트

    private float rayGroundDistance = 1f; //레이캐스트를 쏠 거리.
    private float lookAheadDistance = 0.1f; // 레이캐스트를 쏘는 방향으로 보정할 거리
    public Tilemap backTilemap; //뒷배경 타일맵

    [SerializeField] float climbTime = 0f; // 오르기 가능 여부를 체크하는 타이머 역할의 변수 겸 벽타기 종료 가능까지 남은 시간을 체크하는 변수.

    private Vector2 inputVec;
    private float speed = 3f;
    private bool isFliped = false;
    public bool isClimb = false;

    void Start()
    {
        if (normalSprite == null) normalSprite = spriteRenderer.sprite;
    }

    void Update()
    {
        //입력받은 inputVec값을 현 좌표에 더해주는 방식
        if (isClimb && backTilemap.GetTile(backTilemap.WorldToCell(this.transform.position + (new Vector3(inputVec.x, inputVec.y, 0) * speed * Time.deltaTime))) != null)
        {
            this.transform.Translate(new Vector3(inputVec.x, inputVec.y, 0) * speed * Time.deltaTime);
            if (climbTime <= 0 && IsGrounded() && rigid.gravityScale == 0)
            {
                isClimb = false;
                spriteRenderer.sprite = normalSprite;
                rigid.gravityScale = 1;
                Debug.Log("벽타기 모드 종료");
            }
        }
        else if (IsGrounded())
        {
            this.transform.Translate(new Vector3(inputVec.x, 0, 0) * speed * Time.deltaTime);
        }
        else
        {
            Debug.Log("바닥없음");
        }

        if (climbTime > 0) { climbTime -= Time.deltaTime; }
    }

    public void Move(InputAction.CallbackContext context)
    {
        inputVec = context.ReadValue<Vector2>();
        climbTime = 0.5f;
        if (inputVec.x > 0 && isFliped == false)
        {
            spriteRenderer.flipX = true;
            isFliped = true;
        }
        else if (inputVec.x < 0 && isFliped == true)
        {
            spriteRenderer.flipX = false;
            isFliped = false;
        }
    }

    public void ClimbCheck()
    {
        if (climbTime > 0 && !isClimb)
        {
            climbTime = 1f; // 벽타기 시작 후 1초동안 벽타기 중지를 불가하게 만듬.
            isClimb = true;
            spriteRenderer.sprite = climbSprite;
            rigid.gravityScale = 0;
            if (backTilemap.GetTile(backTilemap.WorldToCell(this.transform.position + (Vector3)inputVec)) != null)
            {
                this.transform.Translate(new Vector3(inputVec.x, inputVec.y, 0) * speed * 0.3f);
            }
            Debug.Log("벽타기 모드 시작");
        }
    }

    // 현재 바닥에 닿아있는지 확인하는 함수
    private bool IsGrounded()
    {
        float directionX = Mathf.Sign(inputVec.x); //Mathf.Sign을 이용해 inputVec.x의 부호를 반환한다.

        //콜라이더의 중심에서 나아가는 방향 * lookAheadDistance를 하여 나아가는 방향의 조금 앞쪽 방향을 가져온다.
        Vector2 rayOrigin = new Vector2(collider.bounds.center.x + (directionX * lookAheadDistance), collider.bounds.center.y);

        //rayOrigin의 아래방향으로 rayGroundDistance만큼 아래쪽을 쏴 groundLayer에 닿았는지 확인한다.
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, rayGroundDistance, groundLayer);

        return hit.collider != null; //바닥에 닿아있다면 true 반환
    }

}

/*
 *  [벽타기 매커니즘]
 *  1) 특정 키 (쉬프트+ WASD) 입력시 벽타기 가능한지(블럭 삭제시 뒤에 빈 공간 타일맵 따로 만들어서 해당 타일맵이 있는지 인식) 확인하기.
 *  	- 즉 타일맵을 두개 만들어서 각각 블럭이 차 있는 타일맵과 차 있지 않은 타일맵으로 구분. 타일맵 자체를 인스펙터창에서 연결해서 빈 타일맵쪽이 NULL인지 확인
 *  2. 벽타기 모드 진입
 *      - 빈 공간 타일맵에서 WASD로 이동. 바닥에 닿으면 벽타기 모드 종료. (IsGrounded()를 활용해 바닥인식.)
 *      - 중력을 껐다켰다하는걸로 벽타기 모드-일반 모드를 오가기.
*/
