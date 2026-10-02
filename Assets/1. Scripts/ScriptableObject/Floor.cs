using System;
using System.Runtime.ExceptionServices;
using JetBrains.Annotations;
using UnityEditor;
using UnityEditor.EditorTools;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.Tilemaps;

//전체 층의 데이터
[CreateAssetMenu(fileName = "New Floors Data", menuName = "MyGame/Floors")]
public class Floor : ScriptableObject
{
    [Header("1층")]
    public FloorData floor1;
    [Header("2층")]
    public FloorData floor2;
    [Header("3층")]
    public FloorData floor3;
    [Header("4층")]
    public FloorData floor4;
    [Header("5층")]
    public FloorData floor5;
}

//각 층의 광물 데이터
[Serializable]
public class FloorData
{
    [Tooltip("해당 층의 가로 칸 수")]
    public int floorWidth;
    [Tooltip("해당 층의 세로 칸 수")]
    public int floorLength;
    [Tooltip("해당 층의 흙 타일 베이스")]
    public TileBase soil;
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
    [Tooltip("층 내 해당 광물의 최대값")]
    public int max;
}
