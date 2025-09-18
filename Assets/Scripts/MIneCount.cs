using UnityEngine;

public class MIneCount : MonoBehaviour
{
    [Header("총 돌 개수")]
    [Tooltip("모든 돌에서 채굴된 총 누적 개수입니다.")]
    public int totalRockCount = 0;

    void Update()
    {
        // MineableRock 스크립트의 정적 변수 rockCount 값을 가져옴
        totalRockCount = Mine.rockCount;
    }
}
