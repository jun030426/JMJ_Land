using UnityEngine;

public class TreeAnimationHooks : MonoBehaviour
{
    public TreeGrowController controller;  

    public void OnFallEnd()                 // 애니메이션 이벤트가 호출
    {
        Debug.Log("TreeFall3 끝!");
        controller?.OnFallEnd();
    }
}
