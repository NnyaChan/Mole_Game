using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

//채굴 진행도 관리: 클릭을 누르고 있는 동안 시간이 쌓이고, 다 차면 블록이 파괴된다.
//조준(어느 칸을 노리는가)은 TileBreaker가, 시간 계산은 BlockCatalog가 담당한다.
public class MiningController : MonoBehaviour
{
    [SerializeField] BlockCatalog catalog;
    [SerializeField, Range(1, 6)] int toolLevel = 1; //GameManager가 씬에 없을 때만 쓰는 대체값. 평소엔 PlayerData.pickaxeLevel이 기준
    [SerializeField] bool holdToContinue = true;     //true: 누른 채로 다음 블록으로 넘어가면 계속 채굴
    [SerializeField] int maxRange = 1;               //채굴 가능 거리(칸). 1 = 상하좌우 대각선 1칸

    //TileBreaker에서 가져오는 참조들 (인스펙터에 따로 넣을 필요 없음)
    TileBreaker breaker;
    RandomMapGenerator generator;
    Tilemap frontTilemap;
    Tilemap backTilemap;
    Transform player;
    Floor floor;

    float duration;
    float elapsed;
    Vector3Int rejectedCell;
    bool hasRejected;

    public bool IsMining { get; private set; }
    public Vector3Int LockedCell { get; private set; } //채굴 중인 칸. 조준(TileBreaker.TargetCell)이 이 칸에서 벗어나면 채굴이 리셋된다
    public float Progress => (IsMining && duration > 0f) ? Mathf.Clamp01(elapsed / duration) : 0f; //0~1
    public float Duration => duration;
    public int ToolLevel => (GameManager.Instance != null) ? GameManager.Instance.Data.pickaxeLevel : toolLevel;

    public event Action<Vector3Int, float> OnMiningStarted; //(칸, 걸리는 시간) 애니메이션용
    public event Action OnMiningCancelled;
    public event Action<Vector3Int> OnBlockBroken;
    public event Action<Vector3Int, BlockType> OnBlockMined; //(칸, 블록 종류) 획득 광물 집계용. 카탈로그에 없는 타일은 발생하지 않음

    void Awake()
    {
        breaker = FindAnyObjectByType<TileBreaker>();
        generator = FindAnyObjectByType<RandomMapGenerator>();
        frontTilemap = breaker.frontTilemap;
        backTilemap = breaker.backTilemap;
        player = breaker.player.transform;
        floor = breaker.floor;
    }

    void Update()
    {
        bool pressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
        bool playing = GameManager.Instance == null || GameManager.Instance.IsPlaying;

        // ── 채굴 중: 진행 또는 취소 판정 ──
        if (IsMining)
        {
            if (!playing) { Cancel("일차 종료"); return; }
            if (!pressed) { Cancel("마우스 버튼을 뗌"); return; }
            if (!InRange(LockedCell)) { Cancel("채굴 범위를 벗어남"); return; }
            if (frontTilemap.GetTile(LockedCell) == null) { Cancel("대상 블록이 사라짐"); return; }

            if (breaker.TargetCell != LockedCell)
            {
                //조준 칸이 바뀌면(마우스 이동/플레이어 이동) 진행도를 리셋하고, 아래에서 새 칸으로 다시 시작한다.
                Cancel("조준 칸이 바뀜");
            }
            else
            {
                elapsed += Time.deltaTime;
                if (elapsed >= duration) Complete();
                return;
            }
        }

        // ── 채굴 중이 아님: 시작 조건 확인 ──
        if (!pressed) { hasRejected = false; return; }
        if (!playing || !breaker.IsArmed) return;

        bool startInput = holdToContinue || Mouse.current.leftButton.wasPressedThisFrame;
        if (startInput) TryBegin(breaker.TargetCell);
    }

    void TryBegin(Vector3Int cell)
    {
        if (hasRejected && cell == rejectedCell) return; //같은 칸의 거절 로그가 매 프레임 찍히는 것을 방지

        TileBase tile = frontTilemap.GetTile(cell);
        if (tile == null) return; //빈 칸은 조용히 무시

        if (!InRange(cell)) { Reject(cell, "채굴 범위 밖"); return; }

        if (!catalog.TryGetBlockType(tile, out BlockType type))
        {
            Reject(cell, $"BlockCatalog에 등록되지 않은 타일: {tile.name}");
            return;
        }

        int floorIndex = floor.GetFloorIndex(generator.TopCellY - cell.y);
        float time = catalog.GetMiningTime(ToolLevel, type, floorIndex);

        if (time < 0f)
        {
            Reject(cell, $"채굴 불가: {type} / {floorIndex + 1}층 / 곡괭이 Lv{ToolLevel}");
            return;
        }

        LockedCell = cell;

        if (time == 0f) //즉시 채굴
        {
            Debug.Log($"[채굴 즉시] {type} ({cell.x}, {cell.y}) Lv{ToolLevel}");
            BreakBlock(cell, floorIndex);
            return;
        }

        IsMining = true;
        duration = time;
        elapsed = 0f;
        Debug.Log($"[채굴 시작] {type} ({cell.x}, {cell.y}) {time:F2}초 / 곡괭이 Lv{ToolLevel}");
        OnMiningStarted?.Invoke(cell, time);
    }

    void Complete()
    {
        IsMining = false;
        int floorIndex = floor.GetFloorIndex(generator.TopCellY - LockedCell.y);
        Debug.Log($"[채굴 완료] ({LockedCell.x}, {LockedCell.y})");
        BreakBlock(LockedCell, floorIndex);
    }

    void Cancel(string reason)
    {
        IsMining = false;
        elapsed = 0f;
        Debug.Log($"[채굴 취소] {reason}");
        OnMiningCancelled?.Invoke();
    }

    void Reject(Vector3Int cell, string reason)
    {
        rejectedCell = cell;
        hasRejected = true;
        Debug.Log($"[채굴 거절] {reason}");
    }

    //실제 파괴 처리. 기존 TileBreaker.ClickBlock의 파괴 로직과 동일하다.
    void BreakBlock(Vector3Int cell, int floorIndex)
    {
        bool known = catalog.TryGetBlockType(frontTilemap.GetTile(cell), out BlockType type); //타일을 지우기 전에 종류를 확인
        backTilemap.SetTile(cell, floor.floorData[floorIndex].backTile);
        frontTilemap.SetTile(cell, null);
        OnBlockBroken?.Invoke(cell);
        if (known) OnBlockMined?.Invoke(cell, type);
    }

    bool InRange(Vector3Int cell)
    {
        Vector3Int p = frontTilemap.WorldToCell(player.position);
        return Mathf.Abs(cell.x - p.x) <= maxRange && Mathf.Abs(cell.y - p.y) <= maxRange;
    }
}