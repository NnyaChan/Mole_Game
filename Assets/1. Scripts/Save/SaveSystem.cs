using System;
using System.IO;
using UnityEngine;

//PlayerData를 JSON 파일 한 개(슬롯 1개)로 저장/불러오기한다.
//임시 파일에 먼저 쓴 뒤 교체하므로, 쓰는 도중에 꺼져도 기존 세이브가 깨지지 않는다.
public static class SaveSystem
{
    const string FileName = "save.json";

    static string FilePath => Path.Combine(Application.persistentDataPath, FileName);

    public static bool Exists => File.Exists(FilePath);

    public static bool Save(PlayerData data)
    {
        string tempPath = FilePath + ".tmp";
        try
        {
            File.WriteAllText(tempPath, JsonUtility.ToJson(data, true));
            if (File.Exists(FilePath)) File.Replace(tempPath, FilePath, null);
            else File.Move(tempPath, FilePath);

            Debug.Log($"[세이브] 저장 완료: {data.day}일차 / {data.money}G / 곡괭이 Lv{data.pickaxeLevel} / 클로 +{data.clawLevel}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[세이브] 저장 실패: {e.Message}");
            return false;
        }
    }

    //세이브가 없거나 읽을 수 없으면 null (호출한 쪽에서 새 게임으로 시작)
    public static PlayerData Load()
    {
        if (!Exists) return null;

        try
        {
            PlayerData data = JsonUtility.FromJson<PlayerData>(File.ReadAllText(FilePath));
            if (data == null)
            {
                Debug.LogWarning("[세이브] 세이브 파일이 비어 있어 새 게임으로 시작합니다.");
                return null;
            }
            if (data.version > PlayerData.CurrentVersion)
            {
                Debug.LogWarning($"[세이브] 더 새로운 버전({data.version})의 세이브입니다. 일부 값이 무시될 수 있습니다.");
            }

            data.Sanitize();
            Debug.Log($"[세이브] 불러오기 완료: {data.day}일차 / {data.money}G / 곡괭이 Lv{data.pickaxeLevel} / 클로 +{data.clawLevel}");
            return data;
        }
        catch (Exception e)
        {
            Debug.LogError($"[세이브] 불러오기 실패, 새 게임으로 시작합니다: {e.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        try
        {
            if (File.Exists(FilePath)) File.Delete(FilePath);
            Debug.Log("[세이브] 세이브 파일 삭제");
        }
        catch (Exception e)
        {
            Debug.LogError($"[세이브] 삭제 실패: {e.Message}");
        }
    }
}
