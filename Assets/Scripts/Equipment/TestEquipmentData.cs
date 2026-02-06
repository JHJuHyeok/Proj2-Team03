using UnityEngine;

namespace SlayerLegend.Equipment
{
    // 테스트용 장비 데이터 생성기
    // JSON 없이 코드로 더미 장비 생성
    public static class TestEquipmentData
    {
        // 테스트용 무기 목록 생성
        public static EquipData[] CreateTestWeapons()
        {
            return new EquipData[]
            {
                // 1. 녹슨 검 (Common, 장착: 공격 7%, 보유: 공격 2.1%)
                CreateWeapon("weapon_001", "녹슨 검", EquipGrade.Common,
                    CreateEffect(EffectType.AttackBoost, 7, 2.1f),
                    new ItemEffect[] { CreateEffect(EffectType.AttackBoost, 2.1f, 0.63f) }),

                // 2. 은검 (Uncommon, 장착: 공격 35%, 보유: 공격 10.5%)
                CreateWeapon("weapon_002", "은검", EquipGrade.Uncommon,
                    CreateEffect(EffectType.AttackBoost, 35, 10.5f),
                    new ItemEffect[] { CreateEffect(EffectType.AttackBoost, 10.5f, 3.15f) }),

                // 3. 덩굴 레이피어 (Rare, 장착: 공격 170%, 보유: 공격 51%/크리뎀 1%)
                CreateWeapon("weapon_003", "덩굴 레이피어", EquipGrade.Rare,
                    CreateEffect(EffectType.AttackBoost, 170, 51),
                    new ItemEffect[]
                    {
                        CreateEffect(EffectType.AttackBoost, 51, 15.3f),
                        CreateEffect(EffectType.CriticalDamage, 0.01f, 0.003f)
                    }),

                // 4. 광기의 칼날 (Hero, 장착: 공격 800%, 보유: 공격 240%/크리뎀 2%)
                CreateWeapon("weapon_004", "광기의 칼날", EquipGrade.Hero,
                    CreateEffect(EffectType.AttackBoost, 800, 240),
                    new ItemEffect[]
                    {
                        CreateEffect(EffectType.AttackBoost, 240, 72),
                        CreateEffect(EffectType.CriticalDamage, 0.02f, 0.006f)
                    }),

                // 5. 혹한의 칼날 (Legend, 장착: 공격 10000%, 보유: 공격 3000%/크리뎀 5%)
                CreateWeapon("weapon_005", "혹한의 칼날", EquipGrade.Legend,
                    CreateEffect(EffectType.AttackBoost, 10000, 3000),
                    new ItemEffect[]
                    {
                        CreateEffect(EffectType.AttackBoost, 3000, 900),
                        CreateEffect(EffectType.CriticalDamage, 0.05f, 0.015f)
                    }),

                // 6. 파괴의 검-루인 (Myth, 장착: 공격 100000%, 보유: 공격 30000%/크리뎀 10%/골드 10%)
                CreateWeapon("weapon_006", "파괴의 검-루인", EquipGrade.Myth,
                    CreateEffect(EffectType.AttackBoost, 100000, 30000),
                    new ItemEffect[]
                    {
                        CreateEffect(EffectType.AttackBoost, 30000, 9000),
                        CreateEffect(EffectType.CriticalDamage, 0.1f, 0.03f),
                        CreateEffect(EffectType.GoldGain, 10, 3)
                    })
            };
        }

        // 테스트용 악세서리 목록 생성
        public static EquipData[] CreateTestAccessories()
        {
            return new EquipData[]
            {
                // 1. 녹슨 팔찌 (Common, 장착: 체력/회복 7%, 보유: 체력/회복 2.1%)
                CreateAccessory("accessory_001", "녹슨 팔찌", EquipGrade.Common,
                    CreateEffect(EffectType.HealthBoost, 7, 2.1f),
                    new ItemEffect[] { CreateEffect(EffectType.HealthBoost, 2.1f, 0.63f) }),

                // 2. 순수의 반지 (Uncommon, 장착: 체력/회복 35%, 보유: 체력/회복 10.5%)
                CreateAccessory("accessory_002", "순수의 반지", EquipGrade.Uncommon,
                    CreateEffect(EffectType.HealthBoost, 35, 10.5f),
                    new ItemEffect[] { CreateEffect(EffectType.HealthBoost, 10.5f, 3.15f) }),

                // 3. 소원의 팔찌 (Rare, 장착: 체력/회복 170%, 보유: 체력/회복 51%/마나회복 0.5%)
                CreateAccessory("accessory_003", "소원의 팔찌", EquipGrade.Rare,
                    CreateEffect(EffectType.HealthBoost, 170, 51),
                    new ItemEffect[]
                    {
                        CreateEffect(EffectType.HealthBoost, 51, 15.3f),
                        CreateEffect(EffectType.ManaBoost, 0.5f, 0.15f)
                    }),

                // 4. 단련의 팔찌 (Hero, 장착: 체력/회복 800%, 보유: 체력/회복 240%/마나회복 1%)
                CreateAccessory("accessory_004", "단련의 팔찌", EquipGrade.Hero,
                    CreateEffect(EffectType.HealthBoost, 800, 240),
                    new ItemEffect[]
                    {
                        CreateEffect(EffectType.HealthBoost, 240, 72),
                        CreateEffect(EffectType.ManaBoost, 1, 0.3f)
                    }),

                // 5. 야망의 팔찌 (Legend, 장착: 체력/회복 10000%, 보유: 체력/회복 3000%/마나회복 2%)
                CreateAccessory("accessory_005", "야망의 팔찌", EquipGrade.Legend,
                    CreateEffect(EffectType.HealthBoost, 10000, 3000),
                    new ItemEffect[]
                    {
                        CreateEffect(EffectType.HealthBoost, 3000, 900),
                        CreateEffect(EffectType.ManaBoost, 2, 0.6f)
                    }),

                // 6. 투신의 팔찌 (Myth, 장착: 체력/회복 100000%, 보유: 체력/회복 30000%/마나회복 3%/경험치 1%)
                CreateAccessory("accessory_006", "투신의 팔찌", EquipGrade.Myth,
                    CreateEffect(EffectType.HealthBoost, 100000, 30000),
                    new ItemEffect[]
                    {
                        CreateEffect(EffectType.HealthBoost, 30000, 9000),
                        CreateEffect(EffectType.ManaBoost, 3, 0.9f),
                        CreateEffect(EffectType.ExpGain, 1, 0.3f)
                    })
            };
        }

        // ===== 내부 헬퍼 메서드 =====

        private static EquipData CreateWeapon(string id, string name, EquipGrade grade,
            ItemEffect mainEffect, ItemEffect[] holdEffects = null)
        {
            return new EquipData
            {
                id = id,
                name = name,
                spriteName = $"Icons/Equipment/{id}",
                grade = grade,
                equipEffect = mainEffect,
                holdEffects = new System.Collections.Generic.List<ItemEffect>(holdEffects ?? new ItemEffect[0])
            };
        }

        private static EquipData CreateAccessory(string id, string name, EquipGrade grade,
            ItemEffect mainEffect, ItemEffect[] holdEffects = null)
        {
            return new EquipData
            {
                id = id,
                name = name,
                spriteName = $"Icons/Equipment/{id}",
                grade = grade,
                equipEffect = mainEffect,
                holdEffects = new System.Collections.Generic.List<ItemEffect>(holdEffects ?? new ItemEffect[0])
            };
        }

        private static ItemEffect CreateEffect(EffectType type, float initValue, float levelUpValue)
        {
            return new ItemEffect
            {
                type = type,
                initValue = initValue,
                levelUpValue = levelUpValue
            };
        }
    }
}
