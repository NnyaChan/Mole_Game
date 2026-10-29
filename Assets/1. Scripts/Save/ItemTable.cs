using System.Collections.Generic;

//아이템 이름(ID)과 최대 보유량 표 (06_Items.md). 이름(ID)은 한 번 정하면 바꾸지 않는다.
//곡괭이/클로 같은 영구 장비는 PlayerData의 단계 값으로 저장하므로 여기에 없다. (배송 주문에서만 ID로 쓴다)
public static class ItemTable
{
    public const string Dynamite1 = "dynamite_1";
    public const string Dynamite2 = "dynamite_2";
    public const string Dynamite3 = "dynamite_3";
    public const string Battery = "battery";
    public const string EnergyDrink = "energy_drink";
    public const string Coffee = "coffee";

    //소모 아이템 ID → 최대 보유량
    static readonly Dictionary<string, int> maxCounts = new Dictionary<string, int>
    {
        { Dynamite1, 3 },
        { Dynamite2, 3 },
        { Dynamite3, 3 },
        { Battery, 1 },
        { EnergyDrink, 3 },
        { Coffee, 3 },
    };

    //등록되지 않은 ID면 false
    public static bool TryGetMax(string id, out int max)
    {
        return maxCounts.TryGetValue(id ?? string.Empty, out max);
    }
}
