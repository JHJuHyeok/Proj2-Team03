using UnityEngine;
using UnityEngine.UI;

/*
[승문]
CharacterSubTabManager
-캐릭터 탭 안의 강화 / 성장 / 승급 전용 탭매니저
-선택된 ScrollView만 활성화해서 겹친 ScrollView가 입력을 막지 않게 함
-extraTab도 같은 인덱스로 같이 전환
*/
public class CharacterSubTabManager : MonoBehaviour
{
    [Header("Main Tabs")]
    [SerializeField] private GameObject[] subTabs;
    // 0: Enhance Scroll View
    // 1: Growth Scroll View
    // 2: Promotion Scroll View

    [Header("Extra Tabs (Optional)")]
    [SerializeField] private GameObject[] extraTabs;
    // subTabs와 같은 인덱스로 같이 켜고 끌 대상

    [Header("Buttons")]
    [SerializeField] private Image[] tabBtnImage;

    [Header("Colors")]
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color dimColor = new Color(1f, 1f, 1f, 0.45f);

    [Header("Default")]
    [SerializeField] private int defaultIndex = 0;
    [SerializeField] private bool keepLastIndexOnEnable = false;

    private int currentIndex = -1;

    private void OnEnable()
    {
        int idx = defaultIndex;

        if (keepLastIndexOnEnable && currentIndex >= 0)
        {
            idx = currentIndex;
        }

        TabClick(idx);
    }

    public void TabClick(int index)
    {
        if (subTabs == null || subTabs.Length == 0) return;
        if (index < 0 || index >= subTabs.Length) return;

        currentIndex = index;

        ApplyTabs(subTabs, index);
        ApplyTabs(extraTabs, index);
        ApplyButtonColors(index);
        ResetScroll(index);
    }

    private void ApplyTabs(GameObject[] targets, int index)
    {
        if (targets == null || targets.Length == 0) return;

        for (int i = 0; i < targets.Length; i++)
        {
            if (targets[i] == null) continue;
            targets[i].SetActive(i == index);
        }
    }

    private void ApplyButtonColors(int index)
    {
        if (tabBtnImage == null || tabBtnImage.Length == 0) return;

        for (int i = 0; i < tabBtnImage.Length; i++)
        {
            if (tabBtnImage[i] == null) continue;
            tabBtnImage[i].color = (i == index) ? selectedColor : dimColor;
        }
    }

    private void ResetScroll(int index)
    {
        if (index < 0 || index >= subTabs.Length) return;
        if (subTabs[index] == null) return;

        ScrollRect scroll = subTabs[index].GetComponent<ScrollRect>();
        if (scroll == null)
        {
            scroll = subTabs[index].GetComponentInChildren<ScrollRect>(true);
        }

        if (scroll == null) return;

        Canvas.ForceUpdateCanvases();
        scroll.verticalNormalizedPosition = 1f;
    }
}