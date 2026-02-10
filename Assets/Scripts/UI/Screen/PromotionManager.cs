using UnityEngine;

/*
[승문]
PromotionManager
-현재 승급 등급을 관리
-승급 성공 시 자동으로 다음 등급으로 업데이트
-승급 탭의 모든 PromotionSlotUI 상태를 갱신(완료/도전/잠금)
*/
public class PromotionManager : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private PromotionSlotUI[] slots;

    [Header("Current Rank")]
    [SerializeField] private EnumUI.SlotKey currentRankKey = EnumUI.SlotKey.STONE;

    private void Start()
    {
        RefreshAll();
    }

    // 현재 등급 조회(데이터 연결용)
    public EnumUI.SlotKey GetCurrentRank()
    {
        return currentRankKey;
    }

    // 데이터 로드 후 현재 등급을 세팅하고 UI 반영
    public void SetCurrentRank(EnumUI.SlotKey rankKey)
    {
        currentRankKey = rankKey;
        RefreshAll();
    }

    // "승급 성공" 콜백에서 호출하면 다음 등급으로 자동 상승
    public void OnPromotionSuccess()
    {
        // 최고 등급이면 유지
        if (currentRankKey == EnumUI.SlotKey.ETHER)
        {
            RefreshAll();
            return;
        }

        currentRankKey = EnumUITables.GetNextPromotionKey(currentRankKey);
        RefreshAll();
    }

    // 슬롯 UI 전체 상태 갱신
    public void RefreshAll()
    {
        if (slots == null || slots.Length == 0) return;

        for (int i = 0; i < slots.Length; i++)
        {
            if (slots[i] != null)
            {
                slots[i].RefreshState(currentRankKey);
            }
        }
    }

    // 테스트용(버튼에 연결해서 누르면 한 단계씩 올라감)
    public void DebugPromoteOnce()
    {
        OnPromotionSuccess();
    }
}
