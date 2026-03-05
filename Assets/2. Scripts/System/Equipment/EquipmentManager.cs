using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlayerLegend.Equipment
{
    /// <summary>
    /// 장비 관리자 (개선된 버전)
    /// - GameData.equipInfo를 Source of Truth로 사용
    /// - StatManager와 연동하여 스탯 적용
    /// - 강화, 융합, 등급별 조회 기능 제공
    /// </summary>
    public class EquipmentManager : Singleton<EquipmentManager>
    {
        #region 상수
        private const int FUSION_MATERIAL_COUNT = 5; // 융합에 필요한 재료 개수
        #endregion

        #region 장착 상태 (런타임)
        // 현재 장착 중인 장비 ID (타입별)
        private Dictionary<EquipType, string> equippedIds = new Dictionary<EquipType, string>
        {
            { EquipType.Weapon, null },
            { EquipType.Accessorie, null }
        };
        #endregion

        #region 이벤트
        /// <summary>인벤토리 변경 시 발생 (장비 타입)</summary>
        public event Action<EquipType> OnInventoryChanged;

        /// <summary>장비 장착 시 발생 (장비 ID, 타입, 레벨)</summary>
        public event Action<string, EquipType, int> OnEquipmentEquipped;

        /// <summary>장비 해제 시 발생 (장비 ID, 타입)</summary>
        public event Action<string, EquipType> OnEquipmentUnequipped;

        /// <summary>장비 강화 시 발생 (장비 ID, 새 레벨)</summary>
        public event Action<string, int> OnEquipmentEnhanced;

        /// <summary>장비 융합 완료 시 발생 (재료 ID, 결과 ID)</summary>
        public event Action<string, string> OnFusionComplete;
        #endregion

        #region 프로퍼티
        /// <summary>현재 세이브 데이터의 장비 정보에 접근</summary>
        private Dictionary<string, Possesion> EquipInfo
        {
            get
            {
                // 조민희 수정 - null 체크 후 빈 딕셔너리 반환 (에러 로그 제거)
                if (DataManager.CurrentSaveData == null)
                {
                    // CurrentSaveData가 null이면 빈 딕셔너리 반환
                    return new Dictionary<string, Possesion>();
                }
                return DataManager.CurrentSaveData.equipInfo;
            }
        }
        #endregion

        #region 초기화
        protected override void Awake()
        {
            base.Awake();
            Debug.Log("[EquipmentManager] 초기화 완료 (Singleton<EquipmentManager>)");
        }
        #endregion

        #region 조회 메서드

        /// <summary>장비 보유 개수 조회</summary>
        public int GetCount(string equipId)
        {
            if (string.IsNullOrEmpty(equipId)) return 0;
            if (!EquipInfo.ContainsKey(equipId)) return 0;
            return EquipInfo[equipId].count;
        }

        /// <summary>장비 레벨 조회</summary>
        public int GetLevel(string equipId)
        {
            if (string.IsNullOrEmpty(equipId)) return 1;
            if (!EquipInfo.ContainsKey(equipId)) return 1;
            return EquipInfo[equipId].level;
        }

        /// <summary>등급별 보유량 조회</summary>
        public Dictionary<EquipGrade, int> GetCountByGrade(EquipType type)
        {
            var result = new Dictionary<EquipGrade, int>();

            foreach (var kvp in EquipInfo)
            {
                string equipId = kvp.Key;
                EquipData data = GetEquipData(equipId);

                if (data != null && GetEquipType(data) == type)
                {
                    EquipGrade grade = data.GetGrade();
                    if (!result.ContainsKey(grade))
                        result[grade] = 0;
                    result[grade] += kvp.Value.count;
                }
            }

            return result;
        }

        /// <summary>타입별 인벤토리 조회</summary>
        public IReadOnlyList<InventoryItem> GetInventory(EquipType type)
        {
            var result = new List<InventoryItem>();

            foreach (var kvp in EquipInfo)
            {
                string equipId = kvp.Key;
                EquipData data = GetEquipData(equipId);

                if (data != null && GetEquipType(data) == type)
                {
                    result.Add(new InventoryItem(data, kvp.Value.level));
                }
            }

            return result.AsReadOnly();
        }

        /// <summary>장비 데이터 조회 (무기/악세서리 통합)</summary>
        public EquipData GetEquipData(string equipId)
        {
            if (string.IsNullOrEmpty(equipId)) return null;

            // 무기에서 먼저 찾기
            EquipData weapon = DataManager.weapons.Get(equipId);
            if (weapon != null) return weapon;

            // 악세서리에서 찾기
            return DataManager.accessories.Get(equipId);
        }

        /// <summary>장비 타입 결정</summary>
        public EquipType GetEquipType(EquipData equipment)
        {
            if (equipment == null) return EquipType.Accessorie;

            string id = equipment.GetId();
            if (string.IsNullOrEmpty(id)) return EquipType.Accessorie;

            // ID 접두사로 판단 (WP_ = Weapon, AC_ = Accessory)
            if (id.StartsWith("WP_", StringComparison.OrdinalIgnoreCase) ||
                id.StartsWith("weapon_", StringComparison.OrdinalIgnoreCase))
            {
                return EquipType.Weapon;
            }

            return EquipType.Accessorie;
        }

        /// <summary>장착 중인 장비 ID 조회</summary>
        public string GetEquippedId(EquipType type)
        {
            return equippedIds.ContainsKey(type) ? equippedIds[type] : null;
        }

        /// <summary>장착 중인 장비 데이터 조회</summary>
        public EquipData GetEquippedData(EquipType type)
        {
            string equippedId = GetEquippedId(type);
            if (string.IsNullOrEmpty(equippedId)) return null;
            return GetEquipData(equippedId);
        }

        /// <summary>특정 등급의 다음 등급 조회</summary>
        public EquipGrade GetNextGrade(EquipGrade currentGrade)
        {
            if (currentGrade >= EquipGrade.Myth) return EquipGrade.Myth;
            return currentGrade + 1;
        }

        /// <summary>다음 순서의 장비 데이터 찾기 (ID 기반) </summary>
        public EquipData FindNextGradeData(EquipData current)
        {
            if (current == null) return null;

            string currentId = current.GetId();
            if (string.IsNullOrEmpty(currentId)) return null;

            // [조민희] ID에서 숫자 추출 후 +1 (예: WP_004 → WP_005)
            string prefix = currentId.Substring(0, 3); // "WP_" 또는 "AC_"
            if (int.TryParse(currentId.Substring(3), out int num))
            {
                string nextId = $"{prefix}{num + 1:D3}"; // WP_004 → WP_005
                EquipType type = GetEquipType(current);
                var db = type == EquipType.Weapon ? DataManager.weapons : DataManager.accessories;

                foreach (var data in db.GetAll())
                {
                    if (data.GetId() == nextId)
                    {
                        return data;
                    }
                }
            }

            return null;
        }
        #endregion

        #region 장비 획득/제거

        /// <summary>장비 획득</summary>
        public void AddEquipment(string equipId, int count = 1, int level = 1)
        {
            if (string.IsNullOrEmpty(equipId))
            {
                Debug.LogWarning("[EquipmentManager] 장비 ID가 null 또는 비어있습니다.");
                return;
            }

            if (!EquipInfo.ContainsKey(equipId))
            {
                EquipInfo[equipId] = new Possesion { count = 0, level = level };
            }

            EquipInfo[equipId].count += count;
            EquipInfo[equipId].level = Math.Max(EquipInfo[equipId].level, level);

            EquipType type = GetEquipTypeFromId(equipId);
            EquipData data = GetEquipData(equipId);
            string name = data != null ? data.GetName() : equipId;

            Debug.Log($"[EquipmentManager] 장비 획득: {name} x{count} (총 {EquipInfo[equipId].count}개)");

            // 보유 효과 적용 (장착 중이 아닌 경우만)
            if (equippedIds[type] != equipId)
            {
                ApplyHoldEffects(equipId, EquipInfo[equipId].level, apply: true);
            }

            OnInventoryChanged?.Invoke(type);
        }

        /// <summary>장비 제거</summary>
        public bool RemoveEquipment(string equipId, int count = 1)
        {
            if (string.IsNullOrEmpty(equipId)) return false;
            if (!EquipInfo.ContainsKey(equipId)) return false;

            EquipType type = GetEquipTypeFromId(equipId);

            // 장착 중이면 해제
            if (equippedIds[type] == equipId)
            {
                Unequip(type);
            }

            EquipInfo[equipId].count -= count;

            if (EquipInfo[equipId].count <= 0)
            {
                // 보유 효과 제거
                ApplyHoldEffects(equipId, EquipInfo[equipId].level, apply: false);
                EquipInfo.Remove(equipId);
            }

            EquipData data = GetEquipData(equipId);
            string name = data != null ? data.GetName() : equipId;
            Debug.Log($"[EquipmentManager] 장비 제거: {name} x{count}");

            OnInventoryChanged?.Invoke(type);
            return true;
        }
        #endregion

        #region 장착/해제

        /// <summary>장비 장착</summary>
        public bool Equip(string equipId)
        {
            if (string.IsNullOrEmpty(equipId))
            {
                Debug.LogWarning("[EquipmentManager] 장착할 장비 ID가 null입니다.");
                return false;
            }

            if (!EquipInfo.ContainsKey(equipId) || EquipInfo[equipId].count <= 0)
            {
                Debug.LogWarning($"[EquipmentManager] 보유하지 않은 장비입니다: {equipId}");
                return false;
            }

            EquipData data = GetEquipData(equipId);
            if (data == null)
            {
                Debug.LogWarning($"[EquipmentManager] 장비 데이터를 찾을 수 없습니다: {equipId}");
                return false;
            }

            EquipType type = GetEquipType(data);

            // 이미 장착 중인 장비면 무시
            if (equippedIds[type] == equipId)
            {
                Debug.Log($"[EquipmentManager] 이미 장착 중인 장비입니다: {data.GetName()}");
                return true;
            }

            // 기존 장비 해제
            if (!string.IsNullOrEmpty(equippedIds[type]))
            {
                Unequip(type);
            }

            // 장착 효과 적용
            int level = EquipInfo[equipId].level;
            ApplyEquipEffects(equipId, level, apply: true);

            // 보유 효과 제거 (장착 중인 장비는 보유 효과 제외)
            ApplyHoldEffects(equipId, level, apply: false);

            // 장착 상태 업데이트
            equippedIds[type] = equipId;

            Debug.Log($"[EquipmentManager] [{type}] {data.GetName()} 장착 완료 (Lv.{level})");

            OnEquipmentEquipped?.Invoke(equipId, type, level);
            return true;
        }

        /// <summary>장비 해제</summary>
        public bool Unequip(EquipType type)
        {
            string equippedId = equippedIds[type];
            if (string.IsNullOrEmpty(equippedId))
            {
                Debug.Log($"[EquipmentManager] {type} 슬롯이 이미 비어있습니다.");
                return true;
            }

            EquipData data = GetEquipData(equippedId);
            int level = GetLevel(equippedId);

            // 장착 효과 해제
            ApplyEquipEffects(equippedId, level, apply: false);

            // 보유 효과 다시 적용
            if (EquipInfo.ContainsKey(equippedId))
            {
                ApplyHoldEffects(equippedId, level, apply: true);
            }

            string name = data != null ? data.GetName() : equippedId;
            Debug.Log($"[EquipmentManager] [{type}] {name} 해제 완료");

            equippedIds[type] = null;

            OnEquipmentUnequipped?.Invoke(equippedId, type);
            return true;
        }
        #endregion

        #region 강화

        /// <summary>장비 강화</summary>
        public bool Enhance(string equipId)
        {
            if (string.IsNullOrEmpty(equipId)) return false;
            if (!EquipInfo.ContainsKey(equipId)) return false;

            EquipType type = GetEquipTypeFromId(equipId);
            int oldLevel = EquipInfo[equipId].level;

            // [조민희] 강화 비용 체크 및 차감 (CurrencyManager 연동)
            long cost = GetEnhanceCost(oldLevel);
            if (!CurrencyManager.Instance.HasEnoughCurrency(CurrencyType.Cube, cost))
            {
                Debug.LogWarning($"[EquipmentManager] 강화 실패: 큐브 부족 (필요: {cost}, 보유: {CurrencyManager.Instance.GetAmount(CurrencyType.Cube)})");
                return false;
            }
            CurrencyManager.Instance.ConsumeCurrency(CurrencyType.Cube, cost);

            EquipInfo[equipId].level++;

            EquipData data = GetEquipData(equipId);
            string name = data != null ? data.GetName() : equipId;
            Debug.Log($"[EquipmentManager] {name} 강화: Lv.{oldLevel} → Lv.{EquipInfo[equipId].level}");

            // 장착 중이면 스탯 재적용
            if (equippedIds[type] == equipId)
            {
                ApplyEquipEffects(equipId, oldLevel, apply: false);
                ApplyEquipEffects(equipId, EquipInfo[equipId].level, apply: true);
            }
            else
            {
                // 보유 효과 재적용
                ApplyHoldEffects(equipId, oldLevel, apply: false);
                ApplyHoldEffects(equipId, EquipInfo[equipId].level, apply: true);
            }

            OnEquipmentEnhanced?.Invoke(equipId, EquipInfo[equipId].level);
            return true;
        }

        /// <summary>강화 비용 계산</summary>
        public long GetEnhanceCost(int currentLevel)
        {
            // 기본 비용 + 레벨별 증가
            return 1000L * currentLevel * currentLevel;
        }
        #endregion

        #region 융합

        /// <summary>융합 가능 여부 확인</summary>
        public bool CanFuse(string equipId)
        {
            if (string.IsNullOrEmpty(equipId)) return false;
            if (!EquipInfo.ContainsKey(equipId)) return false;
            if (EquipInfo[equipId].count < FUSION_MATERIAL_COUNT) return false;

            EquipData data = GetEquipData(equipId);
            if (data == null) return false;
            if (data.GetGrade() >= EquipGrade.Myth) return false;

            EquipData nextData = FindNextGradeData(data);
            return nextData != null;
        }

        /// <summary>융합 불가 사유 메시지</summary>
        public string GetCannotFuseReason(string equipId)
        {
            if (string.IsNullOrEmpty(equipId)) return "장비 ID가 없습니다.";
            if (!EquipInfo.ContainsKey(equipId)) return "보유하지 않은 장비입니다.";

            int count = EquipInfo[equipId].count;
            if (count < FUSION_MATERIAL_COUNT)
                return $"재료 부족 ({count}/{FUSION_MATERIAL_COUNT})";

            EquipData data = GetEquipData(equipId);
            if (data == null) return "장비 데이터를 찾을 수 없습니다.";

            if (data.GetGrade() >= EquipGrade.Myth)
                return "최고 등급은 융합할 수 없습니다.";

            EquipData nextData = FindNextGradeData(data);
            if (nextData == null)
                return "다음 등급 장비가 없습니다.";

            return "알 수 없는 오류";
        }

        /// <summary>장비 융합 (5개 → 상위 등급 1개)</summary>
        public bool Fuse(string equipId, out string resultId)
        {
            resultId = null;

            if (!CanFuse(equipId))
            {
                Debug.LogWarning($"[EquipmentManager] 융합 불가: {GetCannotFuseReason(equipId)}");
                return false;
            }

            EquipData currentData = GetEquipData(equipId);
            EquipData nextData = FindNextGradeData(currentData);

            if (nextData == null)
            {
                Debug.LogError("[EquipmentManager] 다음 등급 장비를 찾을 수 없습니다.");
                return false;
            }

            // 재료 제거
            EquipInfo[equipId].count -= FUSION_MATERIAL_COUNT;
            if (EquipInfo[equipId].count <= 0)
            {
                // 장착 중이면 해제
                EquipType type = GetEquipTypeFromId(equipId);
                if (equippedIds[type] == equipId)
                {
                    Unequip(type);
                }
                EquipInfo.Remove(equipId);
            }

            // 결과 장비 추가
            resultId = nextData.GetId();
            if (!EquipInfo.ContainsKey(resultId))
            {
                EquipInfo[resultId] = new Possesion { count = 0, level = 1 };
            }
            EquipInfo[resultId].count += 1;

            Debug.Log($"[EquipmentManager] 융합 성공: {currentData.GetName()} x{FUSION_MATERIAL_COUNT} → {nextData.GetName()} x1");

            EquipType equipType = GetEquipType(currentData);
            OnInventoryChanged?.Invoke(equipType);
            OnFusionComplete?.Invoke(equipId, resultId);

            return true;
        }
        #endregion

        #region StatManager 연동

        /// <summary>장착 효과 적용/해제</summary>
        private void ApplyEquipEffects(string equipId, int level, bool apply)
        {
            EquipData data = GetEquipData(equipId);
            if (data == null) return;

            var stats = CalculateStatValues(data, level);
            string sourceKey = $"{SourceKey.Equipment}_{equipId}";

            StatManager.Instance.UpdatePlayerStat(sourceKey, apply ? stats : null);
        }

        /// <summary>보유 효과 적용/해제</summary>
        private void ApplyHoldEffects(string equipId, int level, bool apply)
        {
            EquipData data = GetEquipData(equipId);
            if (data == null) return;

            var holdEffects = data.GetHoldEffects();
            if (holdEffects == null || holdEffects.Count == 0) return;

            var stats = ConvertEffectsToStatValues(holdEffects, level);
            string sourceKey = $"{SourceKey.Collect}_{equipId}";

            StatManager.Instance.UpdatePlayerStat(sourceKey, apply ? stats : null);
        }

        /// <summary>EquipData를 StatValue 리스트로 변환</summary>
        private List<StatValue> CalculateStatValues(EquipData data, int level)
        {
            var stats = new List<StatValue>();

            // 장착 효과
            ItemEffect equipEffect = data.GetEquipEffect();
            if (equipEffect != null)
            {
                float value = equipEffect.initValue + (equipEffect.levelUpValue * (level - 1));
                StatType statType = ConvertEffectTypeToStatType(equipEffect.type);

                stats.Add(new StatValue
                {
                    type = statType,
                    baseValue = 0,
                    multiplier = value / 100f // 퍼센트 → 배율 변환
                });
            }

            return stats;
        }

        /// <summary>ItemEffect 리스트를 StatValue 리스트로 변환</summary>
        private List<StatValue> ConvertEffectsToStatValues(List<ItemEffect> effects, int level)
        {
            var stats = new List<StatValue>();

            foreach (var effect in effects)
            {
                float value = effect.initValue + (effect.levelUpValue * (level - 1));
                StatType statType = ConvertEffectTypeToStatType(effect.type);

                stats.Add(new StatValue
                {
                    type = statType,
                    baseValue = 0,
                    multiplier = value / 100f
                });
            }

            return stats;
        }

        /// <summary>EffectType을 StatType으로 변환</summary>
        private StatType ConvertEffectTypeToStatType(EffectType effectType)
        {
            return effectType switch
            {
                EffectType.AttackBoost => StatType.STR,           // 공격력
                EffectType.CriticalDamage => StatType.CRI_DMG,     // 크리티컬 데미지
                EffectType.GoldGain => StatType.ADD_GOLD,          // 추가 골드
                EffectType.HealthBoost => StatType.HP,             // 체력
                EffectType.ManaBoost => StatType.MANA,             // 마나
                EffectType.ExpGain => StatType.ADD_EXP,            // 추가 경험치
                _ => StatType.STR
            };
        }
        #endregion

        #region 유틸리티

        /// <summary>ID로 장비 타입 판단</summary>
        private EquipType GetEquipTypeFromId(string equipId)
        {
            if (string.IsNullOrEmpty(equipId)) return EquipType.Accessorie;

            if (equipId.StartsWith("WP_", StringComparison.OrdinalIgnoreCase) ||
                equipId.StartsWith("weapon_", StringComparison.OrdinalIgnoreCase))
            {
                return EquipType.Weapon;
            }

            return EquipType.Accessorie;
        }
        #endregion

        #region 디버그
        [ContextMenu("디버그: 등급별 보유량 출력")]
        public void DebugPrintGradeCount()
        {
            var weaponCount = GetCountByGrade(EquipType.Weapon);
            var accessoryCount = GetCountByGrade(EquipType.Accessorie);

            Debug.Log("=== 무기 등급별 보유량 ===");
            foreach (var kvp in weaponCount)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value}개");
            }

            Debug.Log("=== 악세서리 등급별 보유량 ===");
            foreach (var kvp in accessoryCount)
            {
                Debug.Log($"  {kvp.Key}: {kvp.Value}개");
            }
        }

        [ContextMenu("디버그: 전체 인벤토리 출력")]
        public void DebugPrintInventory()
        {
            Debug.Log("=== 전체 장비 인벤토리 ===");
            foreach (var kvp in EquipInfo)
            {
                EquipData data = GetEquipData(kvp.Key);
                string name = data != null ? data.GetName() : kvp.Key;
                Debug.Log($"  {name} ({kvp.Key}): {kvp.Value.count}개, Lv.{kvp.Value.level}");
            }
        }
        #endregion

        #region 테스트용 장비 추가 (조민희 추가)

        [ContextMenu("테스트: 랜덤 장비 추가 (무작위 수량)")]
        public void DebugAddRandomEquipment()
        {
            if (DataManager.CurrentSaveData == null)
            {
                Debug.LogError("[EquipmentManager] CurrentSaveData가 null입니다.");
                return;
            }

            var allWeapons = DataManager.weapons.GetAll();
            var allAccessories = DataManager.accessories.GetAll();

            int addedCount = 0;

            // 무기: 50% 확률로 추가, 수량 1~10개
            foreach (var weapon in allWeapons)
            {
                if (UnityEngine.Random.value < 0.5f)
                {
                    string equipId = weapon.GetId();
                    int count = UnityEngine.Random.Range(1, 11);
                    int level = UnityEngine.Random.Range(1, 6);

                    AddEquipment(equipId, count, level);
                    addedCount++;
                }
            }

            // 악세서리: 30% 확률로 추가, 수량 1~5개
            foreach (var accessory in allAccessories)
            {
                if (UnityEngine.Random.value < 0.3f)
                {
                    string equipId = accessory.GetId();
                    int count = UnityEngine.Random.Range(1, 6);
                    int level = UnityEngine.Random.Range(1, 4);

                    AddEquipment(equipId, count, level);
                    addedCount++;
                }
            }

            Debug.Log($"[EquipmentManager] 랜덤 장비 추가 완료: {addedCount}종류");
        }

        [ContextMenu("테스트: 모든 장비 10개씩 추가")]
        public void DebugAddAllEquipment()
        {
            if (DataManager.CurrentSaveData == null)
            {
                Debug.LogError("[EquipmentManager] CurrentSaveData가 null입니다.");
                return;
            }

            var allWeapons = DataManager.weapons.GetAll();
            var allAccessories = DataManager.accessories.GetAll();

            int addedCount = 0;

            // 모든 무기 10개씩 추가
            foreach (var weapon in allWeapons)
            {
                string equipId = weapon.GetId();
                AddEquipment(equipId, 10, 1);
                addedCount++;
            }

            // 모든 악세서리 10개씩 추가
            foreach (var accessory in allAccessories)
            {
                string equipId = accessory.GetId();
                AddEquipment(equipId, 10, 1);
                addedCount++;
            }

            Debug.Log($"[EquipmentManager] 모든 장비 10개씩 추가 완료: {addedCount}종류");
        }

        [ContextMenu("테스트: 장비 데이터 전체 삭제")]
        public void DebugClearAllEquipment()
        {
            if (DataManager.CurrentSaveData != null && DataManager.CurrentSaveData.equipInfo != null)
            {
                DataManager.CurrentSaveData.equipInfo.Clear();
                Debug.Log("[EquipmentManager] 장비 데이터 전체 삭제 완료");

                // 이벤트 발생
                OnInventoryChanged?.Invoke(EquipType.Weapon);
                OnInventoryChanged?.Invoke(EquipType.Accessorie);
            }
        }
        #endregion
    }
}
