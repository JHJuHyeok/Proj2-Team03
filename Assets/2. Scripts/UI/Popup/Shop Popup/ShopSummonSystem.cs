using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

/*
[승문]
ShopSummonSystem
-IShopItemProvider 구현체
-Addressables Address(키)로 WeaponList/AccessorieList를 Awake에서 자동 로드
-SetCurrentTab으로 현재 탭을 저장하고 TryBuyBatch(count)는 현재 탭 기준으로 소환
-간단 버전:재화/확률/중복처리 없이 랜덤 결과만 생성
*/
public class ShopSummonSystem : MonoBehaviour, IShopItemProvider
{
    [System.Serializable]
    private class ShopItemData
    {
        [SerializeField] private string id;//아이템ID
        [SerializeField] private string name;//표시이름
        [SerializeField] private string spriteName;//Resources스프라이트명
        [SerializeField] private string priceText;//표시텍스트(등급/가격 등)
        [SerializeField] private bool canBuy = true;//구매가능여부(임시)

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

    [Header("Auto Load(Json by Addressables Key)")]
    [SerializeField] private bool autoLoadOnAwake = true;//Awake에서 자동 로드
    [Tooltip("Addressables Address 예: Json/Equip/WeaponList")]
    [SerializeField] private string weaponJsonKey = "Json/Equip/WeaponList";//Addressables Address(키)
    [Tooltip("Addressables Address 예: Json/Equip/AccessorieList")]
    [SerializeField] private string accessorieJsonKey = "Json/Equip/AccessorieList";//Addressables Address(키)

    [Header("Tab Items(Runtime)")]
    [SerializeField] private List<ShopItemData> weaponItems = new List<ShopItemData>(31);//무기
    [SerializeField] private List<ShopItemData> accessoryItems = new List<ShopItemData>(31);//악세
    [SerializeField] private List<ShopItemData> skillItems = new List<ShopItemData>(31);//스킬(추후)

    private readonly List<ShopItemInfo> resultCache = new List<ShopItemInfo>(64);
    private ShopTab currentTab;

    private AsyncOperationHandle<TextAsset> weaponHandle;
    private AsyncOperationHandle<TextAsset> accessorieHandle;
    private bool weaponHandleValid;
    private bool accessorieHandleValid;

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
        //Addressables 핸들 해제
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
    }

    //현재탭 설정(그리드 갱신 시점에 UI가 호출)
    public void SetCurrentTab(ShopTab tab)
    {
        currentTab = tab;
    }

    //현재탭 조회(디버그/확인용)
    public ShopTab GetCurrentTab()
    {
        return currentTab;
    }

    //탭의 슬롯 개수
    public int GetCount(ShopTab tab)
    {
        List<ShopItemData> list = GetList(tab);
        if (list == null) return 0;

        int count = list.Count;
        if (count > 31)
        {
            count = 31;
        }

        return count;
    }

    //탭의 슬롯 데이터
    public ShopItemInfo GetItem(ShopTab tab, int index)
    {
        List<ShopItemData> list = GetList(tab);
        if (list == null) return default;
        if (index < 0 || index >= list.Count) return default;

        ShopItemData data = list[index];

        ShopItemInfo info = new ShopItemInfo();
        info.Id = data != null ? data.Id : "";
        info.Name = data != null ? data.Name : "";
        info.SpriteName = data != null ? data.SpriteName : "";
        info.PriceText = data != null ? data.PriceText : "";
        info.CanBuy = data != null && data.CanBuy;

        return info;
    }

    //슬롯 클릭 구매(현재는 1회 소환으로 처리)
    public bool TryBuy(ShopTab tab, int index)
    {
        SetCurrentTab(tab);
        bool ok = TryBuyBatch(1);
        return ok;
    }

    //인터페이스용(탭+횟수)
    public bool TryBuyBatch(ShopTab tab, int count)
    {
        SetCurrentTab(tab);
        bool ok = TryBuyBatch(count);
        return ok;
    }

    //하단 버튼용(횟수만)
    public bool TryBuyBatch(int count)
    {
        //주의:너가 1/11/31회라 했으면 여기 33을 31로 바꾸면 됨
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

        if (OnSummonResult != null)
        {
            OnSummonResult(currentTab, resultCache);
        }

        return true;
    }

    //탭별 리스트
    private List<ShopItemData> GetList(ShopTab tab)
    {
        if (tab == ShopTab.Weapon)
        {
            return weaponItems;
        }

        if (tab == ShopTab.Accessory)
        {
            return accessoryItems;
        }

        if (tab == ShopTab.Skill)
        {
            return skillItems;
        }

        return weaponItems;
    }

    //랜덤 1개
    private ShopItemInfo PickRandom(List<ShopItemData> list)
    {
        int idx = Random.Range(0, list.Count);
        ShopItemData data = list[idx];

        ShopItemInfo info = new ShopItemInfo();
        info.Id = data != null ? data.Id : "";
        info.Name = data != null ? data.Name : "";
        info.SpriteName = data != null ? data.SpriteName : "";
        info.PriceText = data != null ? data.PriceText : "";
        info.CanBuy = data != null && data.CanBuy;

        return info;
    }

    //Addressables 키로 JSON 자동 로드
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

        //무기 로드
        weaponHandle = Addressables.LoadAssetAsync<TextAsset>(weaponJsonKey);
        weaponHandleValid = true;
        weaponHandle.Completed += OnWeaponLoaded;

        //악세 로드
        accessorieHandle = Addressables.LoadAssetAsync<TextAsset>(accessorieJsonKey);
        accessorieHandleValid = true;
        accessorieHandle.Completed += OnAccessorieLoaded;

        Debug.Log("[ShopSummonSystem] Addressables load requested.");
    }

    //무기 JSON 로드 완료
    private void OnWeaponLoaded(AsyncOperationHandle<TextAsset> h)
    {
        if (h.Status != AsyncOperationStatus.Succeeded || h.Result == null)
        {
            Debug.LogError("[ShopSummonSystem] WeaponList json not found(key): " + weaponJsonKey);
            return;
        }

        ApplyEquipJsonToList(h.Result.text, weaponItems, "Weapon");
    }

    //악세 JSON 로드 완료
    private void OnAccessorieLoaded(AsyncOperationHandle<TextAsset> h)
    {
        if (h.Status != AsyncOperationStatus.Succeeded || h.Result == null)
        {
            Debug.LogError("[ShopSummonSystem] AccessorieList json not found(key): " + accessorieJsonKey);
            return;
        }

        ApplyEquipJsonToList(h.Result.text, accessoryItems, "Accessorie");
    }

    //Equip JSON text -> ShopItemData 리스트 변환
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
        if (count > 31)
        {
            Debug.LogWarning("[ShopSummonSystem] Json item count over 31. Clamped.");
            count = 31;
        }

        for (int i = 0; i < count; i++)
        {
            EquipJson e = root.equipList[i];
            if (e == null)
            {
                continue;
            }

            string gradeName = ConvertGradeToKorean(e.grade);
            string gradeStepText = e.gradeStep.ToString();

            ShopItemData item = new ShopItemData();
            item.Set(e.id, gradeName, e.spriteName, gradeStepText, true);
            target.Add(item);
        }

        Debug.Log("[ShopSummonSystem] Json applied: " + tag);
    }

    //등급 스트링을 UI용 한글로 변환
    private string ConvertGradeToKorean(string grade)
    {
        if (string.IsNullOrEmpty(grade))
        {
            return "일반";
        }

        if (grade == "Common") return "일반";
        if (grade == "Uncommon") return "고급";
        if (grade == "Rare") return "레어";
        if (grade == "Epic") return "영웅";
        if (grade == "Legendary") return "레전";

        return grade;
    }
}