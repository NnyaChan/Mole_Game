using System;
using UnityEngine;

//랜턴 = 하루의 시간. 광산(맵 영역) 안에 있을 때만 줄어들고, 0이 되면 일차가 종료된다.
public class LanternManager : MonoBehaviour
{
    [SerializeField] ZoneDetector zone;
    [SerializeField] float dayDuration = 100f; //하루 시간(초)
    [SerializeField] int segments = 10;        //랜턴 칸 수 (100초 / 10칸 = 10초에 한 칸)

    public float Remaining { get; private set; }
    public float SegmentDuration => dayDuration / segments;
    public int SegmentsLeft => Mathf.CeilToInt(Remaining / SegmentDuration);
    public int Segments => segments;

    public event Action<int> OnSegmentsChanged; //남은 칸 수가 바뀔 때 (UI용)
    public event Action OnEmpty;                //랜턴이 모두 떨어졌을 때

    int lastSegments;
    bool emptied;

    void Awake()
    {
        Remaining = dayDuration;
        lastSegments = segments;
        Debug.Log($"[랜턴] {segments}칸 / {dayDuration}초");
    }

    void Update()
    {
        if (emptied) return;
        if (GameManager.Instance != null && !GameManager.Instance.IsPlaying) return;
        if (!zone.InMine) return; //광산 밖에서는 닳지 않음

        Remaining = Mathf.Max(0f, Remaining - Time.deltaTime);

        int now = SegmentsLeft;
        if (now != lastSegments)
        {
            lastSegments = now;
            Debug.Log($"[랜턴] 남은 칸: {now}/{segments} (남은 시간 {Remaining:F0}초, 깊이 {zone.Depth})");
            OnSegmentsChanged?.Invoke(now);
        }

        if (Remaining <= 0f)
        {
            emptied = true;
            OnEmpty?.Invoke();
            if (GameManager.Instance != null) GameManager.Instance.EndDay(zone.Zone);
            else Debug.LogWarning("[랜턴] GameManager가 씬에 없어 일차 종료를 처리하지 못했습니다.");
        }
    }
}