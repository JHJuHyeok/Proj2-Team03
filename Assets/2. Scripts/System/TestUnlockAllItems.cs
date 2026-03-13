using UnityEngine;
using SlayerLegend.Skill;
using SlayerLegend.Equipment;

/// <summary>
/// 모든 스킬/장비/악세서리 해금 테스트 스크립트
/// 씬에 게임오브젝트로 추가하고 ContextMenu로 실행
/// </summary>
public class TestUnlockAllItems : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private bool unlockOnStart = false;
    [SerializeField] private int defaultCount = 10;
    [SerializeField] private int defaultLevel = 1;

    private void Start()
    {
        if (unlockOnStart)
        {
            UnlockAll();
        }
    }

    [ContextMenu("모든 아이템 해금")]
    public void UnlockAll()
    {
        UnlockAllSkills();
        UnlockAllEquipment();
        Debug.Log("[TestUnlockAllItems] 모든 아이템 해금 완료!");
    }

    [ContextMenu("모든 스킬 해금")]
    public void UnlockAllSkills()
    {
        if (DataManager.skills == null)
        {
            Debug.LogWarning("[TestUnlockAllItems] DataManager.skills가 null입니다.");
            return;
        }

        var skillController = FindFirstObjectByType<SkillController>();
        if (skillController == null)
        {
            Debug.LogWarning("[TestUnlockAllItems] SkillController를 찾을 수 없습니다.");
            return;
        }

        var allSkills = DataManager.skills.GetAll();
        int count = 0;

        foreach (var skill in allSkills)
        {
            if (!string.IsNullOrEmpty(skill.id))
            {
                skillController.AddSkill(skill.id, defaultCount, defaultLevel);
                count++;
            }
        }

        Debug.Log($"[TestUnlockAllItems] 스킬 {count}개 해금 완료");
    }

    [ContextMenu("모든 장비 해금")]
    public void UnlockAllEquipment()
    {
        if (EquipmentManager.Instance == null)
        {
            Debug.LogWarning("[TestUnlockAllItems] EquipmentManager.Instance가 null입니다.");
            return;
        }

        int count = 0;

        // 무기 해금
        if (DataManager.weapons != null)
        {
            var allWeapons = DataManager.weapons.GetAll();
            foreach (var weapon in allWeapons)
            {
                if (!string.IsNullOrEmpty(weapon.id))
                {
                    EquipmentManager.Instance.AddEquipment(weapon.id, defaultCount, defaultLevel);
                    count++;
                }
            }
        }

        // 악세서리 해금
        if (DataManager.accessories != null)
        {
            var allAccessories = DataManager.accessories.GetAll();
            foreach (var accessory in allAccessories)
            {
                if (!string.IsNullOrEmpty(accessory.id))
                {
                    EquipmentManager.Instance.AddEquipment(accessory.id, defaultCount, defaultLevel);
                    count++;
                }
            }
        }

        Debug.Log($"[TestUnlockAllItems] 장비 {count}개 해금 완료");
    }

    [ContextMenu("스킬만 초기화")]
    public void ResetAllSkills()
    {
        if (DataManager.CurrentSaveData?.skillInfo != null)
        {
            DataManager.CurrentSaveData.skillInfo.Clear();
            Debug.Log("[TestUnlockAllItems] 스킬 초기화 완료");
        }
    }

    [ContextMenu("장비만 초기화")]
    public void ResetAllEquipment()
    {
        if (DataManager.CurrentSaveData?.equipInfo != null)
        {
            DataManager.CurrentSaveData.equipInfo.Clear();
            Debug.Log("[TestUnlockAllItems] 장비 초기화 완료");
        }
    }

    [ContextMenu("전체 초기화")]
    public void ResetAll()
    {
        ResetAllSkills();
        ResetAllEquipment();
        Debug.Log("[TestUnlockAllItems] 전체 초기화 완료");
    }
}
