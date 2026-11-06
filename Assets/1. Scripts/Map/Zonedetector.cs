using System;
using UnityEngine;
using UnityEngine.Tilemaps;

//일차 종료 시 상태 판정용
public enum Zone
{
    Surface,     //지상 판정 (맵 밖이거나, 맵 상단 returnableRows줄 이내 = 귀환 가능)
    Underground  //지하 판정 (귀환 불가 깊이)
}

public class ZoneDetector : MonoBehaviour
{
    [SerializeField] Transform player;
    [SerializeField] Tilemap frontTilemap;          //TileBreaker와 같은 기준으로 셀 좌표를 얻기 위함
    [SerializeField] RandomMapGenerator generator;  //맵 최상단 기준 좌표
    [SerializeField] int returnableRows = 4;        //맵 상단에서 이 줄 수 이내면 '지상 판정'
    [SerializeField] bool logDepth = false;         //깊이가 바뀔 때마다 로그 (기준 확인용)

    //맵 최상단 줄로부터의 깊이. 최상단 줄 = 0, 맵 위(지상) = 음수
    public int Depth { get; private set; } = -1;

    //동굴 상호작용으로 광산에 들어와 있는가? (랜턴이 닳고 포인터가 켜지는 조건, 씬을 다시 불러오면 지상에서 시작)
    public bool InMine { get; private set; } = false;

    //종료 시 판정용. 최상단 returnableRows줄 이내 또는 맵 밖이면 지상
    public Zone Zone => (Depth < returnableRows) ? Zone.Surface : Zone.Underground;

    public event Action<bool> OnInMineChanged;
    public event Action<Zone> OnZoneChanged;

    Zone lastZone = Zone.Surface;

    void Awake()
    {
        if (generator == null) generator = FindAnyObjectByType<RandomMapGenerator>();
    }

    void Update()
    {
        int playerCellY = frontTilemap.WorldToCell(player.position).y;
        int newDepth = generator.TopCellY - playerCellY;

        if (newDepth != Depth)
        {
            Depth = newDepth;
            if (logDepth) Debug.Log($"[깊이] {Depth}");
        }

        if (Zone != lastZone)
        {
            lastZone = Zone;
            Debug.Log($"[구역] 판정 변경: {(Zone == Zone.Surface ? "지상(귀환 가능)" : "지하(귀환 불가)")}");
            OnZoneChanged?.Invoke(Zone);
        }
    }

    //동굴 상호작용으로 광산에 들어가거나 나올 때 호출된다.
    public void SetInMine(bool value)
    {
        if (InMine == value) return;
        InMine = value;
        Debug.Log(InMine ? "[구역] 광산 진입 (랜턴 소모 시작)" : "[구역] 광산 밖으로 나옴 (랜턴 정지)");
        OnInMineChanged?.Invoke(InMine);
    }
}