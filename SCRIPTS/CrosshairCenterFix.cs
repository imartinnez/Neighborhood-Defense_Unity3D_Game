using UnityEngine;

public class CrosshairCenterFix : MonoBehaviour
{
    RectTransform rt;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void LateUpdate()
    {
        if (rt == null) return;

        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
    }
}
