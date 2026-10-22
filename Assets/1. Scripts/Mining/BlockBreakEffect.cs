using UnityEngine;
using UnityEngine.Tilemaps;

//채굴 진행도에 맞춰 블록 위에 파괴 애니메이션(균열 등)을 보여준다.
//MiningController의 이벤트와 Progress만 읽고, 채굴 로직은 건드리지 않는다.
public class BlockBreakEffect : MonoBehaviour
{
    [SerializeField] GameObject effectPrefab; //Bteak_Animation 프리팹
    [Tooltip("비워두면 Animator의 기본 상태를 자동으로 사용. 동작하지 않으면 Animator 창의 상태 이름을 적을 것")]
    [SerializeField] string stateName = "";

    MiningController mining;
    Tilemap frontTilemap;
    GameObject effect;
    Animator animator;
    int stateHash;

    void Start()
    {
        mining = FindAnyObjectByType<MiningController>();
        frontTilemap = FindAnyObjectByType<TileBreaker>().frontTilemap;

        effect = Instantiate(effectPrefab);
        effect.SetActive(false);
        animator = effect.GetComponentInChildren<Animator>(true);
        if (animator == null)
        {
            Debug.LogError("[BlockBreakEffect] 프리팹에서 Animator를 찾지 못했습니다.");
            enabled = false;
            return;
        }
        animator.speed = 0f; //재생 위치를 코드가 직접 정하므로 자동 재생은 끈다
        if (!string.IsNullOrEmpty(stateName)) stateHash = Animator.StringToHash(stateName);

        mining.OnMiningStarted += OnStarted;
        mining.OnMiningCancelled += Hide;
        mining.OnBlockBroken += OnBroken;
    }

    void OnDestroy()
    {
        if (mining == null) return;
        mining.OnMiningStarted -= OnStarted;
        mining.OnMiningCancelled -= Hide;
        mining.OnBlockBroken -= OnBroken;
    }

    void Update()
    {
        if (effect.activeSelf && mining.IsMining)
        {
            Apply(mining.Progress);
        }
    }

    void OnStarted(Vector3Int cell, float duration)
    {
        effect.transform.position = frontTilemap.GetCellCenterWorld(cell);
        effect.SetActive(true);

        if (stateHash == 0)
        {
            //기본 상태의 해시를 한 번 읽어온다. 활성화 직후 한 번 갱신해야 값이 들어온다.
            animator.Update(0f);
            stateHash = animator.GetCurrentAnimatorStateInfo(0).fullPathHash;
        }
        Apply(0f);
    }

    void OnBroken(Vector3Int cell) { Hide(); }

    void Hide() { effect.SetActive(false); }

    //progress(0~1)에 해당하는 프레임으로 즉시 이동. 1이면 반복 클립에서 처음으로 돌아갈 수 있어 살짝 줄인다.
    void Apply(float progress)
    {
        animator.Play(stateHash, 0, Mathf.Clamp(progress, 0f, 0.999f));
        animator.Update(0f);
    }
}