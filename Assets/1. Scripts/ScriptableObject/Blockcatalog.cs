using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

//블록 종류. 순서가 곧 표의 열 순서이므로 바꾸지 말 것. (Soil=0, 이후 광물 10종)
public enum BlockType
{
    Soil,
    Stone, Copper, Lead, Iron, Gold, Obsidian, Sapphire, Emerald, Ruby, Diamond
}

//타일 → 블록 종류 매핑 + 곡괭이 단계별 채굴시간 표
[CreateAssetMenu(fileName = "BlockCatalog", menuName = "MyGame/Block Catalog")]
public class BlockCatalog : ScriptableObject
{
    [Serializable]
    public class TileEntry
    {
        public TileBase tile;
        public BlockType type;
    }

    public const int ToolLevelCount = 6; //곡괭이 1~6Lv
    public const int FloorCount = 5;
    const int MineralCount = 10;         //돌 ~ 다이아몬드

    [Header("타일 → 블록 종류 (우클릭 메뉴로 Floor 에셋에서 자동 채우기 가능)")]
    public Floor floorSource;
    public TileEntry[] tiles;

    [Header("광물 채굴시간(초) [도구Lv × 광물]  -1 = 채굴불가, 0 = 즉시")]
    [Tooltip("열 순서: 돌, 구리, 납, 철, 금, 흑요석, 사파이어, 에메랄드, 루비, 다이아몬드 / 행: 1Lv~6Lv")]
    public float[] mineralTimes = new float[ToolLevelCount * MineralCount];

    [Header("흙 채굴시간(초) [도구Lv × 층]  -1 = 채굴불가, 0 = 즉시")]
    [Tooltip("열 순서: 1층~5층 / 행: 1Lv~6Lv")]
    public float[] soilTimes = new float[ToolLevelCount * FloorCount];

    Dictionary<TileBase, BlockType> lookup;

    void OnEnable() { lookup = null; }

    public bool TryGetBlockType(TileBase tile, out BlockType type)
    {
        type = BlockType.Soil;
        if (tile == null) return false;

        if (lookup == null)
        {
            lookup = new Dictionary<TileBase, BlockType>();
            foreach (TileEntry entry in tiles)
            {
                if (entry.tile != null) lookup[entry.tile] = entry.type;
            }
        }
        return lookup.TryGetValue(tile, out type);
    }

    //채굴시간(초)을 반환. -1이면 채굴 불가, 0이면 즉시 채굴.
    //toolLevel은 1~6, floorIndex는 0~4 (흙 블록에만 사용됨. 광물 시간은 층과 무관)
    public float GetMiningTime(int toolLevel, BlockType type, int floorIndex)
    {
        int t = toolLevel - 1;
        if (t < 0 || t >= ToolLevelCount) return -1f;

        if (type == BlockType.Soil)
        {
            if (floorIndex < 0 || floorIndex >= FloorCount) return -1f;
            return ReadTable(soilTimes, t * FloorCount + floorIndex);
        }

        int m = (int)type - 1; //Stone = 0
        return ReadTable(mineralTimes, t * MineralCount + m);
    }

    float ReadTable(float[] table, int index)
    {
        if (table == null || index < 0 || index >= table.Length)
        {
            Debug.LogError($"[BlockCatalog] 표 크기가 맞지 않습니다. 우클릭 메뉴의 '표 기본값으로 채우기'를 실행하세요. (index {index})");
            return -1f;
        }
        return table[index];
    }

    // ───────── 에디터 편의 기능 (인스펙터 우클릭 메뉴) ─────────

    //기획서의 채굴시간 표를 그대로 채운다. 값이 기획서와 맞는지 눈으로 확인할 것.
    [ContextMenu("표 기본값으로 채우기")]
    void FillDefaultTables()
    {
        const float X = -1f; //채굴불가
        mineralTimes = new float[]
        {
            //돌     구리    납     철     금     흑요석  사파이어 에메랄드 루비   다이아
            1.44f, 1.44f, X,     X,     X,     X,     X,     X,     X,     X,     //1Lv
            1.08f, 1.08f, 1.17f, 1.26f, X,     X,     X,     X,     X,     X,     //2Lv
            0.72f, 0.72f, 0.78f, 0.84f, 0.84f, 0.90f, X,     X,     X,     X,     //3Lv (흑요석 0.9는 공식으로 채운 값)
            0.36f, 0.36f, 0.39f, 0.42f, 0.42f, 0.45f, 0.48f, 0.48f, X,     X,     //4Lv
            0.36f, 0.36f, 0.39f, 0.42f, 0.42f, 0.45f, 0.48f, 0.48f, 0.51f, 0.57f, //5Lv
            0f,    0f,    0f,    0f,    0.1f,  0.1f,  0.1f,  0.1f,  0.1f,  0.3f,  //6Lv
        };
        soilTimes = new float[]
        {
            //1층   2층   3층   4층   5층
            1.2f, X,    X,    X,    X,     //1Lv
            0.9f, 1.2f, X,    X,    X,     //2Lv
            0.6f, 0.9f, 1.2f, X,    X,     //3Lv
            0.3f, 0.6f, 0.9f, 1.2f, X,     //4Lv
            0f,   0f,   0.3f, 0.3f, 0.6f,  //5Lv
            0f,   0f,   0.1f, 0.1f, 0.1f,  //6Lv
        };
        MarkDirty();
        Debug.Log("[BlockCatalog] 채굴시간 표를 기본값으로 채웠습니다.");
    }

    //Floor 에셋의 층별 흙/광물 타일을 읽어서 tiles 목록을 자동으로 만든다.
    //광물 순서는 기획서의 층별 광물 목록(희귀도 오름차순)을 가정한다. 로그로 결과를 꼭 확인할 것.
    [ContextMenu("Floor 에셋에서 타일 목록 자동 채우기")]
    void FillTilesFromFloor()
    {
        if (floorSource == null)
        {
            Debug.LogError("[BlockCatalog] floorSource에 Floor 에셋을 먼저 넣어주세요.");
            return;
        }

        BlockType[][] mineralOrder =
        {
            new[] { BlockType.Stone, BlockType.Copper, BlockType.Lead },
            new[] { BlockType.Stone, BlockType.Copper, BlockType.Lead, BlockType.Iron, BlockType.Gold },
            new[] { BlockType.Copper, BlockType.Lead, BlockType.Iron, BlockType.Gold, BlockType.Obsidian, BlockType.Sapphire },
            new[] { BlockType.Iron, BlockType.Gold, BlockType.Obsidian, BlockType.Sapphire, BlockType.Emerald },
            new[] { BlockType.Gold, BlockType.Obsidian, BlockType.Sapphire, BlockType.Emerald, BlockType.Ruby, BlockType.Diamond },
        };

        List<TileEntry> result = new List<TileEntry>();
        for (int i = 0; i < floorSource.floorData.Length && i < mineralOrder.Length; i++)
        {
            FloorData data = floorSource.floorData[i];

            result.Add(new TileEntry { tile = data.soilTile, type = BlockType.Soil });
            Debug.Log($"[BlockCatalog] {i + 1}층 흙: {(data.soilTile != null ? data.soilTile.name : "없음")}");

            if (data.Minerals.Length != mineralOrder[i].Length)
            {
                Debug.LogWarning($"[BlockCatalog] {i + 1}층 광물 수가 예상({mineralOrder[i].Length})과 달라 건너뜀. 직접 입력 필요.");
                continue;
            }
            for (int j = 0; j < data.Minerals.Length; j++)
            {
                TileBase tile = data.Minerals[j].mineralTileBase;
                result.Add(new TileEntry { tile = tile, type = mineralOrder[i][j] });
                Debug.Log($"[BlockCatalog] {i + 1}층 {(tile != null ? tile.name : "없음")} → {mineralOrder[i][j]}");
            }
        }
        tiles = result.ToArray();
        lookup = null;
        MarkDirty();
    }

    void MarkDirty()
    {
#if UNITY_EDITOR
        UnityEditor.EditorUtility.SetDirty(this);
#endif
    }
}