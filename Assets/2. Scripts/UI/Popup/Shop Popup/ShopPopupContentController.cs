using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
[승문]
ShopPopupContentController
-상점/소환 팝업 내부 UI 컨트롤러
-Summon Panel 아래 미리 배치된 슬롯(최대31개)을 자동 수집해서 사용
-SetTab으로 현재 탭을 갱신하고 슬롯 UI를 다시 바인딩
-Provider가 ShopSummonSystem이면 현재탭도 같이 전달해서 하단 뽑기버튼이 탭을 몰라도 동작하게 함
*/
public class ShopPopupContentController : MonoBehaviour
{
    [Header("Auto Connect")]
    [SerializeField] private Transform slotRoot;//Summon Panel(슬롯들의 부모)
    [SerializeField] private GridLayoutGroup grid;//Summon Panel의 GridLayoutGroup(선택)

    [Header("Provider")]
    [SerializeField] private MonoBehaviour providerBehaviour;//IShopItemProvider구현체(선택)

    [Header("Sprite")]
    [SerializeField] private string spriteResourcesBasePath = "Icons";//Resources 경로

    private readonly List<ShopItemSlotView> slots = new List<ShopItemSlotView>(31);
    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(128);

    private IShopItemProvider provider;
    private ShopSummonSystem summonSystem;
    private ShopTab currentTab;
    private int lastCount;

    private void Awake()
    {
        if (slotRoot == null)
        {
            slotRoot = transform;
        }

        if (grid == null)
        {
            grid = slotRoot.GetComponent<GridLayoutGroup>();
        }

        CollectSlots();

        ResolveProvider();

        currentTab = ShopTab.Weapon;
        lastCount = 0;

        ApplySlots(0);

        Debug.Log("[ShopPopupContentController] Awake completed.");
    }

    //팝업에서 탭 전달받아 UI 갱신
    public void SetTab(ShopTab tab)
    {
        currentTab = tab;

        //하단 뽑기 버튼이 탭을 몰라도 되게 시스템에 현재탭 전달
        if (summonSystem != null)
        {
            summonSystem.SetCurrentTab(tab);
        }

        Rebuild();

        Debug.Log("[ShopPopupContentController] SetTab applied.");
    }

    //현재 탭 데이터로 슬롯 갱신
    private void Rebuild()
    {
        if (provider == null)
        {
            ApplySlots(0);
            return;
        }

        int count = provider.GetCount(currentTab);
        if (count > 31)
        {
            Debug.LogWarning("[ShopPopupContentController] Count over 31. Clamped.");
            count = 31;
        }

        lastCount = count;
        ApplySlots(lastCount);
    }

    //슬롯 SetActive+레이아웃+바인딩
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

            if (active == false)
            {
                continue;
            }

            slot.ApplyTabLayout(currentTab);
            BindFromProvider(slot, i);
        }
    }

    //provider 기반 바인딩
    private void BindFromProvider(ShopItemSlotView slot, int index)
    {
        if (provider == null)
        {
            return;
        }

        ShopItemInfo info = provider.GetItem(currentTab, index);
        Sprite icon = ResolveSprite(info.SpriteName);

        string primary = info.Name;
        string secondary = info.PriceText;

        //슬롯 클릭 구매는 지금 구조에서 안 쓰면 null로 둬도 됨
        slot.Bind(icon, primary, secondary, info.CanBuy, null);
    }

    //슬롯 자동 수집(자식 순서대로,최대31개)
    private void CollectSlots()
    {
        slots.Clear();

        int childCount = slotRoot.childCount;
        for (int i = 0; i < childCount; i++)
        {
            if (slots.Count >= 31)
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
            Debug.Log("[ShopPopupContentController] Slots collected.");
        }
    }

    //스프라이트 로드+캐시
    private Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            return null;
        }

        if (spriteCache.TryGetValue(spriteName, out Sprite cached))
        {
            return cached;
        }

        string path = spriteResourcesBasePath + "/" + spriteName;
        Sprite loaded = Resources.Load<Sprite>(path);
        spriteCache.Add(spriteName, loaded);

        return loaded;
    }

    //Provider 확정+SummonSystem 캐싱
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

        //씬에서 자동 탐색(비활성 포함,씬 오브젝트만)
        MonoBehaviour[] all = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
        for (int i = 0; i < all.Length; i++)
        {
            MonoBehaviour mb = all[i];
            if (mb == null)
            {
                continue;
            }

            if (mb.gameObject.scene.IsValid() == false)
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