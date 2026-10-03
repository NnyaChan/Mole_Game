using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;

public class RandomMapGenerator : MonoBehaviour
{
    public Tilemap tilemap;
    public Floor floors;
    public List<Vector2> floorCoordinate = new List<Vector2>();
    System.Random autoSeed;
    public Vector2 startPosition; //시작 좌표

    void Start()
    {
        autoSeed = new System.Random((int)System.DateTime.Now.Ticks);
        SetMap();
    }

    void Update()
    {

    }

    //맵을 세팅하는 함수
    void SetMap()
    {
        startPosition = new Vector2(transform.position.x, transform.position.y); //시작위치 초기화
        ShuffleFloorXY(floors.floor1.floorWidth, floors.floor1.floorLength, autoSeed); //1층 세팅
        CreateTile(floors.floor1);
        ShuffleFloorXY(floors.floor2.floorWidth, floors.floor2.floorLength, autoSeed); //1층 세팅
        CreateTile(floors.floor2);
        ShuffleFloorXY(floors.floor3.floorWidth, floors.floor3.floorLength, autoSeed); //1층 세팅
        CreateTile(floors.floor3);
        ShuffleFloorXY(floors.floor4.floorWidth, floors.floor4.floorLength, autoSeed); //1층 세팅
        CreateTile(floors.floor4);
        ShuffleFloorXY(floors.floor5.floorWidth, floors.floor5.floorLength, autoSeed); //1층 세팅
        CreateTile(floors.floor5);

    }

    void CreateTile(FloorData floor)
    {
        int n = floorCoordinate.Count - 1; //좌표 리스트 내에 들어가있는 좌표의 총 수량

        //광물 종류만큼 반복
        for (int i = floor.Minerals.Count() - 1; i >= 0; i--)
        {
            int ran = UnityEngine.Random.Range(floor.Minerals[i].min, floor.Minerals[i].max);
            for (int j = ran; ran > 0; ran--)
            {
                Vector3Int vec = new Vector3Int((int)floorCoordinate[n].x, (int)floorCoordinate[n].y, 0);
                tilemap.SetTile(vec, floor.Minerals[i].mineralTileBase);
                n--;
            }
        }
        while (n > -1)
        {
            Vector3Int vec = new Vector3Int((int)floorCoordinate[n].x, (int)floorCoordinate[n].y, 0);
            tilemap.SetTile(vec, floor.soil);
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
        startPosition.y -= floorLength; //다음 사용시를 대비해 세로길이 더해두기

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
 *      0.1. 해당 층의 범위 내의 모든 타일 좌표(x, y)를 층별 리스트에 넣기. (미리 만들기 X, 섞고 빼고 하기 때문에 어차피 매번 새로 만들어야 함.)
 *   1. 광물의 최소/최대치를 가져온 후 랜덤값 뽑기 (이번 일차에 나올 해당 광물의 수)
 *   2. 뽑은 랜덤값만큼 for문을 돌려 광물 랜덤배치.
 *      2.1. 위에서 만든 리스트를 Pop하여 광물 배치.
 *       ㄴ> 큐가 아닌 스택으로 취급할 것. 선입선출인 큐의 경우 한번 사용할 때 마다 전부 한 칸씩 밀어야 하지만, 선입후출인 스택의 경우 같은 효과를 내면서도 효율적임.
 *      2.2. 해당 층의 모든 광물의 배치가 끝날 때 까지 1~2를 반복할 것.
 *   3. 층 광물 배치가 끝났다면, 리스트 안이 빌 때 까지 좌표를 꺼내와 (남은 것을 전부 꺼내와) 기본타일(흙)으로 배치할 것.
 *   4. 이를 모든 층이 배치될 때 까지 반복.
 *  
 *  [필요한 별도 함수(가독성 향상 & 재사용성 증가를 위해)]
 *   - 1~5층의 가로세로값을 가져와 층별 좌표를 리스트에 전부 넣은 후 섞어서 랜덤 순서로 만드는 함수. (기존 리스트가 있다면 아예 삭제하고 새로 만들 것.)
 *   ㄴ> 미리 크기를 알 수 있기 때문에 동적배열로 생성해 메모리 재할당을 막을 것.
 *  
 *  [참고사항]
 *   - 제너레이터 스크립트가 붙은 오브젝트 자체를 기준좌표로 삼고 X좌표는 +, Y좌표는 -로 해서 우측 하단으로 생성되도록 함.
 *   - list 내부의 좌표가 소진되면 즉시 생성 종료시켜야 함.
 *   - 유니티 SetTile 기능을 사용해서 최적화. 절대!!!! 개별 프리팹으로 만들지 말 것!
 *   - 테스트를 고려하면 시드값을 넣어 오류상황 재현이 필요할 수도 있긴 한데, 이건 후순위로 뺄 것.
*/
