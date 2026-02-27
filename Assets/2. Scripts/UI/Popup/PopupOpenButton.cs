using UnityEngine;
using UnityEngine.UI;

/*
[승문]
PopupOpenButton
- 버튼에 붙여서 PopupManager.Open()을 호출하는 공통 스크립트
- popupId로 어떤 팝업을 열지 결정
- 필요 시 Equip / Skill / Shop 파라미터 전달 가능
- 버튼마다 이 스크립트 하나만 붙이면 됨
*/
[RequireComponent(typeof(Button))]
public class PopupOpenButton : MonoBehaviour
{
    [Header("기본 팝업 종류")]
    [SerializeField] private PopupId popupId = PopupId.None;

    [Header("Equip 옵션 (Equip 팝업일 때만 사용)")]
    [SerializeField] private bool useEquipParam = false;
    [SerializeField] private EquipTab equipTab;

    [Header("Skill 옵션 (Skill 팝업일 때만 사용)")]
    [SerializeField] private bool useSkillParam = false;
    [SerializeField] private SkillAttribute skillAttribute;

    [Header("Shop 옵션 (Shop 팝업일 때만 사용)")]
    [SerializeField] private bool useShopParam = false;
    [SerializeField] private ShopTab shopTab;

    private Button _btn;

    private void Awake()
    {
        // 버튼 컴포넌트 자동 연결
        _btn = GetComponent<Button>();

        // OnClick 이벤트에 Open() 자동 등록
        _btn.onClick.AddListener(Open);
    }

    /// <summary>
    /// 버튼 클릭 시 호출
    /// PopupId와 설정된 파라미터에 따라 팝업 열기
    /// </summary>
    public void Open()
    {
        if (PopupManager.Instance == null) return;

        object param = null;

        // Equip 파라미터 사용 시
        if (useEquipParam)
        {
            param = equipTab;
        }
        // Skill 파라미터 사용 시
        else if (useSkillParam)
        {
            param = skillAttribute;
        }
        // Shop 파라미터 사용 시
        else if (useShopParam)
        {
            param = shopTab;
        }

        PopupManager.Instance.Open(popupId, param);
    }
}