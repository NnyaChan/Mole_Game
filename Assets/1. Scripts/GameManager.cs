using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameState
{
    Playing, //채굴 진행 중
    Ending   //일차 종료 연출 중 (입력/랜턴 정지)
}

[DefaultExecutionOrder(-100)] //다른 스크립트가 PlayerData를 읽기 전에 세이브를 먼저 불러온다
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; } //싱글톤 사용으로 접근성 높임

    public GameState State { get; private set; } = GameState.Playing;
    public bool IsPlaying => State == GameState.Playing;
    public PlayerData Data { get; private set; } //세이브 대상 플레이어 데이터. 씬을 다시 불러와도 유지된다
    public int Day => Data.day;

    [SerializeField] float blackoutSeconds = 1f; //암전 시간
    [SerializeField] bool debugSaveKeys = true;  //임시 테스트 키: F5 저장 / F9 불러오기 / F12 초기화 (나중에 설정키로 대체)

    private void Awake()
    {
        // 인스턴스가 비어있다면 할당해주고,
        // 해당 오브젝트를 씬 이동간 파괴하지 않게합니다.
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            Data = SaveSystem.Load() ?? new PlayerData(); //세이브가 없으면 새 게임 (1일차, 0G, 곡괭이 Lv1)
        }
        // 인스턴스가 이미 할당돼있다면(2개 이상이라면) 파괴합니다.
        // (씬을 다시 불러오면 씬 안에 있는 GameManager가 여기로 들어와 파괴됩니다. 정상입니다.)
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        if (Instance == this) Debug.Log($"[{Day}일차 시작]");
    }

    //일차 종료. 랜턴이 모두 떨어졌을 때 호출된다.
    public void EndDay(Zone zoneAtEnd)
    {
        if (State != GameState.Playing) return; //중복 호출 방지
        State = GameState.Ending;
        StartCoroutine(EndDayRoutine(zoneAtEnd));
    }

    IEnumerator EndDayRoutine(Zone zoneAtEnd)
    {
        string result = (zoneAtEnd == Zone.Surface) ? "지상(귀환 성공)" : "지하(조난)";
        Debug.Log($"[{Day}일차 종료] 랜턴 소진 시점 상태: {result}");

        //암전 연출. 씬에 BlackoutFader가 없으면 시간만 기다린다.
        BlackoutFader fader = FindAnyObjectByType<BlackoutFader>();
        if (fader != null) yield return fader.FadeOut(blackoutSeconds);
        else yield return new WaitForSecondsRealtime(blackoutSeconds);

        Data.day++;
        SaveSystem.Save(Data); //취침 시 자동저장 (현재는 일차 종료 시점에 저장)
        ReloadScene();
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); //맵/플레이어/랜턴을 새로 시작
    }

    //임시 테스트 키. 일차 종료 연출 중에는 동작하지 않는다.
    void Update()
    {
        if (!debugSaveKeys || Instance != this || !IsPlaying || Keyboard.current == null) return;

        if (Keyboard.current.f5Key.wasPressedThisFrame)
        {
            SaveSystem.Save(Data);
        }
        else if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            Data = SaveSystem.Load() ?? new PlayerData();
            ReloadScene();
        }
        else if (Keyboard.current.f12Key.wasPressedThisFrame)
        {
            SaveSystem.Delete();
            Data = new PlayerData();
            ReloadScene();
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        State = GameState.Playing;
        if (Day > 1) Debug.Log($"[{Day}일차 시작]");
    }
}