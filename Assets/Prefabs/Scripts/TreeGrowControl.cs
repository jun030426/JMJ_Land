using UnityEngine;
using System;
using System.Collections;

// 이 스크립트는 나무의 성장을 제어하고, 베였을 때의 동작을 처리합니다.
public class TreeGrowthController : MonoBehaviour
{
    // growthStages: 나무의 성장 단계별 게임 오브젝트 배열.
    // 인덱스 0: 빈 상태 (베인 후)
    // 인덱스 1: 8시간 후 성장
    // 인덱스 2: 16시간 후 성장
    // 인덱스 3: 24시간 후 완전 성장
    public GameObject[] growthStages;

    // animator: 나무가 베일 때 쓰러지는 애니메이션을 제어하는 Animator 컴포넌트입니다.
    public Animator animator;

    // PlayerPrefs에 저장할 고유 키를 생성하기 위한 변수입니다.
    private string saveKey;

    // 나무가 베인 상태인지 확인하는 플래그입니다.
    private bool isChopped = false;

    // Awake() 메서드는 Start() 전에 호출되며, PlayerPrefs 키를 초기화합니다.
    void Awake()
    {
        // 각 나무 오브젝트가 고유한 키를 갖도록 InstanceID를 사용합니다.
        saveKey = "TreeChoppedTime_" + gameObject.GetInstanceID();
    }

    // Start() 메서드는 스크립트가 활성화될 때 한 번 호출됩니다.
    // 여기서는 PlayerPrefs에 저장된 데이터를 불러와 나무의 성장 상태를 업데이트합니다.
    void Start()
    {
        // Debug.Log("[Tree] Start called.");
        if (PlayerPrefs.HasKey(saveKey))
        {
            string savedTime = PlayerPrefs.GetString(saveKey);
            DateTime choppedTime = DateTime.Parse(savedTime);
            double hoursPassed = (DateTime.Now - choppedTime).TotalHours;
            UpdateGrowthStage(hoursPassed);
        }
        else
        {
            SetStage(3); // PlayerPrefs에 데이터가 없으면 완전 성장 상태로 시작합니다.
        }
    }

    // ChopTree() 메서드는 플레이어가 도끼로 나무를 쳤을 때 호출됩니다.
    public void ChopTree()
    {
        // 이미 베인 나무라면 더 이상 동작하지 않습니다.
        if (isChopped) return;
        isChopped = true;

        // "FallTrigger" 애니메이션을 재생하여 나무를 쓰러뜨립니다.
        if (animator != null)
        {
            animator.SetTrigger("FallTrigger");
        }
        else
        {
            Debug.LogError("Animator is not assigned to the TreeGrowthController.");
            // 애니메이터가 없으면 바로 베인 상태로 전환합니다.
            SetStage(0);
            // 3초 후 사라지는 코루틴을 시작합니다.
            StartCoroutine(RemoveTreeAfterDelay(3f));
        }

        // 현재 시간을 PlayerPrefs에 저장하여 리스폰 타이머를 시작합니다.
        PlayerPrefs.SetString(saveKey, DateTime.Now.ToString());
        PlayerPrefs.Save();
    }

    // 애니메이션 이벤트에서 호출될 수 있는 함수입니다.
    // 이 함수는 나무가 넘어지는 애니메이션이 끝난 후 호출되어야 합니다.
    public void OnFallEnd()
    {
        Debug.Log("[Tree] OnFallEnd event triggered.");
        StartCoroutine(RemoveTreeAfterDelay(3f));
    }

    // 나무가 사라지기까지 딜레이를 주는 코루틴입니다.
    IEnumerator RemoveTreeAfterDelay(float delay)
    {
        // 지정된 시간(delay)만큼 기다립니다.
        yield return new WaitForSeconds(delay);
        // 나무를 빈 상태(Stage 0)로 설정하여 사라지게 만듭니다.
        SetStage(0);
    }

    // UpdateGrowthStage() 메서드는 경과된 시간에 따라 나무의 성장 단계를 업데이트합니다.
    private void UpdateGrowthStage(double hoursPassed)
    {
        if (hoursPassed >= 24)
        {
            SetStage(3); // 24시간 이상: 완전 성장
            PlayerPrefs.DeleteKey(saveKey); // 완전 성장했으니 저장된 시간을 삭제합니다.
            isChopped = false;
        }
        else if (hoursPassed >= 16)
        {
            SetStage(2); // 16시간 이상: 3단계 성장
        }
        else if (hoursPassed >= 8)
        {
            SetStage(1); // 8시간 이상: 2단계 성장
        }
        else
        {
            SetStage(0); // 8시간 미만: 1단계 성장 (빈 상태)
        }
    }

    // SetStage() 메서드는 특정 성장 단계의 게임 오브젝트만 활성화하고 나머지는 비활성화합니다.
    private void SetStage(int stageIndex)
    {
        // stageIndex가 배열의 유효 범위를 벗어나지 않도록 검사합니다.
        if (stageIndex < 0 || stageIndex >= growthStages.Length)
        {
            Debug.LogError($"Invalid stageIndex: {stageIndex}");
            return;
        }

        for (int i = 0; i < growthStages.Length; i++)
        {
            growthStages[i].SetActive(i == stageIndex);
        }
    }
}
