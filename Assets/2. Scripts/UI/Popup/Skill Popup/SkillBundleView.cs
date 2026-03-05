using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
[승문]
SkillBundleView
-스킬 메인 메뉴의 "한 칸" UI 뷰
-버튼/텍스트/슬라이더를 자동 연결하고 스킬 데이터로 표시 갱신
-슬롯 클릭/플러스 클릭 콜백을 외부로 전달
*/
public class SkillBundleView : MonoBehaviour
{
    [SerializeField] private Button slotButton;//슬롯 클릭 버튼
    [SerializeField] private Image iconImage;//스킬 아이콘
    [SerializeField] private TMP_Text nameText;//스킬 이름
    [SerializeField] private Slider progressSlider;//진행 슬라이더(있으면 사용)
    [SerializeField] private Button plusButton;//플러스 버튼(있으면 사용)

    private SkillData bound;
    private System.Action<SkillData> onClickSlot;
    private System.Action<SkillData> onClickPlus;

    private void Awake()
    {
        //참조 자동 연결
        if (slotButton == null)
        {
            slotButton = GetComponentInChildren<Button>(true);
        }

        if (iconImage == null)
        {
            Transform t = transform.Find("Skill Slot Button/Icon Image");
            if (t != null) iconImage = t.GetComponent<Image>();
            if (iconImage == null) iconImage = GetComponentInChildren<Image>(true);
        }

        if (nameText == null)
        {
            Transform t = transform.Find("Skill Name Text");
            if (t != null) nameText = t.GetComponent<TMP_Text>();
            if (nameText == null) nameText = GetComponentInChildren<TMP_Text>(true);
        }

        if (progressSlider == null)
        {
            Transform t = transform.Find("Skill Slider");
            if (t != null) progressSlider = t.GetComponent<Slider>();
            if (progressSlider == null) progressSlider = GetComponentInChildren<Slider>(true);
        }

        if (plusButton == null)
        {
            Transform t = transform.Find("Plus Button");
            if (t != null) plusButton = t.GetComponent<Button>();
        }

        //이벤트 연결
        if (slotButton != null)
        {
            slotButton.onClick.AddListener(HandleSlotClick);
        }

        if (plusButton != null)
        {
            plusButton.onClick.AddListener(HandlePlusClick);
        }

        SetActive(false);
    }

    //슬롯 활성/비활성
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }

    //데이터 바인딩
    public void BindData(SkillData data, Sprite icon, float slider01, System.Action<SkillData> clickSlot, System.Action<SkillData> clickPlus)
    {
        bound = data;
        onClickSlot = clickSlot;
        onClickPlus = clickPlus;

        if (iconImage != null)
        {
            iconImage.sprite = icon;
        }

        if (nameText != null)
        {
            nameText.text = data != null ? data.name : "";
        }

        if (progressSlider != null)
        {
            progressSlider.value = slider01;
        }
    }

    //현재 바인딩된 스킬 반환
    public SkillData GetBound()
    {
        return bound;
    }

    //슬롯 클릭
    private void HandleSlotClick()
    {
        if (bound == null) return;
        if (onClickSlot == null) return;
        onClickSlot(bound);
    }

    //플러스 클릭
    private void HandlePlusClick()
    {
        if (bound == null) return;
        if (onClickPlus == null) return;
        onClickPlus(bound);
    }
}