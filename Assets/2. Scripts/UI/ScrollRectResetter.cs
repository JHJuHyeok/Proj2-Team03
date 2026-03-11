using UnityEngine;
using UnityEngine.UI;

/*
[승문]
ScrollRectResetter
-탭/패널이 켜질 때 스크롤을 항상 맨 위로 맞춤
*/
public class ScrollRectResetter : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;

    private void Awake()
    {
        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>(true);
    }

    private void OnEnable()
    {
        if (scrollRect == null) return;

        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}