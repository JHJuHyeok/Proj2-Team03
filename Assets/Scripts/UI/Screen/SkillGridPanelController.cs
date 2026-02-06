using UnityEngine;

/*
[승문]
SkillGridPanelController
-스킬 배치용 그리드 패널 Show / Hide 전용
-슬라이드 애니메이션은 내부에서 처리하거나 외부와 연동
*/
public class SkillGridPanelController : MonoBehaviour
{
    [SerializeField] private RectTransform panelRoot;

    private bool isShown;

    public bool IsShown => isShown;

    private void Awake()
    {
        if (panelRoot == null)
        {
            panelRoot = transform as RectTransform;
        }
    }

    public void Show()
    {
        if (isShown) return;
        isShown = true;
        panelRoot.gameObject.SetActive(true);

        // 여기서 슬라이드 애니메이션 추가하면 호출
    }

    public void Hide()
    {
        if (!isShown) return;
        isShown = false;
        panelRoot.gameObject.SetActive(false);

        // 여기서 슬라이드 애니메이션 추가하면 호출
    }

    public void Toggle()
    {
        if (isShown) Hide();
        else Show();
    }
}
