using System.Collections.Generic;

//광물 기본 가격표 (05_Economy.md). 흙은 판매가 없다(0G).
//뉴스 시세 변동(로드맵 6번)이 생기면 GetPrice에서 그날의 배율을 적용한다.
public static class MineralPrices
{
    static readonly Dictionary<BlockType, int> basePrices = new Dictionary<BlockType, int>
    {
        { BlockType.Stone, 1 },
        { BlockType.Copper, 2 },
        { BlockType.Lead, 4 },
        { BlockType.Iron, 7 },
        { BlockType.Gold, 10 },
        { BlockType.Obsidian, 15 },
        { BlockType.Sapphire, 20 },
        { BlockType.Emerald, 25 },
        { BlockType.Ruby, 50 },
        { BlockType.Diamond, 100 },
    };

    public static int GetPrice(BlockType type)
    {
        return basePrices.TryGetValue(type, out int price) ? price : 0;
    }
}
