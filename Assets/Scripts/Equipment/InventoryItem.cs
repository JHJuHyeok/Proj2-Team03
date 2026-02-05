using System;

namespace SlayerLegend.Equipment
{
    /// <summary>
    /// 인벤토리에 보관할 장비 아이템
    /// 장비 데이터와 레벨을 함께 저장
    /// </summary>
    [Serializable]
    public struct InventoryItem : IEquatable<InventoryItem>
    {
        public EquipData equipment;
        public int level;

        public InventoryItem(EquipData equipment, int level = 1)
        {
            this.equipment = equipment;
            this.level = level;
        }

        public bool Equals(InventoryItem other)
        {
            return equipment == other.equipment && level == other.level;
        }

        public override bool Equals(object obj)
        {
            return obj is InventoryItem other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (equipment != null ? equipment.GetHashCode() : 0) * 31 + level.GetHashCode();
        }

        public static bool operator ==(InventoryItem left, InventoryItem right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(InventoryItem left, InventoryItem right)
        {
            return !left.Equals(right);
        }
    }
}
