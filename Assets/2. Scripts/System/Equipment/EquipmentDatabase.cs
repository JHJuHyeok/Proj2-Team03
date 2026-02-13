using System;

namespace SlayerLegend.Equipment
{
    // 장비 데이터베이스
    // 등급별 장비 조회 기능 제공
    public class EquipmentDatabase
    {
        private static EquipData[] cachedWeapons;
        private static EquipData[] cachedAccessories;

        // 데이터베이스 초기화
        public static void Initialize()
        {
            cachedWeapons = TestEquipmentData.CreateTestWeapons();
            cachedAccessories = TestEquipmentData.CreateTestAccessories();
        }

        // 등급으로 장비 찾기
        public static EquipData GetEquipmentByGrade(EquipType type, EquipGrade grade)
        {
            EquipData[] source = type == EquipType.Weapon ? cachedWeapons : cachedAccessories;

            foreach (var item in source)
            {
                if (item.GetGrade() == grade)
                    return item;
            }
            return null;
        }

        // 다음 등급 장비 찾기 (융합용)
        public static EquipData GetNextGradeEquipment(EquipData current)
        {
            if (current == null) return null;

            EquipGrade currentGrade = current.GetGrade();
            if (currentGrade >= EquipGrade.Myth)
                return null; // Myth 이상은 합성 불가

            EquipGrade nextGrade = currentGrade + 1;
            EquipType type = GetEquipmentType(current);
            return GetEquipmentByGrade(type, nextGrade);
        }

        // 장비 타입 결정
        private static EquipType GetEquipmentType(EquipData equipment)
        {
            if (equipment == null) return EquipType.Accessorie;

            string id = equipment.GetId();
            if (string.IsNullOrEmpty(id)) return EquipType.Accessorie;

            return id.StartsWith("weapon_", StringComparison.OrdinalIgnoreCase)
                ? EquipType.Weapon
                : EquipType.Accessorie;
        }
    }
}
