using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
[승문]
ShopPopupContentController
- 상점/소환 팝업 내부 UI 컨트롤러
- Summon Panel 아래 미리 배치된 슬롯을 자동 수집해서 사용
- SetTab으로 현재 탭을 갱신하고 슬롯 UI를 다시 바인딩
- ShopSummonSystem의 OnSummonResult를 구독해서
  1회 / 11회 / 33회 결과 개수만큼만 슬롯을 표시
- Weapon / Accessory는 인스펙터 Sprite 배열 사용
- Skill은 Resources/Skill/skillicon 에서 자동 로드
- Accessory는 JSON spriteName(acce_00)를 실제 Sprite 이름(Acc_000)으로 변환해서 찾음
*/
public class ShopPopupContentController : MonoBehaviour
{
    [Header("Auto Connect")]
    [SerializeField] private Transform slotRoot; // Summon Panel(슬롯들의 부모)
    [SerializeField] private GridLayoutGroup grid; // Summon Panel의 GridLayoutGroup(선택)

    [Header("Provider")]
    [SerializeField] private MonoBehaviour providerBehaviour; // IShopItemProvider 구현체

    [Header("Sprite Database (Inspector)")]
    [SerializeField] private Sprite[] weaponSprites;     // 무기 스프라이트
    [SerializeField] private Sprite[] accessorySprites;  // 악세 스프라이트

    [Header("Skill Sprite Auto Load")]
    [Tooltip("Resources 기준 경로")]
    [SerializeField] private string skillSpriteResourcesPath = "Skill/skillicon";

    [Header("Slot Limit")]
    [SerializeField] private int maxSlotCount = 33; // 1/11/33 대응

    private readonly List<ShopItemSlotView> slots = new List<ShopItemSlotView>(33);
    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(256);
    private readonly Dictionary<string, Sprite> skillSpriteMap = new Dictionary<string, Sprite>(128);
    private readonly List<ShopItemInfo> summonResults = new List<ShopItemInfo>(33);

    private IShopItemProvider provider;
    private ShopSummonSystem summonSystem;
    private ShopTab currentTab;
    private bool showingSummonResults;

    private void Awake()
    {
        if (slotRoot == null)
        {
            slotRoot = transform;
        }

        if (grid == null && slotRoot != null)
        {
            grid = slotRoot.GetComponent<GridLayoutGroup>();
        }

        CollectSlots();
        ResolveProvider();
        LoadSkillSprites();

        if (summonSystem != null)
        {
            summonSystem.OnSummonResult += HandleSummonResult;
        }

        currentTab = ShopTab.Weapon;
        showingSummonResults = false;

        ApplySlots(0);

        Debug.Log("[ShopPopupContentController] Awake completed.");
    }

    private void OnDestroy()
    {
        if (summonSystem != null)
        {
            summonSystem.OnSummonResult -= HandleSummonResult;
        }
    }

    /// <summary>
    /// 팝업에서 탭 전달받아 UI 갱신
    /// </summary>
    public void SetTab(ShopTab tab)
    {
        currentTab = tab;
        showingSummonResults = false;

        if (summonSystem != null)
        {
            summonSystem.SetCurrentTab(tab);
        }

        Rebuild();

        Debug.Log("[ShopPopupContentController] SetTab applied.");
    }

    /// <summary>
    /// 현재 탭 기준으로 기본 목록 표시
    /// </summary>
    private void Rebuild()
    {
        if (provider == null)
        {
            ApplySlots(0);
            return;
        }

        int count = provider.GetCount(currentTab);
        if (count > maxSlotCount)
        {
            Debug.LogWarning("[ShopPopupContentController] Count over maxSlotCount. Clamped.");
            count = maxSlotCount;
        }

        ApplySlots(count);
    }

    /// <summary>
    /// 소환 결과 이벤트 수신
    /// </summary>
    private void HandleSummonResult(ShopTab tab, IReadOnlyList<ShopItemInfo> results)
    {
        currentTab = tab;
        showingSummonResults = true;

        if (summonSystem != null)
        {
            summonSystem.SetCurrentTab(tab);
        }

        summonResults.Clear();

        if (results != null)
        {
            int count = results.Count;
            if (count > maxSlotCount)
            {
                count = maxSlotCount;
            }

            for (int i = 0; i < count; i++)
            {
                summonResults.Add(results[i]);
            }
        }

        ApplySummonResults();

        Debug.Log("[ShopPopupContentController] Summon results applied. Count=" + summonResults.Count);
    }

    /// <summary>
    /// 기본 목록 표시
    /// </summary>
    private void ApplySlots(int count)
    {
        int slotCount = slots.Count;
        if (slotCount <= 0)
        {
            Debug.LogError("[ShopPopupContentController] Slots empty.");
            return;
        }

        if (count > slotCount)
        {
            count = slotCount;
        }

        for (int i = 0; i < slotCount; i++)
        {
            ShopItemSlotView slot = slots[i];
            if (slot == null)
            {
                continue;
            }

            bool active = i < count;
            slot.gameObject.SetActive(active);

            if (!active)
            {
                slot.Clear();
                continue;
            }

            slot.ApplyTabLayout(currentTab);
            BindFromProvider(slot, i);
        }
    }

    /// <summary>
    /// 소환 결과 표시
    /// </summary>
    private void ApplySummonResults()
    {
        int slotCount = slots.Count;
        if (slotCount <= 0)
        {
            Debug.LogError("[ShopPopupContentController] Slots empty.");
            return;
        }

        int count = summonResults.Count;
        if (count > slotCount)
        {
            count = slotCount;
        }

        for (int i = 0; i < slotCount; i++)
        {
            ShopItemSlotView slot = slots[i];
            if (slot == null)
            {
                continue;
            }

            bool active = i < count;
            slot.gameObject.SetActive(active);

            if (!active)
            {
                slot.Clear();
                continue;
            }

            slot.ApplyTabLayout(currentTab);

            ShopItemInfo info = summonResults[i];
            Sprite icon = ResolveSprite(info.SpriteName);

            string primary = info.Name;
            string secondary = info.PriceText;

            slot.Bind(icon, primary, secondary, info.CanBuy, null);
        }
    }

    /// <summary>
    /// Provider에서 데이터 받아 슬롯 바인딩
    /// </summary>
    private void BindFromProvider(ShopItemSlotView slot, int index)
    {
        if (provider == null || slot == null)
        {
            return;
        }

        ShopItemInfo info = provider.GetItem(currentTab, index);
        Sprite icon = ResolveSprite(info.SpriteName);

        string primary = info.Name;
        string secondary = info.PriceText;

        slot.Bind(icon, primary, secondary, info.CanBuy, null);
    }

    /// <summary>
    /// Summon Panel 아래 자식 슬롯 자동 수집
    /// </summary>
    private void CollectSlots()
    {
        slots.Clear();

        if (slotRoot == null)
        {
            Debug.LogError("[ShopPopupContentController] slotRoot is null.");
            return;
        }

        int childCount = slotRoot.childCount;
        for (int i = 0; i < childCount; i++)
        {
            if (slots.Count >= maxSlotCount)
            {
                break;
            }

            Transform child = slotRoot.GetChild(i);
            if (child == null)
            {
                continue;
            }

            ShopItemSlotView view = child.GetComponent<ShopItemSlotView>();
            if (view == null)
            {
                continue;
            }

            view.SetIndex(slots.Count);
            view.gameObject.SetActive(false);
            slots.Add(view);
        }

        if (slots.Count <= 0)
        {
            Debug.LogError("[ShopPopupContentController] No ShopItemSlotView found under slotRoot.");
        }
        else
        {
            Debug.Log("[ShopPopupContentController] Slots collected. Count=" + slots.Count);
        }
    }

    /// <summary>
    /// 스킬 스프라이트 자동 로드
    /// Resources/Skill/skillicon 아래에 있는 Sprite 전부 수집
    /// </summary>
    private void LoadSkillSprites()
    {
        skillSpriteMap.Clear();

        Sprite[] sprites = Resources.LoadAll<Sprite>(skillSpriteResourcesPath);

        if (sprites == null || sprites.Length <= 0)
        {
            Debug.LogWarning("[ShopPopupContentController] No skill sprites found at: " + skillSpriteResourcesPath);
            return;
        }

        for (int i = 0; i < sprites.Length; i++)
        {
            Sprite sp = sprites[i];
            if (sp == null)
            {
                continue;
            }

            if (!skillSpriteMap.ContainsKey(sp.name))
            {
                skillSpriteMap.Add(sp.name, sp);
            }
        }

        Debug.Log("[ShopPopupContentController] Skill sprites loaded: " + skillSpriteMap.Count);
    }

    /// <summary>
    /// JSON의 악세 spriteName을 실제 Sprite 이름으로 변환
    /// 예: acce_00 -> Acc_000
    /// </summary>
    private string ConvertAccessorySpriteName(string jsonSpriteName)
    {
        if (string.IsNullOrEmpty(jsonSpriteName))
        {
            return jsonSpriteName;
        }

        if (jsonSpriteName.StartsWith("acce_"))
        {
            string numberPart = jsonSpriteName.Substring(5);

            if (int.TryParse(numberPart, out int number))
            {
                return "Acc_" + number.ToString("000");
            }
        }

        return jsonSpriteName;
    }

    /// <summary>
    /// spriteName으로 스프라이트 찾기
    /// - Skill: Resources 자동 로드 맵 사용
    /// - Weapon/Accessory: 인스펙터 배열 사용
    /// - Accessory는 JSON 이름을 실제 Sprite 이름으로 보정
    /// </summary>
    private Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            return null;
        }

        string cacheKey = currentTab + ":" + spriteName;

        if (spriteCache.TryGetValue(cacheKey, out Sprite cached))
        {
            return cached;
        }

        Sprite found = null;

        if (currentTab == ShopTab.Skill)
        {
            if (skillSpriteMap.TryGetValue(spriteName, out Sprite skillSprite))
            {
                found = skillSprite;
            }
        }
        else
        {
            Sprite[] source = GetCurrentSpriteArray();
            if (source != null)
            {
                string targetName = spriteName;

                // 악세는 JSON 이름 -> 실제 Sprite 이름으로 변환
                if (currentTab == ShopTab.Accessory)
                {
                    targetName = ConvertAccessorySpriteName(spriteName);
                }

                for (int i = 0; i < source.Length; i++)
                {
                    Sprite sp = source[i];
                    if (sp == null)
                    {
                        continue;
                    }

                    if (sp.name == targetName)
                    {
                        found = sp;
                        break;
                    }
                }
            }
        }

        if (found == null)
        {
            Debug.LogWarning("[ShopPopupContentController] Sprite not found. tab=" + currentTab + ", spriteName=" + spriteName);
            return null;
        }

        spriteCache.Add(cacheKey, found);
        return found;
    }

    /// <summary>
    /// 현재 탭에 맞는 무기/악세 Sprite 배열 반환
    /// </summary>
    private Sprite[] GetCurrentSpriteArray()
    {
        switch (currentTab)
        {
            case ShopTab.Weapon:
                return weaponSprites;

            case ShopTab.Accessory:
                return accessorySprites;
        }

        return null;
    }

    /// <summary>
    /// Provider 확정 + SummonSystem 캐싱
    /// </summary>
    private void ResolveProvider()
    {
        provider = null;
        summonSystem = null;

        if (providerBehaviour != null)
        {
            if (providerBehaviour is IShopItemProvider p0)
            {
                provider = p0;
                summonSystem = providerBehaviour as ShopSummonSystem;
                Debug.Log("[ShopPopupContentController] Provider assigned by inspector.");
                return;
            }

            Debug.LogError("[ShopPopupContentController] providerBehaviour is not IShopItemProvider.");
        }

        MonoBehaviour[] all = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
        for (int i = 0; i < all.Length; i++)
        {
            MonoBehaviour mb = all[i];
            if (mb == null)
            {
                continue;
            }

            if (!mb.gameObject.scene.IsValid())
            {
                continue;
            }

            if (mb is IShopItemProvider p)
            {
                provider = p;
                summonSystem = mb as ShopSummonSystem;
                Debug.Log("[ShopPopupContentController] Provider found by search.");
                return;
            }
        }

        Debug.LogError("[ShopPopupContentController] IShopItemProvider not found.");
    }
}