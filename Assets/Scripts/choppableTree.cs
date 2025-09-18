using UnityEngine;
using System.Collections;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // 새 입력 시스템 지원
#endif

[RequireComponent(typeof(BoxCollider))]
public class choppableTree : MonoBehaviour
{
    public Animator animator;
    public string fallTrigger = "FallTreeTrigger";

    [SerializeField] int treeMaxHealth = 10;
    private int treeHealth;

    public float vanishDelay = 3f;     // 쓰러지고 나서 숨기기까지
    public float respawnDelay = 6f;    // 다시 나타나기까지(예: 600=10분)

    // ▼ 추가: 플레이어 근접 체크
    [Header("Interaction")]
    public Transform player;           // 비워두면 playerTag로 자동 찾음
    public string playerTag = "Player";
    public float interactRange = 0.1f;   // 이 거리 안에 있어야 스페이스가 먹음

    private bool isAnimating = false;

    private Collider col;
    private Renderer[] rends;

    private Vector3 initLocalPos;
    private Quaternion initLocalRot;
    private Vector3 initLocalScale;

    void Awake()
    {
        col = GetComponent<Collider>();
        rends = GetComponentsInChildren<Renderer>(true);

        initLocalPos = transform.localPosition;
        initLocalRot = transform.localRotation;
        initLocalScale = transform.localScale;

        // Animator를 비워두면 자식에서 자동 탐색
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    public void Start()
    {
        treeHealth = treeMaxHealth;
        isAnimating = false;

        // 시작부터 애니메이터 정지(클릭/스페이스 전엔 재생 금지)
        if (animator)
        {
            animator.ResetTrigger(fallTrigger);
            animator.Rebind();          // 기본 상태로 리셋
            animator.Update(0f);
            animator.enabled = false;
        }

        // 플레이어 자동 할당
        if (player == null && !string.IsNullOrEmpty(playerTag))
        {
            var go = GameObject.FindWithTag(playerTag);
            if (go) player = go.transform;
        }

        if (col) col.enabled = true;
        if (rends != null) foreach (var r in rends) r.enabled = true;
    }

    // ▼ 스페이스바 입력 + 근접 체크
    void Update()
    {
        if (isAnimating) return;
        if (!IsInteractableNow()) return;

        bool pressed = false;
#if ENABLE_INPUT_SYSTEM && !ENABLE_LEGACY_INPUT_MANAGER
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            pressed = true;
#else
        if (Input.GetKeyDown(KeyCode.Space))
            pressed = true;
#endif
        if (!pressed) return;

        treeHealth = Mathf.Max(0, treeHealth - 1);
        if (treeHealth <= 0) Fell();
    }

    // 마우스 클릭은 더 이상 사용하지 않음(원하면 이 함수 삭제해도 됨)
    void OnMouseDown() { /* intentionally empty */ }

    bool IsInteractableNow()
    {
        // 숨김 상태면(콜라이더 OFF) 상호작용 금지
        if (col != null && !col.enabled) return false;

        // 플레이어가 지정되어 있으면 거리 체크
        if (player != null)
        {
            float d = Vector3.Distance(player.position, transform.position);
            if (d > interactRange) return false;
        }
        return true;
    }

    void Fell()
    {
        isAnimating = true;

        if (animator && animator.runtimeAnimatorController && !string.IsNullOrEmpty(fallTrigger))
        {
            animator.enabled = true;          // 재생 허용
            animator.ResetTrigger(fallTrigger);
            animator.Rebind();                // 항상 예측 가능한 시작점
            animator.Update(0f);
            animator.SetTrigger(fallTrigger); // Idle→Fall 전이(Trigger) 필요
        }

        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        // (애니 이름 기다림 대신) 고정 시간 대기 후 숨김 처리도 가능
        // 여기서는 기존 구조 유지
        if (animator)
        {
            // Fall 상태로 들어갈 때까지 대기
            var info = animator.GetCurrentAnimatorStateInfo(0);
            while (!info.IsName("Fall"))
            {
                yield return null; info = animator.GetCurrentAnimatorStateInfo(0);
            }
            // Fall이 끝날 때(정규화 시간 1.0)까지 대기
            while (info.IsName("Fall") && info.normalizedTime < 1f)
            {
                yield return null; info = animator.GetCurrentAnimatorStateInfo(0);
            }
        }
        else
        {
            yield return new WaitForSeconds(vanishDelay);
        }

        // 숨김(보이기/클릭 차단)
        if (col) col.enabled = false;
        if (rends != null) foreach (var r in rends) r.enabled = false;

        // 리스폰 대기
        yield return new WaitForSeconds(respawnDelay);

        // 원래 상태로 복구 + 애니메이터 OFF
        transform.localPosition = initLocalPos;
        transform.localRotation = initLocalRot;
        transform.localScale = initLocalScale;

        if (animator)
        {
            animator.ResetTrigger(fallTrigger);
            animator.Rebind();
            animator.Update(0f);
            animator.enabled = false;
        }

        treeHealth = treeMaxHealth;
        isAnimating = false;
        if (rends != null) foreach (var r in rends) r.enabled = true;
        if (col) col.enabled = true;
    }
}
