using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class RandomMapGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public Floor floors;
    public List<Vector2> floorCoordinate = new List<Vector2>();
    System.Random autoSeed;
    public Vector2 startPosition;  //층을 생성하는 동안 계속 아래로 내려가는 '작업용' 좌표
    public Vector2 originPosition; //맵 최상단 기준 좌표. 생성이 끝나도 변하지 않는다. (지상/지하, 층 판정의 기준)

    //맵 최상단 줄의 셀 Y좌표
    public int TopCellY => (int)originPosition.y;

    void Awake()
    {
        //다른 스크립트의 Start보다 먼저 기준 좌표가 준비되도록 Awake에서 저장
        originPosition = new Vector2(transform.position.x, transform.position.y);
    }

    void Start()
    {
        autoSeed = new System.Random((int)System.DateTime.Now.Ticks);
        SetMap();
    }

    //맵을 세팅하는 함수
    void SetMap()
    {
        startPosition = originPosition; //시작위치 초기화
        for (int i = 0; i < floors.floorData.Length; i++)
        {
            ShuffleFloorXY(floors.floorData[i].floorWidth, floors.floorData[i].floorLength, autoSeed);
            CreateTile(floors.floorData[i]);
        }
    }

    void CreateTile(FloorData floor)
    {
        int n = floorCoordinate.Count - 1; //좌표 리스트 내에 들어가있는 좌표의 총 수량

        //광물 종류만큼 반복 (뒤쪽 = 희귀한 광물부터 배치)
        for (int i = floor.Minerals.Length - 1; i >= 0; i--)
        {
            MineralData mineral = floor.Minerals[i];
            //System.Random.Next는 max가 제외되므로 +1 (기획서의 '최대 N개'를 포함하기 위함)
            int count = autoSeed.Next(mineral.min, Mathf.Max(mineral.min, mineral.max) + 1);

            for (int k = 0; k < count && n >= 0; k++) //좌표가 소진되면 즉시 종료
            {
                Vector3Int vec = new Vector3Int((int)floorCoordinate[n].x, (int)floorCoordinate[n].y, 0);
                tilemap.SetTile(vec, mineral.mineralTileBase);
                n--;
            }
        }
        //남은 좌표는 전부 기본타일(흙)
        while (n > -1)
        {
            Vector3Int vec = new Vector3Int((int)floorCoordinate[n].x, (int)floorCoordinate[n].y, 0);
            tilemap.SetTile(vec, floor.soilTile);
            n--;
        }
    }

    //층별 리스트 생성 함수
    void ShuffleFloorXY(int floorWidth, int floorLength, System.Random seed)
    {
        floorCoordinate.Clear(); //리스트 초기화

        Vector2 endPosition = new Vector2(startPosition.x + floorWidth, startPosition.y - floorLength); //종료 좌표

        //모든 좌표 긁어오기
        for (int x = (int)startPosition.x; x < (int)endPosition.x; x++)
        {
            for (int y = (int)startPosition.y; y > (int)endPosition.y; y--)
            {
                floorCoordinate.Add(new Vector2(x, y));
            }
        }
        startPosition.y -= floorLength; //다음 층을 위해 세로길이만큼 내려두기

        //피셔-예이츠 셔플
        int n = floorCoordinate.Count;
        while (n > 1)
        {
            n--;
            int k = seed.Next(n + 1);
            Vector2 value = floorCoordinate[k];
            floorCoordinate[k] = floorCoordinate[n];
            floorCoordinate[n] = value;
        }
    }
}

/*
 *  [맵 배치 구현 방식 설계]
 *   0. 해당 층의 범위(타일을 깔 좌표 범위) 가져오기
 *      0.1. 해당 층의 범위 내의 모든 타일 좌표(x, y)를 층별 리스트에 넣기.
 *   1. 광물의 최소/최대치를 가져온 후 랜덤값 뽑기 (이번 일차에 나올 해당 광물의 수)
 *   2. 뽑은 랜덤값만큼 for문을 돌려 광물 랜덤배치. (리스트 뒤에서부터 Pop하는 스택 방식)
 *   3. 해당 층의 광물 배치가 끝났다면, 남은 좌표는 전부 기본타일(흙)으로 배치.
 *   4. 이를 모든 층이 배치될 때 까지 반복.
 *
 *  [참고사항]
 *   - 제너레이터 스크립트가 붙은 오브젝트 자체를 기준좌표로 삼고 X좌표는 +, Y좌표는 -로 해서 우측 하단으로 생성되도록 함.
 *   - 유니티 SetTile 기능을 사용해서 최적화. 절대!!!! 개별 프리팹으로 만들지 말 것!
 *   - 테스트를 고려하면 시드값을 넣어 오류상황 재현이 필요할 수도 있긴 한데, 이건 후순위로 뺄 것.
 *
 *  [변경 이력]
 *   - originPosition 추가: 생성 중 startPosition이 바뀌므로, 맵 최상단 기준점을 따로 보관.
 *   - 난수를 System.Random(autoSeed) 하나로 통일하고, 최대값도 포함되도록 수정.
*/