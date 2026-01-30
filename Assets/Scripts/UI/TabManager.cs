using UnityEngine;
using UnityEngine.UI;

/*
TabManager
-서브탭(강화/성장/승급) 버튼 클릭으로 패널을 전환
-Idle/Select 스프라이트로 버튼 상태 표시
-배열 길이 불일치/누락 참조를 방어하여 팀 작업에서 터지는 걸 방지
*/
public class TabManager : MonoBehaviour
{
    [SerializeField] private GameObject[] tab;
    [SerializeField] private Image[] tabBtnImage;
    [SerializeField] private Sprite[] idleSprite;
    [SerializeField] private Sprite[] selectSprite;

    private int Count => tab != null ? tab.Length : 0;

    private void Start()
    {
        if (Count <= 0) return;

        TabClick(0);
    }

    public void TabClick(int n)
    {
        if (Count <= 0) return;

        if (n < 0) n = 0;
        if (n > Count - 1) n = Count - 1;

        for (int i = 0; i < Count; i++)
        {
            //탭 패널 전환
            if (tab[i] != null)
            {
                tab[i].SetActive(i == n);
            }

            //버튼 이미지 스프라이트 전환
            if (tabBtnImage != null && i < tabBtnImage.Length && tabBtnImage[i] != null)
            {
                Sprite idle = idleSprite != null && i < idleSprite.Length ? idleSprite[i] : null;
                Sprite select = selectSprite != null && i < selectSprite.Length ? selectSprite[i] : null;

                tabBtnImage[i].sprite = i == n ? select : idle;
            }
        }
    }
}
