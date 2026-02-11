using UnityEngine;
using UnityEngine.UI;

/*
[승문]
SlotKeyIconBinder
-슬롯 컨테이너에 붙여서 "강화/성장/승급" 그룹만 고르면
-자식 슬롯들에 SlotKey를 순서대로 자동 배정
-아이콘도 '슬롯 순서'대로 받은 Sprite로 자동 세팅
-배정 대상: SetKey(EnumUI.SlotKey, Sprite)를 가진 컴포넌트들
  (EnhanceSlotUI / GrowthSlotUI / PromotionSlotUI / SlotBarAutoText 등)
-EnumUITables에 키 순서 테이블을 두고 여기서는 가져다 쓰기만 함
*/
public class SlotKeyIconBinder : MonoBehaviour
{
    [Header("Group")]
    [SerializeField] private EnumUI.GroupType groupType;

    [Header("Options")]
    [SerializeField] private bool includeInactive = true;

    [Header("Sprites By Slot Order")]
    [Tooltip("표시 순서대로 넣어주세요. (0번=첫 슬롯 아이콘) / 길이는 해당 그룹 슬롯 개수와 맞추는게 좋음")]
    [SerializeField] private Sprite[] spritesBySlotOrder;

#if UNITY_EDITOR
    private void OnValidate()
    {
        Apply();
    }
#endif

    private void Awake()
    {
        Apply();
    }

    public void Apply()
    {
        EnumUI.SlotKey[] keys = EnumUITables.GetKeysByGroup(groupType);
        if (keys == null || keys.Length == 0) return;

        // 슬롯 순서 기준으로 "키 + 아이콘" 같이 배정
        ApplyToEnhance(keys);
        ApplyToGrowth(keys);
        ApplyToPromotion(keys);
        ApplyToSlotBar(keys);
    }

    private Sprite GetIconByIndex(int slotIndex)
    {
        if (spritesBySlotOrder == null || spritesBySlotOrder.Length == 0) return null;
        if (slotIndex < 0 || slotIndex >= spritesBySlotOrder.Length) return null;
        return spritesBySlotOrder[slotIndex];
    }

    private void ApplyToEnhance(EnumUI.SlotKey[] keys)
    {
        var slots = GetComponentsInChildren<EnhanceSlotUI>(includeInactive);
        int count = Mathf.Min(slots.Length, keys.Length);

        for (int i = 0; i < count; i++)
        {
            if (slots[i] == null) continue;
            slots[i].SetKey(keys[i], GetIconByIndex(i));
        }
    }

    private void ApplyToGrowth(EnumUI.SlotKey[] keys)
    {
        var slots = GetComponentsInChildren<GrowthSlotUI>(includeInactive);
        int count = Mathf.Min(slots.Length, keys.Length);

        for (int i = 0; i < count; i++)
        {
            if (slots[i] == null) continue;
            slots[i].SetKey(keys[i], GetIconByIndex(i));
        }
    }

    private void ApplyToPromotion(EnumUI.SlotKey[] keys)
    {
        var slots = GetComponentsInChildren<PromotionSlotUI>(includeInactive);
        int count = Mathf.Min(slots.Length, keys.Length);

        for (int i = 0; i < count; i++)
        {
            if (slots[i] == null) continue;
            slots[i].SetKey(keys[i], GetIconByIndex(i));
        }
    }

    private void ApplyToSlotBar(EnumUI.SlotKey[] keys)
    {
        var bars = GetComponentsInChildren<SlotBarAutoText>(includeInactive);
        int count = Mathf.Min(bars.Length, keys.Length);

        for (int i = 0; i < count; i++)
        {
            if (bars[i] == null) continue;
            bars[i].SetKey(keys[i]);
        }
    }
}
