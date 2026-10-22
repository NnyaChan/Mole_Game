using System.Collections;
using UnityEngine;

//화면 암전 연출. 이 스크립트는 '항상 켜져 있는' 오브젝트(예: Canvas)에 붙이고,
//검은 Image의 CanvasGroup(비활성 상태여도 됨)을 fadeGroup에 연결한다.
public class BlackoutFader : MonoBehaviour
{
    [SerializeField] CanvasGroup fadeGroup;   //암전용 검은 Image의 CanvasGroup
    [SerializeField] float fadeInTime = 0.5f; //새 일차 시작 시 밝아지는 시간

    void Awake()
    {
        fadeGroup.alpha = 0f;
        fadeGroup.gameObject.SetActive(false);
    }

    void Start()
    {
        //2일차부터는 검은 화면에서 시작해 서서히 밝아진다.
        if (GameManager.Instance != null && GameManager.Instance.Day > 1)
        {
            StartCoroutine(FadeIn());
        }
    }

    public IEnumerator FadeOut(float duration)
    {
        fadeGroup.gameObject.SetActive(true);
        fadeGroup.blocksRaycasts = true; //암전 중 마우스 클릭이 아래로 새지 않게
        yield return Fade(0f, 1f, duration);
    }

    IEnumerator FadeIn()
    {
        fadeGroup.gameObject.SetActive(true);
        fadeGroup.alpha = 1f;
        yield return Fade(1f, 0f, fadeInTime);
        fadeGroup.gameObject.SetActive(false); //다 밝아지면 다시 끔
    }

    //timeScale과 무관하게 동작하도록 unscaledDeltaTime 사용
    IEnumerator Fade(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            fadeGroup.alpha = Mathf.Lerp(from, to, t / duration);
            yield return null;
        }
        fadeGroup.alpha = to;
    }
}