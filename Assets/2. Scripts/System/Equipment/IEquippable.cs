namespace SlayerLegend.Equipment
{
    // 장비 효과를 적용받을 수 있는 대상 인터페이스
    // 상태 관리는 EquipmentManager가 담당하며, 이 인터페이스는 효과 적용만 담당
    public interface IEquippable
    {
        // 장비 효과 적용 (착용/해제 시 호출됨)
        void ApplyEquipmentEffect(ItemEffect effect, int level, bool equip);
    }
}
