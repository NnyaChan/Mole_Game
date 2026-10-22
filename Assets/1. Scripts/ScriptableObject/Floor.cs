using System;
using UnityEngine;
using UnityEngine.Tilemaps;

//전체 층의 데이터
[CreateAssetMenu(fileName = "New Floors Data", menuName = "MyGame/Floors")]
public class Floor : ScriptableObject
{
    [Header("층 데이터")]
    public FloorData[] floorData;

    //맵 최상단 줄로부터의 깊이(0 = 최상단 줄)를 받아 층 인덱스(0~4)를 반환한다.
    //층별 세로길이를 누적해서 판정하므로 층마다 길이가 달라져도 동작한다.
    public int GetFloorIndex(int depth)
    {
        if (depth < 0) return 0;
        int accumulated = 0;
        for (int i = 0; i < floorData.Length; i++)
        {
            accumulated += floorData[i].floorLength;
            if (depth < accumulated) return i;
        }
        return floorData.Length - 1; //맵 바깥(더 아래)이면 마지막 층으로 취급
    }
}

//각 층의 광물 데이터
[Serializable]
public class FloorData
{
    [Tooltip("해당 층의 가로 칸 수")]
    public int floorWidth;
    [Tooltip("해당 층의 세로 칸 수")]
    public int floorLength;
    [Tooltip("해당 층의 흙 타일")]
    public TileBase soilTile;
    [Tooltip("해당 층의 뒷배경 타일")]
    public TileBase backTile;
    [Tooltip("해당 층의 등장 광물")]
    public MineralData[] Minerals;
}

//각 광물 개별의 데이터
[Serializable]
public class MineralData
{
    [Tooltip("해당 광물의 타일베이스")]
    public TileBase mineralTileBase;
    [Tooltip("층 내 해당 광물의 최소값")]
    public int min;
    [Tooltip("층 내 해당 광물의 최대값 (이 값도 나올 수 있음)")]
    public int max;
}