using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

//핸드폰 정산 화면. 오늘 판매한 광물과 금액을 보여주고, 확인 버튼을 누를 때까지 기다린다.
//GameManager가 일차 종료 때 찾아서 쓴다. 씬에 없으면 정산은 화면 없이 자동으로 처리된다.
public class SettlementUI : MonoBehaviour
{
    [SerializeField] GameObject panel;        //정산 화면 전체 (평소엔 꺼 둠)
    [SerializeField] TMP_Text linesText;      //광물별 내역
    [SerializeField] TMP_Text totalText;      //합계 / 소지금
    [SerializeField] Button confirmButton;    //확인 버튼

    bool confirmed;

    //정산 내역을 보여주고 확인 버튼이 눌릴 때까지 기다린다. (GameManager의 코루틴에서 yield return으로 사용)
    public IEnumerator Show(SettlementReport report, int moneyBefore)
    {
        StringBuilder sb = new StringBuilder();
        if (report.lines.Count == 0)
        {
            sb.Append("오늘 판매한 광물이 없습니다.");
        }
        else
        {
            foreach (SettlementLine line in report.lines)
            {
                sb.AppendLine($"{line.type} x{line.count}  ×  {line.unitPrice}G  =  {line.Subtotal}G");
            }
        }
        if (linesText != null) linesText.text = sb.ToString();
        if (totalText != null) totalText.text = $"합계 {report.Total}G\n소지금 {moneyBefore}G → {moneyBefore + report.Total}G";

        confirmed = false;
        confirmButton.onClick.AddListener(OnConfirm);
        panel.SetActive(true);

        while (!confirmed) yield return null;

        confirmButton.onClick.RemoveListener(OnConfirm);
        panel.SetActive(false);
    }

    void OnConfirm() { confirmed = true; }
}
