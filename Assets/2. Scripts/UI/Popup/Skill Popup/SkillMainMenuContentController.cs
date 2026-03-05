using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlayerLegend.Skill;

/*
[승문]
SkillMainMenuContentController
-미리 만들어둔 SkillBar/SkillBundle 구조를 자동 수집
-DataManager.skills 기반으로 등급별로 Bar에 분배해서 표시
-Instantiate 없이 SetActive+Bind만 수행
-슬롯 클릭 시: 스킬 팝업 열기(속성은 element로 전달) 또는 외부 훅 처리
*/
public class SkillMainMenuContentController : MonoBehaviour
{
    [SerializeField] private Transform tabPanelRoot;//Skill Tab Panel
    [SerializeField] private string iconBasePath = "Icons";//Resources/Icons/...
    [SerializeField] private bool cacheSprites = true;//스프라이트 캐시
    [SerializeField] private bool openPopupOnSlotClick = true;//클릭 시 팝업 오픈
    [SerializeField] private PopupId popupId = PopupId.Skill;//열 팝업
    [SerializeField] private bool usePlusButton = true;//플러스 버튼 사용

    private readonly List<SkillBarView> bars = new List<SkillBarView>(16);
    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(128);
    private readonly List<SkillData> working = new List<SkillData>(256);

    private void Awake()
    {
        //루트 자동 연결
        if (tabPanelRoot == null)
        {
            tabPanelRoot = transform;
        }

        //Bar 수집
        CollectBars();

        //초기 갱신
        Refresh();

        Debug.Log("[SkillMainMenuContentController] Awake completed.");
    }

    //외부에서 갱신 호출
    public void Refresh()
    {
        if (DataManager.skills == null)
        {
            Debug.LogError("[SkillMainMenuContentController] DataManager.skills is null.");
            HideAll();
            return;
        }

        List<SkillData> all = DataManager.skills.GetAll();
        if (all == null || all.Count <= 0)
        {
            Debug.LogWarning("[SkillMainMenuContentController] Skill list empty.");
            HideAll();
            return;
        }

        working.Clear();
        for (int i = 0; i < all.Count; i++)
        {
            if (all[i] == null) continue;
            working.Add(all[i]);
        }

        //등급 기준 정렬
        working.Sort(CompareByGradeThenId);

        //Bar에 적용
        ApplyToBars(working);

        Debug.Log("[SkillMainMenuContentController] Refresh applied.");
    }

    //Bar 자동 수집
    private void CollectBars()
    {
        bars.Clear();

        SkillBarView[] found = tabPanelRoot.GetComponentsInChildren<SkillBarView>(true);
        for (int i = 0; i < found.Length; i++)
        {
            if (found[i] == null) continue;
            bars.Add(found[i]);
        }

        if (bars.Count <= 0)
        {
            Debug.LogError("[SkillMainMenuContentController] No SkillBarView found under tabPanelRoot.");
        }
    }

    //전체 숨김
    private void HideAll()
    {
        for (int b = 0; b < bars.Count; b++)
        {
            if (bars[b] == null) continue;

            IReadOnlyList<SkillBundleView> bundles = bars[b].GetBundles();
            for (int i = 0; i < bundles.Count; i++)
            {
                if (bundles[i] == null) continue;
                bundles[i].SetActive(false);
            }
        }
    }

    //Bar에 분배해서 바인딩
    private void ApplyToBars(List<SkillData> sorted)
    {
        int cursor = 0;

        for (int barIndex = 0; barIndex < bars.Count; barIndex++)
        {
            SkillBarView bar = bars[barIndex];
            if (bar == null) continue;

            IReadOnlyList<SkillBundleView> bundles = bar.GetBundles();
            if (bundles == null || bundles.Count <= 0) continue;

            //이번 Bar가 담당할 grade 결정(SkillGrade 유지)
            SkillGrade targetGrade = default;
            bool hasTarget = false;

            if (cursor < sorted.Count && sorted[cursor] != null)
            {
                targetGrade = sorted[cursor].grade;
                hasTarget = true;
            }

            //더 이상 채울 데이터가 없으면 이 Bar는 전부 숨김
            if (hasTarget == false)
            {
                for (int i = 0; i < bundles.Count; i++)
                {
                    if (bundles[i] == null) continue;
                    bundles[i].SetActive(false);
                }
                continue;
            }

            //Bundle에 같은 grade만 채움
            for (int i = 0; i < bundles.Count; i++)
            {
                SkillBundleView bundle = bundles[i];
                if (bundle == null) continue;

                if (cursor >= sorted.Count)
                {
                    bundle.SetActive(false);
                    continue;
                }

                SkillData data = sorted[cursor];
                if (data == null)
                {
                    bundle.SetActive(false);
                    cursor++;
                    continue;
                }

                //grade가 달라지면 이 Bar는 끝
                if (data.grade.CompareTo(targetGrade) != 0)
                {
                    bundle.SetActive(false);
                    continue;
                }

                Sprite icon = ResolveSprite(data.spriteName);

                //슬라이더값은 팀 로직으로 교체
                float slider01 = 0f;

                bundle.SetActive(true);
                bundle.BindData(
                    data,
                    icon,
                    slider01,
                    HandleSlotClick,
                    usePlusButton ? HandlePlusClick : null
                );

                cursor++;
            }
        }

        //남은 데이터가 더 있으면 Bar가 부족한 상태
        if (cursor < sorted.Count)
        {
            Debug.LogWarning("[SkillMainMenuContentController] Not enough bars/bundles for all skills. Remaining=" + (sorted.Count - cursor));
        }
    }

    //정렬 비교(grade→id)
    private int CompareByGradeThenId(SkillData a, SkillData b)
    {
        if (a == null && b == null) return 0;
        if (a == null) return 1;
        if (b == null) return -1;

        int g = a.grade.CompareTo(b.grade);
        if (g != 0) return g;

        string ida = a.id != null ? a.id : "";
        string idb = b.id != null ? b.id : "";
        return string.CompareOrdinal(ida, idb);
    }

    //슬롯 클릭 처리
    private void HandleSlotClick(SkillData data)
    {
        if (data == null) return;

        Debug.Log("[SkillMainMenuContentController] Slot clicked: " + data.id);

        if (openPopupOnSlotClick == false) return;
        if (PopupManager.Instance == null) return;

        SkillAttribute attr = ConvertElementToAttribute(data.element);
        PopupManager.Instance.Open(popupId, attr);
    }

    //플러스 클릭 처리
    private void HandlePlusClick(SkillData data)
    {
        if (data == null) return;

        Debug.Log("[SkillMainMenuContentController] Plus clicked: " + data.id);

        //TODO:팀 스킬 강화/합성/슬롯해금 등 연결 지점
    }

    //스프라이트 로드+캐시
    private Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            return null;
        }

        if (cacheSprites && spriteCache.TryGetValue(spriteName, out Sprite cached))
        {
            return cached;
        }

        string path = iconBasePath + "/" + spriteName;
        Sprite loaded = Resources.Load<Sprite>(path);

        if (cacheSprites)
        {
            spriteCache[spriteName] = loaded;
        }

        return loaded;
    }

    //element→attribute 변환
    private SkillAttribute ConvertElementToAttribute(SkillElement element)
    {
        if (element == SkillElement.Fire) return SkillAttribute.Fire;
        if (element == SkillElement.Water) return SkillAttribute.Water;
        if (element == SkillElement.Wind) return SkillAttribute.Wind;
        if (element == SkillElement.Earth) return SkillAttribute.Earth;

        return SkillAttribute.Fire;
    }
}