using System;
using System.Xml.Serialization;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] CapsuleCollider2D collider; //플레이어의 콜라이더
    [SerializeField] private LayerMask groundLayer; //바닥으로 인식할 레이어.
    private float rayGroundDistance = 1f; //레이캐스트를 쏠 거리.
    private float lookAheadDistance = 0.5f;

    private SpriteRenderer spriteRenderer;
    private Vector2 inputVec;
    private float speed = 3f;
    private bool isFliped = false;

    void Start()
    {
        spriteRenderer = this.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        //입력받은 inputVec값을 현 좌표에 더해주는 방식
        if (IsGrounded())
        {
            this.transform.Translate(new Vector3(inputVec.x, 0, 0) * speed * Time.deltaTime);
        }
        else
        {
            //Debug.Log("바닥없음"); 
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        inputVec = context.ReadValue<Vector2>();
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

    public void Climbing()
    {
        Debug.Log("벽타기 모드");
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
