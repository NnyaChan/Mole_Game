using UnityEngine;
using UnityEngine.UI;
using TMPro;

//플레이어의 깊이를 Slider로 표시한다. ZoneDetector의 Depth만 읽는다.
public class DepthGauge : MonoBehaviour
{
    [SerializeField] ZoneDetector zone;
    [SerializeField] Slider slider;        //깊이를 표시할 Slider
    [SerializeField] TMP_Text label;       //선택: "3층 / 25칸" 같은 글자 (비워도 됨)
    [SerializeField] float moveSpeed = 2f; //게이지가 움직이는 속도 (초당 비율). 클수록 빠름

    Floor floor;
    int totalRows;
    float current;
    int lastDepth = int.MinValue;

    void Start()
    {
        if (zone == null) zone = FindAnyObjectByType<ZoneDetector>();
        floor = FindAnyObjectByType<TileBreaker>().floor;

        foreach (FloorData data in floor.floorData)
        {
            totalRows += data.floorLength; //전체 깊이 = 층별 세로 길이의 합
        }

        //표시 전용: 사용자가 드래그하지 못하게 하고, 비활성 색(회색)이 입혀지지 않게 한다.
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.wholeNumbers = false;
        slider.transition = Selectable.Transition.None;
        slider.interactable = false;
    }

    void Update()
    {
        //지상(-1)은 0, 맨 아래 줄은 1이 되도록 계산
        float target = totalRows > 0 ? Mathf.Clamp01((zone.Depth + 1f) / totalRows) : 0f;
        current = Mathf.MoveTowards(current, target, moveSpeed * Time.deltaTime);
        slider.value = current;

        //글자는 깊이가 바뀔 때만 갱신
        if (label != null && zone.Depth != lastDepth)
        {
            lastDepth = zone.Depth;
            label.text = zone.InMine
                ? $"{floor.GetFloorIndex(zone.Depth) + 1}층 / {zone.Depth + 1}칸"
                : "지상";
        }
    }
}