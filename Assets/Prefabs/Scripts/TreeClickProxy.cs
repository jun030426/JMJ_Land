using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TreeClickProxy : MonoBehaviour
{
     TreeGrowController controller;

    void Awake() { controller = GetComponentInParent<TreeGrowController>(); }

    void OnMouseDown()
    {
        controller?.Chop();   // 클릭 넘어
    }
}

