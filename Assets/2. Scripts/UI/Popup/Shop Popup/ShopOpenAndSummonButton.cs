using UnityEngine;
using UnityEngine.UI;

/*
[승문]
ShopOpenAndSummonButton
- 상점 메인 화면 버튼 전용
- 클릭 시 Shop 팝업을 열고 바로 소환 실행
- Weapon / Accessory / Skill 선택 가능
*/

[RequireComponent(typeof(Button))]
public class ShopOpenAndSummonButton : MonoBehaviour
{
    [Header("Popup")]
    [SerializeField] private PopupId popupId = PopupId.Shop;
    [SerializeField] private ShopTab shopTab = ShopTab.Weapon;

    [Header("Summon")]
    [SerializeField] private int summonCount = 1; // 1 / 11 / 33
    [SerializeField] private ShopSummonSystem summonSystem; // 선택 (비워도 자동 탐색)

    private Button button;

    private void Awake()
    {
        button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }

        if (summonSystem == null)
        {
            summonSystem = FindFirstObjectByType<ShopSummonSystem>(FindObjectsInactive.Include);
        }
    }

    private void OnClick()
    {
        if (PopupManager.Instance == null)
        {
            Debug.LogError("[ShopOpenAndSummonButton] PopupManager not found.");
            return;
        }

        // 1️. 팝업 열기
        UIPopup popup = PopupManager.Instance.Open(popupId, shopTab);

        if (popup == null)
        {
            Debug.LogWarning("[ShopOpenAndSummonButton] Popup open failed.");
            return;
        }

        // 2️. 소환 시스템 확보
        if (summonSystem == null)
        {
            summonSystem = FindFirstObjectByType<ShopSummonSystem>(FindObjectsInactive.Include);

            if (summonSystem == null)
            {
                Debug.LogError("[ShopOpenAndSummonButton] ShopSummonSystem not found.");
                return;
            }
        }

        // 3️. 현재 탭 동기화
        summonSystem.SetCurrentTab(shopTab);

        // 4️. 즉시 소환
        bool ok = summonSystem.TryBuyBatch(summonCount);

        Debug.Log(ok
            ? $"[ShopOpenAndSummonButton] Summon {summonCount} success."
            : $"[ShopOpenAndSummonButton] Summon {summonCount} fail.");
    }
}