using UnityEngine;

namespace SlayerLegend.Equipment
{
    // 장비 슬롯 클래스
    // 개별 슬롯의 상태를 관리
    public class EquipmentSlot
    {
        // 슬롯 타입
        public EquipType SlotType { get; private set; }

        // 현재 착용 중인 장비
        public EquipData EquippedItem { get; private set; }

        // 장비 레벨
        public int Level { get; private set; }

        // 슬롯이 비어있는지
        public bool IsEmpty => EquippedItem == null;

        public EquipmentSlot(EquipType slotType)
        {
            SlotType = slotType;
            Level = 1; // 기본 레벨 1
        }

        // 장비 장착
        public EquipData Equip(EquipData equipment, int level = 1)
        {
            EquipData previousItem = EquippedItem;
            EquippedItem = equipment;
            Level = level;
            return previousItem;
        }

        // 장비 해제
        public EquipData Unequip()
        {
            EquipData item = EquippedItem;
            EquippedItem = null;
            Level = 1;
            return item;
        }

        // 장비 레벨 설정
        public void SetLevel(int level)
        {
            Level = Mathf.Max(1, level);
        }

        // 슬롯 초기화
        public void Clear()
        {
            EquippedItem = null;
            Level = 1;
        }
    }
}
