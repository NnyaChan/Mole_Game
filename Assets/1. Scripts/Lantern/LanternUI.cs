using UnityEngine;
using UnityEngine.UI;

//랜턴 칸 UI. 칸 하나 프리팹을 칸 수만큼 복제해서 만들고, 남은 칸 수만큼만 보이게 한다.
public class LanternUI : MonoBehaviour
{
    [SerializeField] LanternManager lantern;
    [SerializeField] Image segmentPrefab; //칸 하나 이미지 (프리팹)
    [SerializeField] Transform container; //Horizontal Layout Group이 붙은 부모

    Image[] cells;

    void Start()
    {
        cells = new Image[lantern.Segments];
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i] = Instantiate(segmentPrefab, container);
        }

        Refresh(lantern.SegmentsLeft);
        lantern.OnSegmentsChanged += Refresh; //칸 수가 바뀔 때만 갱신
    }

    void OnDestroy()
    {
        if (lantern != null) lantern.OnSegmentsChanged -= Refresh;
    }

    //남은 칸 수만큼 왼쪽부터 보이게 한다. (오른쪽 칸부터 사라짐)
    void Refresh(int left)
    {
        for (int i = 0; i < cells.Length; i++)
        {
            cells[i].enabled = i < left;
        }
    }
}