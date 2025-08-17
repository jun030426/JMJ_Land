using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class TreeGrowController : MonoBehaviour
{
    [Header("Animator & Trigger")]
    public Animator animator;
    public string fallTrigger = "FallTrigger";
    public string idleStateName = "idle";   

    [Header("넘어지는 전용 오브젝트(FallVisual)")]
    [SerializeField] Transform fallingObject;   // ← FallVisual의 Transform
    public GameObject fallVisual;               // ← FallVisual 오브젝트

    [Header("단계 오브젝트 (한 번에 하나만 켬)")]
    public GameObject stage0_Stump;   
    public GameObject t1_Small;       
    public GameObject t2_Mid;         
    public GameObject t3_Full;        

    [Header("단계 전환 간격(초)")]
    public float stageInterval = 3f;
    public bool useRealtimeWait = false;

    [Header("넘어지는 동안 클릭 막기(선택)")]
    public Collider[] clickableColliders; // 비우면 t3_Full 자식에서 자동 수집

    enum Phase { Full, Falling, Regrow }
    Phase phase = Phase.Full;
    Coroutine regrowCo;
    bool isBusy;

    // 원래 포즈 저장
    Vector3 pivotPos0, fallPos0;
    Quaternion pivotRot0, fallRot0;

    // 각 스테이지 기본 포즈 
    Vector3 s0Pos0, s1Pos0, s2Pos0, s3Pos0;
    Quaternion s0Rot0, s1Rot0, s2Rot0, s3Rot0;

    void Reset() { animator = GetComponent<Animator>(); }

    void Awake()
    {
        pivotPos0 = transform.localPosition;
        pivotRot0 = transform.localRotation;

        // FallVisual 자동 지정
        if (!fallingObject && fallVisual) fallingObject = fallVisual.transform;
        if (fallingObject) { fallPos0 = fallingObject.localPosition; fallRot0 = fallingObject.localRotation; }

        // 스테이지 기본 포즈 저장
        if (stage0_Stump) { s0Pos0 = stage0_Stump.transform.localPosition; s0Rot0 = stage0_Stump.transform.localRotation; }
        if (t1_Small) { s1Pos0 = t1_Small.transform.localPosition; s1Rot0 = t1_Small.transform.localRotation; }
        if (t2_Mid) { s2Pos0 = t2_Mid.transform.localPosition; s2Rot0 = t2_Mid.transform.localRotation; }
        if (t3_Full) { s3Pos0 = t3_Full.transform.localPosition; s3Rot0 = t3_Full.transform.localRotation; }

        // 클릭 콜라이더 자동 수집
        if ((clickableColliders == null || clickableColliders.Length == 0) && t3_Full)
            clickableColliders = t3_Full.GetComponentsInChildren<Collider>(true);

        if (fallVisual) fallVisual.SetActive(false); // 시작엔 숨김
        SetOnly(t3_Full);                            // 평상시 3단계만 보이기
    }

    // 클릭 프록시가 호출
    public void Chop()
    {
        if (phase != Phase.Full || isBusy) return;
        if (!animator) animator = GetComponent<Animator>();

        var st = animator.GetCurrentAnimatorStateInfo(0);
        if (!st.IsName(idleStateName) || animator.IsInTransition(0)) return;

        isBusy = true;
        phase = Phase.Falling;

        SetClickable(false);

        // t3 끄고 FallVisual 켜서 넘어
        if (t3_Full) { t3_Full.SetActive(false); Debug.Log("t3 OFF"); }
        if (fallVisual) { fallVisual.SetActive(true); Debug.Log("FallVisual ON"); }

        animator.ResetTrigger(fallTrigger);
        animator.SetTrigger(fallTrigger);//넘어가
    }

    // TreeFall 마지막 프레임 이벤트
    public void OnFallEnd()
    {
        phase = Phase.Regrow;

        ResetUpright();                 // (피벗 + FallVisual)
        if (animator) {
            animator.Rebind(); 
            animator.Update(0f); }

        if (fallVisual) fallVisual.SetActive(false); // 보이지 않게

        if (regrowCo != null) return;                // 중복 시작 방지
        regrowCo = StartCoroutine(RegrowRoutine());
    }

    //성장유지
    IEnumerator RegrowRoutine()
    {
        ResetUpright(); 
        SetOnly(stage0_Stump); 
        yield return Wait(stageInterval);

        ResetUpright(); 
        if (t1_Small) { 
            SetOnly(t1_Small); 
            yield return Wait(stageInterval); }

        ResetUpright(); 
        if (t2_Mid) { 
            SetOnly(t2_Mid); 
            yield return Wait(stageInterval); }

        ResetUpright(); 
        SetOnly(t3_Full); // 최종 유지

        SetClickable(true);
        phase = Phase.Full;
        isBusy = false;
        regrowCo = null;
    }

    void ResetUpright()
    {
        // 피벗 복구
        transform.localPosition = pivotPos0;
        transform.localRotation = pivotRot0;

        // FallVisual 복구
        if (fallingObject)
        {
            fallingObject.localPosition = fallPos0;
            fallingObject.localRotation = fallRot0;
        }

        //  항상 똑바로
        if (stage0_Stump) { stage0_Stump.transform.localPosition = s0Pos0; stage0_Stump.transform.localRotation = s0Rot0; }
        if (t1_Small) { t1_Small.transform.localPosition = s1Pos0; t1_Small.transform.localRotation = s1Rot0; }
        if (t2_Mid) { t2_Mid.transform.localPosition = s2Pos0; t2_Mid.transform.localRotation = s2Rot0; }
        if (t3_Full) { t3_Full.transform.localPosition = s3Pos0; t3_Full.transform.localRotation = s3Rot0; }
    }

    IEnumerator Wait(float t)
    {
        if (useRealtimeWait) yield return new WaitForSecondsRealtime(t);
        else yield return new WaitForSeconds(t);
    }

    void SetOnly(GameObject target)
    {
        if (stage0_Stump) stage0_Stump.SetActive(target == stage0_Stump);
        if (t1_Small) t1_Small.SetActive(target == t1_Small);
        if (t2_Mid) t2_Mid.SetActive(target == t2_Mid);
        if (t3_Full) t3_Full.SetActive(target == t3_Full);
        if (fallVisual) fallVisual.SetActive(false);    // 성장 단계 동안엔 항상 OFF
    }

    void SetClickable(bool on)
    {
        if (clickableColliders == null) return;
        foreach (var c in clickableColliders) if (c) c.enabled = on;
    }
}
