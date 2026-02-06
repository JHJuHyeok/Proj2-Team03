using System;

namespace SlayerLegend.Equipment
{
    [Serializable]
    public class InventoryItem : IEquatable<InventoryItem>
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
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(equipment, other.equipment) && level == other.level;
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
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(InventoryItem left, InventoryItem right)
        {
            return !(left == right);
        }
    }
}
