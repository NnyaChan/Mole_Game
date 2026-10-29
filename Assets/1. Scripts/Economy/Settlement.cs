using System;
using System.Collections.Generic;
using UnityEngine;

//정산 내역 한 줄 (광물 한 종류)
public class SettlementLine
{
    public BlockType type;
    public int count;
    public int unitPrice;
    public int Subtotal => count * unitPrice;
}

public class SettlementReport
{
    public List<SettlementLine> lines = new List<SettlementLine>();
    public int Total
    {
        get
        {
            int sum = 0;
            foreach (SettlementLine line in lines) sum += line.Subtotal;
            return sum;
        }
    }
}

//오늘 획득한 광물을 금액으로 정산한다. 계산(Calculate)과 반영(Apply)을 분리해 화면에 먼저 보여줄 수 있다.
public static class Settlement
{
    public static SettlementReport Calculate(PlayerData data)
    {
        SettlementReport report = new SettlementReport();
        foreach (ItemStack stack in data.todayMinerals)
        {
            if (!Enum.TryParse(stack.id, out BlockType type) || type == BlockType.Soil) continue;
            report.lines.Add(new SettlementLine { type = type, count = stack.count, unitPrice = MineralPrices.GetPrice(type) });
        }
        report.lines.Sort((a, b) => a.type.CompareTo(b.type)); //BlockType 순서(흔한 광물 → 희귀 광물)
        return report;
    }

    //소지금에 합계를 더하고 오늘의 광물 목록을 비운다.
    public static void Apply(PlayerData data, SettlementReport report)
    {
        int before = data.money;
        data.money += report.Total;
        data.ClearMinerals();

        foreach (SettlementLine line in report.lines)
        {
            Debug.Log($"[정산] {line.type} x{line.count} × {line.unitPrice}G = {line.Subtotal}G");
        }
        Debug.Log($"[정산] 합계 {report.Total}G / 소지금 {before}G → {data.money}G");
    }
}
