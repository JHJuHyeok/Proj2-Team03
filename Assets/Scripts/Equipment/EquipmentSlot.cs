using UnityEngine;

namespace SlayerLegend.Equipment
{
    /// <summary>
    /// 장비 슬롯 클래스
    /// 개별 슬롯의 상태를 관리
    /// </summary>
    public class EquipmentSlot
    {
        /// <summary>
        /// 슬롯 타입
        /// </summary>
        public EquipType SlotType { get; private set; }

        /// <summary>
        /// 현재 착용 중인 장비
        /// </summary>
        public EquipData EquippedItem { get; private set; }

        /// <summary>
        /// 장비 레벨
        /// </summary>
        public int Level { get; private set; }

        /// <summary>
        /// 슬롯이 비어있는지
        /// </summary>
        public bool IsEmpty => EquippedItem == null;

        public EquipmentSlot(EquipType slotType)
        {
            SlotType = slotType;
            Level = 1; // 기본 레벨 1
        }

        /// <summary>
        /// 장비 장착
        /// </summary>
        /// <param name="equipment">장착할 장비</param>
        /// <param name="level">장비 레벨</param>
        /// <returns>장착된 장비 (기존 장비가 있으면 반환)</returns>
        public EquipData Equip(EquipData equipment, int level = 1)
        {
            EquipData previousItem = EquippedItem;
            EquippedItem = equipment;
            Level = level;
            return previousItem;
        }

        /// <summary>
        /// 장비 해제
        /// </summary>
        /// <returns>해제된 장비</returns>
        public EquipData Unequip()
        {
            EquipData item = EquippedItem;
            EquippedItem = null;
            Level = 1;
            return item;
        }

        /// <summary>
        /// 장비 레벨 설정
        /// </summary>
        public void SetLevel(int level)
        {
            Level = Mathf.Max(1, level);
        }

        /// <summary>
        /// 슬롯 초기화
        /// </summary>
        public void Clear()
        {
            EquippedItem = null;
            Level = 1;
        }
    }
}
