using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerManager : MonoBehaviour
{
    [SerializeField] CapsuleCollider2D collider; //플레이어의 콜라이더
    [SerializeField] Rigidbody2D rigid; //플레이어의 리지드바디
    [SerializeField] GameObject frontModel; //지상용(앞모습) 모델. Animator 컨트롤러: Mole_Front
    [SerializeField] GameObject backModel;  //벽타기용(뒷모습) 모델. Animator 컨트롤러: Mole_Back
    [SerializeField] bool facesLeftByDefault = true; //모델이 기본으로 왼쪽을 보고 있는지 (좌우가 반대로 나오면 체크 해제)
    [SerializeField] private LayerMask groundLayer; //바닥으로 인식할 레이어.
    [SerializeField] int surfaceClimbRows = 1; //맵 최상단 위로 벽타기가 가능한 줄 수 (1 = 지상 한 줄)
    Tilemap frontTilemap;
    RandomMapGenerator generator;

    private float rayGroundDistance = 1f; //레이캐스트를 쏠 거리.
    private float lookAheadDistance = 0.1f; // 레이캐스트를 쏘는 방향으로 보정할 거리
    public Tilemap backTilemap; //뒷배경 타일맵

    [SerializeField] float climbTime = 0f; // 오르기 가능 여부를 체크하는 타이머 역할의 변수 겸 벽타기 종료 가능까지 남은 시간을 체크하는 변수.

    private Vector2 inputVec;
    private float speed = 3f;
    private bool isFliped = false;
    public bool isClimb = false;

    //애니메이터 파라미터 (Mole_Front: IsMoving/IsMining, Mole_Back: MoveX/MoveY)
    static readonly int IsMovingHash = Animator.StringToHash("IsMoving");
    static readonly int IsMiningHash = Animator.StringToHash("IsMining");
    static readonly int MoveXHash = Animator.StringToHash("MoveX");
    static readonly int MoveYHash = Animator.StringToHash("MoveY");
    Animator frontAnimator;
    Animator backAnimator;
    MiningController mining;

    void Start()
    {
        frontTilemap = FindAnyObjectByType<TileBreaker>().frontTilemap;
        generator = FindAnyObjectByType<RandomMapGenerator>();
        mining = FindAnyObjectByType<MiningController>();

        if (frontModel == null || backModel == null)
        {
            Debug.LogWarning("[플레이어] frontModel/backModel이 연결되지 않았습니다. 인스펙터에서 연결하세요.");
            return;
        }
        frontAnimator = frontModel.GetComponentInChildren<Animator>(true);
        backAnimator = backModel.GetComponentInChildren<Animator>(true);
        SetClimbModel(false);
    }

    void Update()
    {
        bool walking = false;                 //지상에서 실제로 걷는 중인지 (애니메이션용)
        Vector2 climbMove = Vector2.zero;     //벽타기 중 실제로 움직이는 축 (애니메이션용, 절댓값)

        //입력받은 inputVec값을 현 좌표에 더해주는 방식
        if (isClimb && CanClimbTo(this.transform.position + (new Vector3(inputVec.x, inputVec.y, 0) * speed * Time.deltaTime), true))
        {
            this.transform.Translate(new Vector3(inputVec.x, inputVec.y, 0) * speed * Time.deltaTime);
            climbMove = new Vector2(Mathf.Abs(inputVec.x), Mathf.Abs(inputVec.y));
            if (climbTime <= 0 && IsGrounded() && rigid.gravityScale == 0)
            {
                isClimb = false;
                SetClimbModel(false);
                rigid.gravityScale = 1;
                Debug.Log("벽타기 모드 종료");
            }
        }
        else if (IsGrounded())
        {
            this.transform.Translate(new Vector3(inputVec.x, 0, 0) * speed * Time.deltaTime);
            walking = inputVec.x != 0;
        }
        else
        {
            Debug.Log("바닥없음");
        }

        UpdateAnimation(walking, climbMove);

        if (climbTime > 0) { climbTime -= Time.deltaTime; }
    }

    public void Move(InputAction.CallbackContext context)
    {
        inputVec = context.ReadValue<Vector2>();
        climbTime = 0.5f;
        if (inputVec.x > 0 && isFliped == false)
        {
            isFliped = true;
            ApplyFlip();
        }
        else if (inputVec.x < 0 && isFliped == true)
        {
            isFliped = false;
            ApplyFlip();
        }
    }

    //벽타기 여부에 따라 앞모습/뒷모습 모델 중 하나만 켠다.
    void SetClimbModel(bool climbing)
    {
        if (frontModel == null || backModel == null) return;
        frontModel.SetActive(!climbing);
        backModel.SetActive(climbing);

        if (climbing && backAnimator != null)
        {
            backAnimator.SetFloat(MoveXHash, 0f);
            backAnimator.SetFloat(MoveYHash, 0f);
        }
    }

    //활성화된 모델의 애니메이터에만 파라미터를 보낸다.
    //채굴 중이면 걷기보다 채굴 애니메이션이 우선이다 (트랜지션에서 처리). 벽타기 중에는 채굴 애니메이션 없음.
    void UpdateAnimation(bool walking, Vector2 climbMove)
    {
        if (isClimb)
        {
            if (backAnimator == null) return;
            backAnimator.SetFloat(MoveXHash, climbMove.x);
            backAnimator.SetFloat(MoveYHash, climbMove.y);
        }
        else
        {
            if (frontAnimator == null) return;
            frontAnimator.SetBool(IsMovingHash, walking);
            frontAnimator.SetBool(IsMiningHash, mining != null && mining.IsMining);
        }
    }

    //본(bone) 리깅 모델이라 flipX 대신 모델 루트의 localScale.x로 좌우를 뒤집는다.
    void ApplyFlip()
    {
        float sign = (isFliped == facesLeftByDefault) ? -1f : 1f;
        FlipModel(frontModel, sign);
        FlipModel(backModel, sign);
    }

    void FlipModel(GameObject model, float sign)
    {
        if (model == null) return;
        Vector3 s = model.transform.localScale;
        s.x = Mathf.Abs(s.x) * sign;
        model.transform.localScale = s;
    }

    public void ClimbCheck()
    {
        if (climbTime > 0 && !isClimb)
        {
            climbTime = 1f; // 벽타기 시작 후 1초동안 벽타기 중지를 불가하게 만듬.
            isClimb = true;
            SetClimbModel(true);
            rigid.gravityScale = 0;
            if (backTilemap.GetTile(backTilemap.WorldToCell(this.transform.position + (Vector3)inputVec)) != null)
            {
                this.transform.Translate(new Vector3(inputVec.x, inputVec.y, 0) * speed * 0.3f);
            }
            Debug.Log("벽타기 모드 시작");
        }
    }

    //벽타기로 해당 위치(월드 좌표)로 이동할 수 있는지 판정한다.
    //allowSurfaceRow: true면 맵 최상단 위 지상 줄까지 허용 (이미 벽타기 중일 때),
    //false면 맵 안쪽 칸만 허용 (벽타기를 시작할 때)
    bool CanClimbTo(Vector3 worldPos, bool allowSurfaceRow)
    {
        Vector3Int cell = backTilemap.WorldToCell(worldPos);

        if (cell.x < generator.LeftCellX || cell.x > generator.RightCellX) return false; //맵 가로 범위 밖
        if (cell.y < generator.BottomCellY) return false; //맵 바닥 아래

        //맵 안쪽: 파낸 칸(블록 없음 + 뒷배경 타일 있음)만 가능. 하늘이 보이는 빈 칸은 불가
        if (cell.y <= generator.TopCellY)
        {
            return frontTilemap.GetTile(cell) == null && backTilemap.GetTile(cell) != null;
        }

        //맵 최상단 위(지상 줄): 이미 벽타기 중일 때만, 지상 줄 높이까지 허용
        return allowSurfaceRow
            && cell.y <= generator.TopCellY + surfaceClimbRows
            && frontTilemap.GetTile(cell) == null;
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
