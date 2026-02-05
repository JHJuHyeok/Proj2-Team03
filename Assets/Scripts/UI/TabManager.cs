using UnityEngine;
using UnityEngine.UI;

/*[승문]
TabManager
- 탭 클릭으로 패널 전환
- 버튼 Sprite 전환 + 색상 Dim 처리
- 고정 패널(배경/프레임)도 탭에 맞춰 Sprite 또는 GameObject로 연동
- 배열 길이 불일치/누락 참조 방어
*/
public class TabManager : MonoBehaviour
{
    [Header("Tab Panels (main content)")]
    [SerializeField] private GameObject[] tab;

    [Header("Tab Button Images")]
    [SerializeField] private Image[] tabBtnImage;
    [SerializeField] private Sprite[] idleSprite;
    [SerializeField] private Sprite[] selectSprite;

    [Header("Tab Button Colors")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color dimColor = new Color(1f, 1f, 1f, 0.45f);

    [Header("Linked Fixed Panel (Optional)")]
    [Tooltip("탭에 따라 고정 패널(배경/프레임 등) Sprite를 바꾸고 싶으면 지정")]
    [SerializeField] private Image fixedPanelImage;

    [Tooltip("탭 인덱스별 고정 패널 Sprite (길이는 탭 개수와 같을수록 좋음)")]
    [SerializeField] private Sprite[] fixedPanelSpriteByTab;

    [Tooltip("탭 인덱스별로 고정 패널 GameObject를 켜고 끄고 싶으면 지정 (ex: 탭별 데코)")]
    [SerializeField] private GameObject[] fixedPanelObjectsByTab;

    private int Count => tab != null ? tab.Length : 0;

    private void Start()
    {
        if (Count <= 0) return;
        TabClick(0);
    }

    public void TabClick(int n)
    {
        Debug.Log($"[TabManager] TabClick: {n}", this);
        if (Count <= 0) return;

        if (n < 0) n = 0;
        if (n > Count - 1) n = Count - 1;

        // 1) 탭 콘텐츠 패널 전환
        for (int i = 0; i < Count; i++)
        {
            if (tab[i] != null)
                tab[i].SetActive(i == n);
        }

        // 2) 버튼 Sprite + 색상 Dim 처리
        if (tabBtnImage != null)
        {
            for (int i = 0; i < tabBtnImage.Length; i++)
            {
                var img = tabBtnImage[i];
                if (img == null) continue;

                Sprite idle = (idleSprite != null && i < idleSprite.Length) ? idleSprite[i] : null;
                Sprite select = (selectSprite != null && i < selectSprite.Length) ? selectSprite[i] : null;

                bool isSelected = (i == n);

                // 스프라이트 전환 (없으면 기존 유지)
                if (isSelected && select != null) img.sprite = select;
                else if (!isSelected && idle != null) img.sprite = idle;

                // 색상 처리
                img.color = isSelected ? selectedColor : dimColor;
            }
        }

        // 3) 고정 패널 Sprite 연동(옵션)
        if (fixedPanelImage != null && fixedPanelSpriteByTab != null && n < fixedPanelSpriteByTab.Length)
        {
            if (fixedPanelSpriteByTab[n] != null)
                fixedPanelImage.sprite = fixedPanelSpriteByTab[n];
        }

        // 4) 고정 패널 GameObject 연동(옵션)
        if (fixedPanelObjectsByTab != null && fixedPanelObjectsByTab.Length > 0)
        {
            for (int i = 0; i < fixedPanelObjectsByTab.Length; i++)
            {
                if (fixedPanelObjectsByTab[i] != null)
                    fixedPanelObjectsByTab[i].SetActive(i == n);
            }
        }
    }
}