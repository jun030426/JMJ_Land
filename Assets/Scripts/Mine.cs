using UnityEngine;
using System.Collections;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[RequireComponent(typeof(BoxCollider))]
public class Mine: MonoBehaviour
{
    // ▼ 돌 개수 누적 (모든 돌 오브젝트가 공유)
    [Tooltip("모든 돌 오브젝트가 공유하는 누적 개수입니다. 이 값은 인스펙터에서 보이지 않습니다.")]
    public static int rockCount = 0;

    [Header("Inventory")]
    [Tooltip("인스펙터에서 실시간으로 볼 수 있는 총 돌 개수")]
    public int totalRockCount = 0;


    // ▼ 플레이어 근접 체크
    [Header("Interaction")]
    public Transform player;
    public string playerTag = "Player";
    public float interactRange = 1.0f; // 돌 상호작용 반경 (1미터)

    private bool isInteracting = false;

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
    }

    public void Start()
    {
        isInteracting = false;

        if (col) col.enabled = true;
        if (rends != null) foreach (var r in rends) r.enabled = true;
    }

    void Update()
    {
        // static 변수 rockCount의 값을 public 변수에 할당
        totalRockCount = rockCount;
    }
    // 마우스 클릭 시 호출 (돌에 콜라이더가 있어야 작동)
    void OnMouseDown()
    {
        // 플레이어가 일정 거리 안에 있을 때만 상호작용
        if (!IsInteractableNow()) return;
        if (isInteracting) return;

        Mines();
    }

    bool IsInteractableNow()
    {
        if (col != null && !col.enabled) return false;

        if (player != null)
        {
            float d = Vector3.Distance(player.position, transform.position);
            if (d > interactRange) return false;
        }
        return true;
    }

    void Mines()
    {
        isInteracting = true;

        // 돌 개수 누적
        rockCount++;
        StartCoroutine(RespawnCoroutine());
    }

    IEnumerator RespawnCoroutine()
    {
        // 숨김
        if (col) col.enabled = false;
        if (rends != null) foreach (var r in rends) r.enabled = false;

        // 30초 대기
        yield return new WaitForSeconds(30f);

        // 원래 상태로 복구
        transform.localPosition = initLocalPos;
        transform.localRotation = initLocalRot;
        transform.localScale = initLocalScale;

        isInteracting = false;
        if (rends != null) foreach (var r in rends) r.enabled = true;
        if (col) col.enabled = true;
    }
}