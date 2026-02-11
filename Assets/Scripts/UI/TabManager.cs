using UnityEngine;
using UnityEngine.UI;

/*[승문]
TabManager
- 탭 클릭으로 패널 전환
- 버튼 Sprite 전환 + 색상 Dim 처리
- (옵션) 고정 패널 Sprite / GameObject 연동
- TabManager 여러 개 있을 때: 활성화될 때만 초기 탭 선택(덮어쓰기 방지)
*/
public class TabManager : MonoBehaviour
{
    [Header("Tab Panels")]
    [SerializeField] private GameObject[] tab;

    [Header("Tab Button Images")]
    [SerializeField] private Image[] tabBtnImage;

    [Header("Tab Button Colors")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color dimColor = new Color(1f, 1f, 1f, 0.45f);

    private int currentIndex = -1;

    private void OnEnable()
    {
        TabClick(0);
    }

    public void TabClick(int index)
    {
        if (tab == null || tab.Length == 0) return;
        if (index < 0 || index >= tab.Length) return;

        currentIndex = index;

        // 패널 전환
        for (int i = 0; i < tab.Length; i++)
        {
            if (tab[i] != null)
                tab[i].SetActive(i == index);
        }

        // 버튼 색상 처리
        for (int i = 0; i < tabBtnImage.Length; i++)
        {
            if (tabBtnImage[i] == null) continue;
            tabBtnImage[i].color = (i == index) ? selectedColor : dimColor;
        }
    }
}