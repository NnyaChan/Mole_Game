using System;
using System.Collections.Generic;
using UnityEngine;

//이름(ID)과 개수 한 줄. JsonUtility는 Dictionary를 저장하지 못해서 목록으로 둔다.
[Serializable]
public class ItemStack
{
    public string id;
    public int count;
}

//세이브 대상 플레이어 데이터. 필드를 추가하는 것은 안전하지만(기존 세이브는 기본값으로 채워짐),
//이름/타입을 바꾸면 기존 세이브를 읽지 못하므로 version을 올리고 변환을 고려할 것. (→ Docs/09_Save_Data_Test.md)
[Serializable]
public class PlayerData
{
    public const int CurrentVersion = 1;
    public const int MaxClawLevel = 3;

    public int version = CurrentVersion;
    public int day = 1;           //다음에 시작할 일차 (취침 시 저장하므로 저장 시점엔 이미 +1된 값)
    public int money = 0;         //소지금(G). 마이너스 가능, 한도 없음
    public int pickaxeLevel = 1;  //1~6, 기본 곡괭이(Lv1)는 증정
    public int clawLevel = 0;     //0 = 없음, 1~3 = 클로 +1~+3

    public List<ItemStack> items = new List<ItemStack>();         //소모 아이템 보유량 (0개는 저장 안 함)
    public List<ItemStack> pendingOrders = new List<ItemStack>(); //배송 대기 주문 (다음 날 아침 도착)

    //획득한 광물 (id = BlockType 이름, 예: "Gold"). 흙은 집계하지 않는다.
    //까마귀가 가져간 뒤 취침 시 정산되면 비워진다. (정산 기능 전에는 비워지지 않고 쌓인다)
    public List<ItemStack> todayMinerals = new List<ItemStack>();

    public void AddMineral(BlockType type, int amount = 1)
    {
        if (type == BlockType.Soil || amount <= 0) return;

        string id = type.ToString();
        ItemStack stack = todayMinerals.Find(s => s.id == id);
        if (stack == null) todayMinerals.Add(new ItemStack { id = id, count = amount });
        else stack.count += amount;
    }

    public int GetMineralCount(BlockType type)
    {
        ItemStack stack = todayMinerals.Find(s => s.id == type.ToString());
        return stack != null ? stack.count : 0;
    }

    public void ClearMinerals()
    {
        todayMinerals.Clear();
    }

    public int GetItemCount(string id)
    {
        ItemStack stack = items.Find(s => s.id == id);
        return stack != null ? stack.count : 0;
    }

    //최대 보유량을 넘지 않게 개수를 설정한다. 등록되지 않은 ID는 거부한다. 실제로 설정된 개수를 반환.
    public int SetItemCount(string id, int count)
    {
        if (!ItemTable.TryGetMax(id, out int max))
        {
            Debug.LogWarning($"[세이브] 등록되지 않은 아이템 ID: {id}");
            return GetItemCount(id);
        }

        count = Mathf.Clamp(count, 0, max);
        items.RemoveAll(s => s.id == id);
        if (count > 0) items.Add(new ItemStack { id = id, count = count });
        return count;
    }

    //클로는 +1 → +2 → +3 순서로만 구입 가능하다.
    public bool CanBuyClaw(int level)
    {
        return level == clawLevel + 1 && level <= MaxClawLevel;
    }

    //불러온 값이 범위를 벗어났거나 비어 있으면 보정한다.
    public void Sanitize()
    {
        day = Mathf.Max(1, day);
        pickaxeLevel = Mathf.Clamp(pickaxeLevel, 1, BlockCatalog.ToolLevelCount);
        clawLevel = Mathf.Clamp(clawLevel, 0, MaxClawLevel);

        if (items == null) items = new List<ItemStack>();
        items.RemoveAll(s => s == null || !ItemTable.TryGetMax(s.id, out _) || s.count <= 0);
        foreach (ItemStack s in items)
        {
            ItemTable.TryGetMax(s.id, out int max);
            s.count = Mathf.Min(s.count, max);
        }

        if (todayMinerals == null) todayMinerals = new List<ItemStack>();
        todayMinerals.RemoveAll(s => s == null || s.count <= 0
            || !Enum.TryParse(s.id, out BlockType t) || t == BlockType.Soil); //알 수 없는 이름/흙은 버림

        if (pendingOrders == null) pendingOrders = new List<ItemStack>();
        pendingOrders.RemoveAll(s => s == null || string.IsNullOrEmpty(s.id) || s.count <= 0);
    }
}
