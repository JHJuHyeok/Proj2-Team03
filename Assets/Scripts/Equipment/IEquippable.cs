namespace SlayerLegend.Equipment
{
    /// <summary>
    /// 장비 효과를 적용받을 수 있는 대상 인터페이스
    /// 상태 관리는 EquipmentManager가 담당하며, 이 인터페이스는 효과 적용만 담당
    /// </summary>
    public interface IEquippable
    {
        /// <summary>
        /// 장비 효과 적용 (착용/해제 시 호출됨)
        /// </summary>
        /// <param name="effect">적용할 효과 데이터</param>
        /// <param name="level">장비 레벨 (레벨에 따른 값 계산용)</param>
        /// <param name="equip">true=착용(효과 추가), false=해제(효과 제거)</param>
        void ApplyEquipmentEffect(ItemEffect effect, int level, bool equip);
    }
}
