using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
/// <summary>
/// 
/// </summary>
public class EquipmentCatalog : MonoBehaviour
{
    public static EquipmentCatalog I { get; private set; }

    [Header("Resources JSON Paths (without extension)")]
    [SerializeField] private string weaponJsonPath = "Data/WeaponList";
    [SerializeField] private string accessorieJsonPath = "Data/AccessorieList";
    [SerializeField] private string skillJsonPath = "Data/SkillList";

    private readonly Dictionary<string, EquipmentDto> weaponById = new();
    private readonly Dictionary<string, EquipmentDto> accessorieById = new();
    private readonly Dictionary<string, SkillDto> skillById = new();

    public IReadOnlyList<EquipmentDto> Weapons { get; private set; } = Array.Empty<EquipmentDto>();
    public IReadOnlyList<EquipmentDto> Accessories { get; private set; } = Array.Empty<EquipmentDto>();
    public IReadOnlyList<SkillDto> Skills { get; private set; } = Array.Empty<SkillDto>();

    private void Awake()
    {
        if (I != null) { Destroy(gameObject); return; }
        I = this;
        DontDestroyOnLoad(gameObject);

        ReloadAll();
    }

    public void ReloadAll()
    {
        Weapons = LoadEquipmentList(weaponJsonPath, weaponById);
        Accessories = LoadEquipmentList(accessorieJsonPath, accessorieById);
        Skills = LoadSkillList(skillJsonPath, skillById);
    }

    private static IReadOnlyList<EquipmentDto> LoadEquipmentList(string resourcesPath, Dictionary<string, EquipmentDto> byId)
    {
        byId.Clear();

        var ta = Resources.Load<TextAsset>(resourcesPath);
        if (ta == null)
        {
            Debug.LogError($"[EquipmentCatalog] TextAsset not found at Resources/{resourcesPath}.json");
            return Array.Empty<EquipmentDto>();
        }

        var dto = JsonUtility.FromJson<EquipmentListDto>(ta.text);
        if (dto == null || dto.equipList == null)
        {
            Debug.LogError($"[EquipmentCatalog] Invalid JSON at {resourcesPath}");
            return Array.Empty<EquipmentDto>();
        }

        foreach (var e in dto.equipList)
        {
            if (e == null || string.IsNullOrWhiteSpace(e.id)) continue;
            byId[e.id] = e;
        }

        return dto.equipList.Where(x => x != null).ToList();
    }

    private static IReadOnlyList<SkillDto> LoadSkillList(string resourcesPath, Dictionary<string, SkillDto> byId)
    {
        byId.Clear();

        var ta = Resources.Load<TextAsset>(resourcesPath);
        if (ta == null)
        {
            Debug.LogError($"[EquipmentCatalog] TextAsset not found at Resources/{resourcesPath}.json");
            return Array.Empty<SkillDto>();
        }

        var dto = JsonUtility.FromJson<SkillListDto>(ta.text);
        if (dto == null || dto.skillList == null)
        {
            Debug.LogError($"[EquipmentCatalog] Invalid JSON at {resourcesPath}");
            return Array.Empty<SkillDto>();
        }

        foreach (var s in dto.skillList)
        {
            if (s == null || string.IsNullOrWhiteSpace(s.id)) continue;
            byId[s.id] = s;
        }

        return dto.skillList.Where(x => x != null).ToList();
    }

    public bool TryGetEquipment(EquipmentKind kind, string id, out EquipmentDto dto)
    {
        dto = null;
        if (string.IsNullOrWhiteSpace(id)) return false;

        return kind switch
        {
            EquipmentKind.Weapon => weaponById.TryGetValue(id, out dto),
            EquipmentKind.Accessorie => accessorieById.TryGetValue(id, out dto),
            _ => false
        };
    }

    public bool TryGetSkill(string id, out SkillDto dto)
        => skillById.TryGetValue(id, out dto);
}