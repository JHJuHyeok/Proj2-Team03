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

    [Header("Equip ID 옵션 (동적 설정용)")]
    [SerializeField] private string equipId;

    private Button _btn;

    private void Awake()
    {
        _btn = GetComponent<Button>();
        _btn.onClick.AddListener(Open);
    }

    public void Open()
    {
        if (PopupManager.Instance == null)
        {
            Debug.LogWarning("[PopupOpenButton] PopupManager.Instance가 null입니다.");
            return;
        }

        object param = null;

        if (useEquipParam)
        {
            // Equip 팝업은 equipId가 있어야 정확한 장비를 열 수 있음
            if (string.IsNullOrEmpty(equipId))
            {
                Debug.LogWarning($"[PopupOpenButton] Equip 팝업 열기 실패 - equipId가 비어있음. popupId={popupId}, equipTab={equipTab}");
                return;
            }

            param = new EquipPopupParam(equipTab, equipId);
            Debug.Log($"[PopupOpenButton] EquipPopupParam 생성 - tab={equipTab}, equipId={equipId}");
        }
        else if (useSkillParam)
        {
            param = skillAttribute;
            Debug.Log($"[PopupOpenButton] Skill param 사용 - skillAttribute={skillAttribute}");
        }
        else if (useShopParam)
        {
            param = shopTab;
            Debug.Log($"[PopupOpenButton] Shop param 사용 - shopTab={shopTab}");
        }

        Debug.Log($"[PopupOpenButton] Popup open 호출 - popupId={popupId}, param={(param != null ? param.ToString() : "NULL")}");
        PopupManager.Instance.Open(popupId, param);
    }

    /// <summary>
    /// 외부에서 equipId를 동적으로 설정
    /// AccessoryBundleUI 등에서 클릭 시 해당 장비 ID 전달용
    /// </summary>
    public void SetEquipId(string id)
    {
        equipId = id;
        Debug.Log($"[PopupOpenButton] SetEquipId 호출 - equipId={equipId}");
    }
}