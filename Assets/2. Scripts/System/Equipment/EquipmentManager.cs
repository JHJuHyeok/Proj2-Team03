using System;
using System.Collections.Generic;
using UnityEngine;

namespace SlayerLegend.Equipment
{
    // 장비 관리자
    // 모든 장비 상태를 관리하는 단일 소스 (Single Source of Truth)
    public class EquipmentManager : MonoBehaviour
    {
        [Header("장착 대상")]
        [SerializeField] private IEquippable equipTarget;

        // 장비 슬롯들 (슬롯 타입별로 관리)
        private Dictionary<EquipType, EquipmentSlot> equipmentSlots;

        // 인벤토리 (소유한 장비 목록)
        // 같은 장비를 여러 개 보유 가능
        private readonly Dictionary<EquipType, List<InventoryItem>> inventory = new Dictionary<EquipType, List<InventoryItem>>();

        // 현재 적용된 보유 효과 추적 (장비 ID -> 적용됨)
        // 같은 장비를 여러 개 보유해도 중첩 방지
        private readonly HashSet<string> appliedHoldEffects = new HashSet<string>();

        // 인벤토리 변경 이벤트
        public event Action<EquipType> OnInventoryChanged;

        // 장비 변경 이벤트
        public event Action<EquipData, EquipType, int> OnEquipmentChanged;
        public event Action<EquipData, EquipType> OnEquipmentEquipped;
        public event Action<EquipData, EquipType> OnEquipmentUnequipped;

        // 장착 대상
        public IEquippable EquipTarget
        {
            get => equipTarget;
            set
            {
                equipTarget = value;
                // 장착 대상 설정 시 보유 효과 적용
                if (equipTarget != null)
                {
                    RefreshHoldEffects();
                }
            }
        }

        private void Awake()
        {
            InitializeSlots();
        }

        // 슬롯 초기화
        private void InitializeSlots()
        {
            equipmentSlots = new Dictionary<EquipType, EquipmentSlot>
            {
                { EquipType.Weapon, new EquipmentSlot(EquipType.Weapon) },
                { EquipType.Accessorie, new EquipmentSlot(EquipType.Accessorie) }
            };

            // 인벤토리 초기화
            inventory[EquipType.Weapon] = new List<InventoryItem>();
            inventory[EquipType.Accessorie] = new List<InventoryItem>();
        }

        // 장비 장착
        public bool EquipItem(EquipData equipment, int level = 1)
        {
            if (equipment == null)
            {
                Debug.LogWarning("[EquipmentManager] 장비 데이터가 null입니다.");
                return false;
            }

            // Lazy initialization (중요: Awake() 이전에 호출될 수 있음)
            if (equipmentSlots == null)
            {
                InitializeSlots();
            }

            EquipType slotType = DetermineSlotType(equipment);

            if (!equipmentSlots.ContainsKey(slotType))
            {
                Debug.LogError($"[EquipmentManager] 지원하지 않는 슬롯 타입: {slotType}");
                return false;
            }

            EquipmentSlot slot = equipmentSlots[slotType];

            // 기존 장비 해제
            if (!slot.IsEmpty)
            {
                UnequipItem(slotType);
            }

            // 새 장비 장착
            EquipData previousItem = slot.Equip(equipment, level);

            // 효과 적용
            ApplyEffects(equipment, level, equip: true);

            // 보유 효과 재계산 (장착 중인 장비의 보유 효과는 제외)
            RefreshHoldEffects();

            // 이벤트 발생
            OnEquipmentEquipped?.Invoke(equipment, slotType);
            OnEquipmentChanged?.Invoke(equipment, slotType, level);

            string equipName = equipment.GetName();
            Debug.Log($"[EquipmentManager] [{slotType}] {equipName} 장착 완료 (Lv.{level})");

            return true;
        }

        // 장비 해제
        public EquipData UnequipItem(EquipType slotType)
        {
            if (!equipmentSlots.ContainsKey(slotType))
            {
                Debug.LogError($"[EquipmentManager] 지원하지 않는 슬롯 타입: {slotType}");
                return null;
            }

            EquipmentSlot slot = equipmentSlots[slotType];

            if (slot.IsEmpty)
            {
                Debug.LogWarning($"[EquipmentManager] {slotType} 슬롯이 비어있습니다.");
                return null;
            }

            EquipData equipment = slot.Unequip();

            // 효과 해제
            ApplyEffects(equipment, slot.Level, equip: false);

            // 보유 효과 재계산 (장착 해제된 장비의 보유 효과 적용)
            RefreshHoldEffects();

            // 이벤트 발생
            OnEquipmentUnequipped?.Invoke(equipment, slotType);
            OnEquipmentChanged?.Invoke(null, slotType, 1);

            string equipName = equipment.GetName();
            Debug.Log($"[EquipmentManager] [{slotType}] {equipName} 해제 완료");

            return equipment;
        }

        // 특정 장비 해제 (ID 기반 비교)
        public bool UnequipItem(EquipData equipment)
        {
            if (equipment == null) return false;

            string targetId = equipment.GetId();
            if (string.IsNullOrEmpty(targetId)) return false;

            EquipType slotType = DetermineSlotType(equipment);

            if (!equipmentSlots.ContainsKey(slotType))
            {
                return false;
            }

            EquipmentSlot slot = equipmentSlots[slotType];

            // ID 기반 비교로 변경
            if (slot.EquippedItem != null && slot.EquippedItem.GetId() == targetId)
            {
                UnequipItem(slotType);
                return true;
            }

            return false;
        }

        // 모든 장비 해제
        public void UnequipAll()
        {
            foreach (EquipType slotType in equipmentSlots.Keys)
            {
                UnequipItem(slotType);
            }
        }

        // 슬롯의 장비 조회
        public EquipData GetEquipment(EquipType slotType)
        {
            if (!equipmentSlots.ContainsKey(slotType))
            {
                return null;
            }

            return equipmentSlots[slotType].EquippedItem;
        }

        // 슬롯 조회
        public EquipmentSlot GetSlot(EquipType slotType)
        {
            return equipmentSlots.ContainsKey(slotType) ? equipmentSlots[slotType] : null;
        }

        // 모든 슬롯 조회 (읽기 전용)
        public IReadOnlyDictionary<EquipType, EquipmentSlot> GetAllSlots()
        {
            return equipmentSlots;
        }

        // 장비 슬롯 타입 결정
        // ID 접두사를 기준으로 판단 (weapon_* = Weapon, 나머지 = Accessorie)
        private EquipType DetermineSlotType(EquipData equipment)
        {
            if (equipment == null) return EquipType.Accessorie;

            // ID를 기준으로 판단 (JSON 데이터의 id 필드)
            string id = equipment.GetId();

            if (!string.IsNullOrEmpty(id) && id.StartsWith("weapon_", System.StringComparison.OrdinalIgnoreCase))
            {
                return EquipType.Weapon;
            }

            // 기본값은 악세서리
            return EquipType.Accessorie;
        }

        // 장비 효과 적용/해제
        private void ApplyEffects(EquipData equipment, int level, bool equip)
        {
            if (equipment == null || equipTarget == null) return;

            // 메인 효과
            ItemEffect mainEffect = equipment.GetEquipEffect();
            if (mainEffect != null)
            {
                equipTarget.ApplyEquipmentEffect(mainEffect, level, equip);
            }

            // 추가 효과
            System.Collections.Generic.List<ItemEffect> holdEffects = equipment.GetHoldEffects();
            if (holdEffects != null)
            {
                foreach (var effect in holdEffects)
                {
                    equipTarget.ApplyEquipmentEffect(effect, level, equip);
                }
            }
        }

        // 장비 레벨 설정
        public bool SetEquipmentLevel(EquipType slotType, int level)
        {
            if (!equipmentSlots.ContainsKey(slotType))
            {
                return false;
            }

            EquipmentSlot slot = equipmentSlots[slotType];

            if (slot.IsEmpty)
            {
                Debug.LogWarning($"[EquipmentManager] {slotType} 슬롯이 비어있습니다.");
                return false;
            }

            int oldLevel = slot.Level;
            slot.SetLevel(level);

            // 효과 재적용 (해제 후 장착)
            EquipData equipment = slot.EquippedItem;
            if (equipment != null && equipTarget != null)
            {
                // 기존 효과 해제
                ApplyEffects(equipment, oldLevel, equip: false);
                // 새 레벨로 적용
                ApplyEffects(equipment, level, equip: true);

                Debug.Log($"[EquipmentManager] {slotType} 슬롯 레벨 변경: {oldLevel} → {level}");
            }

            return true;
        }

        #region 보유 효과 관리
        // 보유 효과 재계산
        // 인벤토리의 모든 장비 보유 효과를 적용 (중첩 없음)
        private void RefreshHoldEffects()
        {
            if (equipTarget == null) return;

            // 기존 보유 효과 모두 해제
            RemoveAllHoldEffects();

            // 인벤토리의 각 장비에 대해 보유 효과 적용
            foreach (var kvp in inventory)
            {
                EquipType slotType = kvp.Key;
                List<InventoryItem> items = kvp.Value;

                // 해당 슬롯에 장착 중인 장비 확인
                EquipData equippedItem = GetEquipment(slotType);

                foreach (var invItem in items)
                {
                    EquipData equipData = invItem.equipment;
                    string equipId = equipData.GetId();

                    // 장착 중인 장비는 보유 효과 적용 제외
                    if (equippedItem == equipData) continue;

                    // 이미 적용된 장비 ID는 스킵 (중첩 방지)
                    if (appliedHoldEffects.Contains(equipId)) continue;

                    // 보유 효과 적용
                    ApplyHoldEffects(equipData, invItem.level);
                    appliedHoldEffects.Add(equipId);
                }
            }
        }

        // 특정 장비의 보유 효과 적용
        private void ApplyHoldEffects(EquipData equipment, int level)
        {
            if (equipment == null || equipTarget == null) return;

            System.Collections.Generic.List<ItemEffect> holdEffects = equipment.GetHoldEffects();
            if (holdEffects == null) return;

            foreach (var effect in holdEffects)
            {
                // 소스를 "owned_" + 장비 ID로 구분 (장착 효과와 중복 방지)
                string effectSource = $"owned_{equipment.GetId()}_{effect.GetType()}";
                equipTarget.ApplyEquipmentEffect(effect, level, equip: true);
            }
        }

        // 모든 보유 효과 해제
        private void RemoveAllHoldEffects()
        {
            if (equipTarget == null) return;

            foreach (var kvp in inventory)
            {
                foreach (var invItem in kvp.Value)
                {
                    EquipData equipData = invItem.equipment;
                    System.Collections.Generic.List<ItemEffect> holdEffects = equipData.GetHoldEffects();
                    if (holdEffects == null) continue;

                    foreach (var effect in holdEffects)
                    {
                        equipTarget.ApplyEquipmentEffect(effect, invItem.level, equip: false);
                    }
                }
            }

            appliedHoldEffects.Clear();
        }
        #endregion

        #region 인벤토리 관리
        // 장비를 인벤토리에 추가
        // 같은 장비를 여러 개 보유 가능
        public void AddToInventory(EquipData equipment, int level = 1)
        {
            if (equipment == null)
            {
                Debug.LogWarning("[EquipmentManager] 인벤토리에 추가할 장비가 null입니다.");
                return;
            }

            // Lazy initialization
            if (inventory == null || inventory.Count == 0)
            {
                InitializeSlots();
            }

            EquipType slotType = DetermineSlotType(equipment);

            if (!inventory.ContainsKey(slotType))
            {
                inventory[slotType] = new List<InventoryItem>();
            }

            // 장비의 자체 레벨 사용 (EquipData.level 필드)
            inventory[slotType].Add(new InventoryItem(equipment, equipment.level));

            string equipName = equipment.GetName();
            Debug.Log($"[EquipmentManager] 인벤토리에 추가: {equipName} (Lv.{equipment.level}) - 현재 보유량: {GetEquipmentCount(equipment)}");

            // 보유 효과 재계산
            RefreshHoldEffects();

            OnInventoryChanged?.Invoke(slotType);
        }

        // 인벤토리에서 장비 제거 (ID 기반 비교)
        public bool RemoveFromInventory(EquipData equipment)
        {
            if (equipment == null) return false;

            string targetId = equipment.GetId();
            if (string.IsNullOrEmpty(targetId)) return false;

            EquipType slotType = DetermineSlotType(equipment);

            if (!inventory.ContainsKey(slotType))
            {
                return false;
            }

            // ID 기반으로 첫 번째 해당 장비 찾아서 제거
            for (int i = 0; i < inventory[slotType].Count; i++)
            {
                if (inventory[slotType][i].equipment != null &&
                    inventory[slotType][i].equipment.GetId() == targetId)
                {
                    string equipName = equipment.GetName();
                    inventory[slotType].RemoveAt(i);
                    Debug.Log($"[EquipmentManager] 인벤토리에서 제거: {equipName} - 현재 보유량: {GetEquipmentCount(equipment)}");

                    // 보유 효과 재계산
                    RefreshHoldEffects();

                    OnInventoryChanged?.Invoke(slotType);
                    return true;
                }
            }

            return false;
        }

        // 특정 장비의 보유 개수 조회 (ID 기반 비교)
        public int GetEquipmentCount(EquipData equipment)
        {
            if (equipment == null) return 0;

            // ID 기반 비교로 변경 (참조 비교 버그 수정)
            string targetId = equipment.GetId();
            if (string.IsNullOrEmpty(targetId)) return 0;

            EquipType slotType = DetermineSlotType(equipment);

            if (!inventory.ContainsKey(slotType))
            {
                return 0;
            }

            int count = 0;
            foreach (var item in inventory[slotType])
            {
                // ID로 비교 (참조 무시)
                if (item.equipment != null && item.equipment.GetId() == targetId)
                {
                    count++;
                }
            }

            return count;
        }

        // 특정 슬롯 타입의 전체 인벤토리 조회 (읽기 전용)
        public IReadOnlyList<InventoryItem> GetInventory(EquipType slotType)
        {
            if (!inventory.ContainsKey(slotType))
            {
                return System.Array.Empty<InventoryItem>();
            }

            return inventory[slotType];
        }

        // 인벤토리 비우기
        public void ClearInventory(EquipType slotType)
        {
            if (!inventory.ContainsKey(slotType))
            {
                return;
            }

            inventory[slotType].Clear();
            Debug.Log($"[EquipmentManager] {slotType} 인벤토리 비우기 완료");
            OnInventoryChanged?.Invoke(slotType);
        }

        // 전체 인벤토리 비우기
        public void ClearAllInventory()
        {
            foreach (EquipType slotType in inventory.Keys)
            {
                inventory[slotType].Clear();
            }
            Debug.Log("[EquipmentManager] 전체 인벤토리 비우기 완료");

            // 보유 효과 모두 해제
            RemoveAllHoldEffects();

            // 이벤트는 각 슬롯 타입별로 한 번씩 발생
            foreach (EquipType slotType in inventory.Keys)
            {
                OnInventoryChanged?.Invoke(slotType);
            }
        }
        #endregion
    }
}
