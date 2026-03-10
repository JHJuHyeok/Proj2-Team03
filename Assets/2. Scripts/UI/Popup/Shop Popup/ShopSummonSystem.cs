using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/*
[승문]
ShopSummonSystem
- IShopItemProvider 구현체
- Addressables Address(키)로 Weapon / Accessorie / Skill JSON을 Awake에서 자동 로드
- SetCurrentTab으로 현재 탭 저장
- TryBuyBatch(count)는 현재 탭 기준으로 랜덤 결과 생성
- 간단 버전: 재화/확률/중복처리 없이 랜덤 결과만 생성
*/
public class ShopSummonSystem : MonoBehaviour, IShopItemProvider
{
    [System.Serializable]
    private class ShopItemData
    {
        [SerializeField] private string id;
        [SerializeField] private string name;
        [SerializeField] private string spriteName;
        [SerializeField] private string priceText;
        [SerializeField] private bool canBuy = true;

        public string Id => id;
        public string Name => name;
        public string SpriteName => spriteName;
        public string PriceText => priceText;
        public bool CanBuy => canBuy;

        public void Set(string newId, string newName, string newSpriteName, string newPriceText, bool newCanBuy)
        {
            id = newId;
            name = newName;
            spriteName = newSpriteName;
            priceText = newPriceText;
            canBuy = newCanBuy;
        }
    }

    [System.Serializable]
    private class EquipListRoot
    {
        public string listType;
        public EquipJson[] equipList;
    }

    [System.Serializable]
    private class EquipJson
    {
        public string id;
        public string name;
        public string spriteName;
        public string grade;
        public int gradeStep;
    }

    [System.Serializable]
    private class SkillListRoot
    {
        public string listType;
        public SkillJson[] skillList;
    }

    [System.Serializable]
    private class SkillJson
    {
        public string id;
        public string name;
        public string spriteName;
        public string element;
    }

    [Header("Auto Load(Json by Addressables Key)")]
    [SerializeField] private bool autoLoadOnAwake = true;
    [SerializeField] private string weaponJsonKey = "Json/Equip/WeaponList";
    [SerializeField] private string accessorieJsonKey = "Json/Equip/AccessorieList";
    [SerializeField] private string skillJsonKey = "Json/Skill/SkillList";

    [Header("Tab Items(Runtime)")]
    [SerializeField] private List<ShopItemData> weaponItems = new List<ShopItemData>(33);
    [SerializeField] private List<ShopItemData> accessoryItems = new List<ShopItemData>(33);
    [SerializeField] private List<ShopItemData> skillItems = new List<ShopItemData>(33);

    private readonly List<ShopItemInfo> resultCache = new List<ShopItemInfo>(64);
    private ShopTab currentTab;

    private AsyncOperationHandle<TextAsset> weaponHandle;
    private AsyncOperationHandle<TextAsset> accessorieHandle;
    private AsyncOperationHandle<TextAsset> skillHandle;

    private bool weaponHandleValid;
    private bool accessorieHandleValid;
    private bool skillHandleValid;

    public event System.Action<ShopTab, IReadOnlyList<ShopItemInfo>> OnSummonResult;

    private void Awake()
    {
        currentTab = ShopTab.Weapon;

        if (autoLoadOnAwake)
        {
            LoadFromAddressablesIfPossible();
        }
    }

    private void OnDestroy()
    {
        if (weaponHandleValid)
        {
            Addressables.Release(weaponHandle);
            weaponHandleValid = false;
        }

        if (accessorieHandleValid)
        {
            Addressables.Release(accessorieHandle);
            accessorieHandleValid = false;
        }

        if (skillHandleValid)
        {
            Addressables.Release(skillHandle);
            skillHandleValid = false;
        }
    }

    public void SetCurrentTab(ShopTab tab)
    {
        currentTab = tab;
    }

    public ShopTab GetCurrentTab()
    {
        return currentTab;
    }

    public int GetCount(ShopTab tab)
    {
        List<ShopItemData> list = GetList(tab);
        if (list == null) return 0;

        int count = list.Count;
        if (count > 33) count = 33;

        return count;
    }

    public ShopItemInfo GetItem(ShopTab tab, int index)
    {
        List<ShopItemData> list = GetList(tab);
        if (list == null) return default;
        if (index < 0 || index >= list.Count) return default;

        ShopItemData data = list[index];

        ShopItemInfo info = new ShopItemInfo
        {
            Id = data != null ? data.Id : "",
            Name = data != null ? data.Name : "",
            SpriteName = data != null ? data.SpriteName : "",
            PriceText = data != null ? data.PriceText : "",
            CanBuy = data != null && data.CanBuy
        };

        return info;
    }

    public bool TryBuy(ShopTab tab, int index)
    {
        SetCurrentTab(tab);
        return TryBuyBatch(1);
    }

    public bool TryBuyBatch(ShopTab tab, int count)
    {
        SetCurrentTab(tab);
        return TryBuyBatch(count);
    }

    public bool TryBuyBatch(int count)
    {
        if (count != 1 && count != 11 && count != 33)
        {
            Debug.LogWarning("[ShopSummonSystem] Invalid count.");
            return false;
        }

        List<ShopItemData> list = GetList(currentTab);
        if (list == null || list.Count <= 0)
        {
            Debug.LogWarning("[ShopSummonSystem] Item list empty.");
            return false;
        }

        resultCache.Clear();

        for (int i = 0; i < count; i++)
        {
            resultCache.Add(PickRandom(list));
        }

        Debug.Log("[ShopSummonSystem] Summon done.");
        OnSummonResult?.Invoke(currentTab, resultCache);
        return true;
    }

    private List<ShopItemData> GetList(ShopTab tab)
    {
        switch (tab)
        {
            case ShopTab.Weapon:
                return weaponItems;
            case ShopTab.Accessory:
                return accessoryItems;
            case ShopTab.Skill:
                return skillItems;
        }

        return weaponItems;
    }

    private ShopItemInfo PickRandom(List<ShopItemData> list)
    {
        int idx = Random.Range(0, list.Count);
        ShopItemData data = list[idx];

        ShopItemInfo info = new ShopItemInfo
        {
            Id = data != null ? data.Id : "",
            Name = data != null ? data.Name : "",
            SpriteName = data != null ? data.SpriteName : "",
            PriceText = data != null ? data.PriceText : "",
            CanBuy = data != null && data.CanBuy
        };

        return info;
    }

    private void LoadFromAddressablesIfPossible()
    {
        if (string.IsNullOrEmpty(weaponJsonKey))
        {
            Debug.LogError("[ShopSummonSystem] weaponJsonKey is empty.");
            return;
        }

        if (string.IsNullOrEmpty(accessorieJsonKey))
        {
            Debug.LogError("[ShopSummonSystem] accessorieJsonKey is empty.");
            return;
        }

        if (string.IsNullOrEmpty(skillJsonKey))
        {
            Debug.LogError("[ShopSummonSystem] skillJsonKey is empty.");
            return;
        }

        weaponHandle = Addressables.LoadAssetAsync<TextAsset>(weaponJsonKey);
        weaponHandleValid = true;
        weaponHandle.Completed += OnWeaponLoaded;

        accessorieHandle = Addressables.LoadAssetAsync<TextAsset>(accessorieJsonKey);
        accessorieHandleValid = true;
        accessorieHandle.Completed += OnAccessorieLoaded;

        skillHandle = Addressables.LoadAssetAsync<TextAsset>(skillJsonKey);
        skillHandleValid = true;
        skillHandle.Completed += OnSkillLoaded;

        Debug.Log("[ShopSummonSystem] Addressables load requested.");
    }

    private void OnWeaponLoaded(AsyncOperationHandle<TextAsset> h)
    {
        if (h.Status != AsyncOperationStatus.Succeeded || h.Result == null)
        {
            Debug.LogError("[ShopSummonSystem] WeaponList json not found(key): " + weaponJsonKey);
            return;
        }

        ApplyEquipJsonToList(h.Result.text, weaponItems, "Weapon");
    }

    private void OnAccessorieLoaded(AsyncOperationHandle<TextAsset> h)
    {
        if (h.Status != AsyncOperationStatus.Succeeded || h.Result == null)
        {
            Debug.LogError("[ShopSummonSystem] AccessorieList json not found(key): " + accessorieJsonKey);
            return;
        }

        ApplyEquipJsonToList(h.Result.text, accessoryItems, "Accessorie");
    }

    private void OnSkillLoaded(AsyncOperationHandle<TextAsset> h)
    {
        if (h.Status != AsyncOperationStatus.Succeeded || h.Result == null)
        {
            Debug.LogError("[ShopSummonSystem] SkillList json not found(key): " + skillJsonKey);
            return;
        }

        ApplySkillJsonToList(h.Result.text, skillItems);
    }

    private void ApplyEquipJsonToList(string jsonText, List<ShopItemData> target, string tag)
    {
        if (string.IsNullOrEmpty(jsonText)) return;
        if (target == null) return;

        EquipListRoot root = JsonUtility.FromJson<EquipListRoot>(jsonText);
        if (root == null || root.equipList == null)
        {
            Debug.LogError("[ShopSummonSystem] Json parse failed: " + tag);
            return;
        }

        target.Clear();

        int count = root.equipList.Length;
        if (count > 33)
        {
            Debug.LogWarning("[ShopSummonSystem] Json item count over 33. Clamped.");
            count = 33;
        }

        for (int i = 0; i < count; i++)
        {
            EquipJson e = root.equipList[i];
            if (e == null) continue;

            string gradeName = ConvertGradeToKorean(e.grade);
            string gradeStepText = e.gradeStep.ToString();

            ShopItemData item = new ShopItemData();
            item.Set(
                e.id,
                gradeName,         // 위쪽 등급명
                e.spriteName,      // 아이콘
                gradeStepText,     // 아래쪽 숫자 등급
                true
            );

            target.Add(item);
        }

        Debug.Log("[ShopSummonSystem] Json applied: " + tag);
    }

    private void ApplySkillJsonToList(string jsonText, List<ShopItemData> target)
    {
        if (string.IsNullOrEmpty(jsonText)) return;
        if (target == null) return;

        SkillListRoot root = JsonUtility.FromJson<SkillListRoot>(jsonText);
        if (root == null || root.skillList == null)
        {
            Debug.LogError("[ShopSummonSystem] Skill json parse failed.");
            return;
        }

        target.Clear();

        int count = root.skillList.Length;
        if (count > 33)
        {
            Debug.LogWarning("[ShopSummonSystem] Skill item count over 33. Clamped.");
            count = 33;
        }

        for (int i = 0; i < count; i++)
        {
            SkillJson s = root.skillList[i];
            if (s == null) continue;

            ShopItemData item = new ShopItemData();
            item.Set(
                s.id,
                s.name,         // 스킬 이름
                s.spriteName,   // 아이콘
                s.element,      // 필요 시 속성
                true
            );

            target.Add(item);
        }

        Debug.Log("[ShopSummonSystem] Skill json applied.");
    }

    private string ConvertGradeToKorean(string grade)
    {
        if (string.IsNullOrEmpty(grade))
            return "일반";

        switch (grade)
        {
            case "Common":
            case "Normal":
            case "일반":
                return "일반";

            case "Uncommon":
            case "High":
            case "고급":
                return "고급";

            case "Rare":
            case "레어":
                return "레어";

            case "Epic":
            case "Hero":
            case "영웅":
                return "영웅";

            case "Legend":
            case "Legendary":
            case "전설":
                return "전설";

            case "Myth":
            case "Mythic":
            case "신화":
                return "신화";
        }

        return grade;
    }
}