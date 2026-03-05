using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using SlayerLegend.Skill;

/*
[승문]
SkillPopupContentController
-스킬 팝업 내부 UI만 담당(리스트,선택,AUTO,강화)
-리스트는 풀링 재사용으로 GC 최소화
-상세 UI는 SkillPopupJsonBinder에 위임(선택된 id만 전달)
-슬라이더는 상태가 바뀔 때만 이벤트/로그 발생
*/
public class SkillPopupContentController : MonoBehaviour
{
    [SerializeField] private SkillController skillController;//팀 로직 연결용(강화 등)
    [SerializeField] private Slider autoSlider;//AUTO 슬라이더
    [SerializeField, Range(0.01f, 0.99f)] private float autoOnThreshold = 0.5f;//0.5 이상이면 ON
    [SerializeField] private Transform listRoot;//ScrollRect Content
    [SerializeField] private SkillItemCell cellPrefab;//리스트 셀 프리팹
    [SerializeField, Min(1)] private int poolWarmCount = 24;//초기 풀 크기
    [SerializeField] private SkillPopupJsonBinder jsonBinder;//상세 바인더
    [SerializeField] private Button upgradeButton;//강화 버튼

    [SerializeField] private string iconBasePath = "Icons";//Resources/Icons/...
    [SerializeField] private bool cacheSprites = true;//스프라이트 캐시 사용

    private readonly List<SkillItemCell> cellPool = new List<SkillItemCell>(64);
    private readonly List<SkillData> filtered = new List<SkillData>(128);
    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(128);

    private SkillAttribute currentAttribute;
    private SkillData selectedSkill;
    private bool lastAutoOn;

    private void Awake()
    {
        //필수참조 자동 캐싱
        if (skillController == null)
        {
            skillController = FindFirstObjectByType<SkillController>();
        }

        //바인더 자동 탐색
        if (jsonBinder == null)
        {
            jsonBinder = GetComponentInChildren<SkillPopupJsonBinder>(true);
        }

        //버튼 이벤트 연결
        if (upgradeButton != null)
        {
            upgradeButton.onClick.AddListener(OnClickUpgrade);
        }

        //슬라이더 이벤트 연결
        if (autoSlider != null)
        {
            autoSlider.onValueChanged.AddListener(OnAutoSliderChanged);
            lastAutoOn = autoSlider.value >= autoOnThreshold;
        }

        //풀 워밍
        WarmPool();

        //초기 비선택
        ApplySelection(null);

        Debug.Log("[SkillPopupContentController] Awake completed.");
    }

    private void OnDestroy()
    {
        //이벤트 해제
        if (autoSlider != null)
        {
            autoSlider.onValueChanged.RemoveListener(OnAutoSliderChanged);
        }
    }

    //팝업에서 속성을 전달받아 리스트를 재구성
    public void SetAttribute(SkillAttribute attribute)
    {
        currentAttribute = attribute;
        selectedSkill = null;

        RebuildFiltered();
        RebuildList();
        ApplySelection(null);

        Debug.Log("[SkillPopupContentController] SetAttribute applied.");
    }

    //AUTO 슬라이더 변경 처리(상태 변할 때만)
    private void OnAutoSliderChanged(float value)
    {
        bool on = value >= autoOnThreshold;

        if (on == lastAutoOn)
        {
            return;
        }

        lastAutoOn = on;

        Debug.Log(on ? "[SkillPopupContentController] Auto ON." : "[SkillPopupContentController] Auto OFF.");

        //TODO:팀 자동 시전 시스템에 on 전달
    }

    //속성에 맞는 스킬 목록 필터링
    private void RebuildFiltered()
    {
        filtered.Clear();

        if (DataManager.skills == null)
        {
            Debug.LogError("[SkillPopupContentController] DataManager.skills is null.");
            return;
        }

        SkillElement element = ConvertAttributeToElement(currentAttribute);
        List<SkillData> all = DataManager.skills.GetAll();

        for (int i = 0; i < all.Count; i++)
        {
            SkillData s = all[i];
            if (s == null)
            {
                continue;
            }

            if (s.element == element)
            {
                filtered.Add(s);
            }
        }
    }

    //리스트 풀링 생성/바인딩
    private void RebuildList()
    {
        EnsurePool(filtered.Count);

        for (int i = 0; i < cellPool.Count; i++)
        {
            bool active = i < filtered.Count;
            SkillItemCell cell = cellPool[i];

            cell.gameObject.SetActive(active);

            if (active == false)
            {
                continue;
            }

            SkillData data = filtered[i];
            cell.Bind(data, ResolveSprite, OnClickCell);
            cell.SetSelected(selectedSkill != null && selectedSkill.id == data.id);
        }
    }

    //셀 클릭 처리
    private void OnClickCell(SkillData data)
    {
        selectedSkill = data;

        for (int i = 0; i < cellPool.Count; i++)
        {
            if (cellPool[i].gameObject.activeSelf == false)
            {
                continue;
            }

            SkillData bound = cellPool[i].GetBound();
            bool selected = bound != null && selectedSkill != null && bound.id == selectedSkill.id;
            cellPool[i].SetSelected(selected);
        }

        ApplySelection(selectedSkill);
    }

    //선택 변경 시 상세 갱신은 JsonBinder에 위임
    private void ApplySelection(SkillData data)
    {
        if (upgradeButton != null)
        {
            upgradeButton.interactable = data != null;
        }

        if (jsonBinder == null)
        {
            Debug.LogWarning("[SkillPopupContentController] jsonBinder missing.");
            return;
        }

        if (data == null)
        {
            return;
        }

        jsonBinder.SetSkillId(data.id);
    }

    //강화 버튼 클릭
    private void OnClickUpgrade()
    {
        if (selectedSkill == null)
        {
            Debug.LogWarning("[SkillPopupContentController] Upgrade blocked. No selection.");
            return;
        }

        //TODO:팀 스킬 강화/레벨업 시스템 연결 지점
        Debug.Log("[SkillPopupContentController] Upgrade clicked. Hook skill upgrade system.");
    }

    //풀 워밍
    private void WarmPool()
    {
        if (cellPrefab == null || listRoot == null)
        {
            Debug.LogError("[SkillPopupContentController] List references missing.");
            return;
        }

        for (int i = 0; i < poolWarmCount; i++)
        {
            SkillItemCell cell = Instantiate(cellPrefab, listRoot);
            cell.gameObject.SetActive(false);
            cellPool.Add(cell);
        }
    }

    //풀 확보
    private void EnsurePool(int need)
    {
        if (cellPrefab == null || listRoot == null)
        {
            return;
        }

        while (cellPool.Count < need)
        {
            SkillItemCell cell = Instantiate(cellPrefab, listRoot);
            cell.gameObject.SetActive(false);
            cellPool.Add(cell);
        }
    }

    //속성 enum 매핑
    private SkillElement ConvertAttributeToElement(SkillAttribute attribute)
    {
        switch (attribute)
        {
            case SkillAttribute.Fire:
                return SkillElement.Fire;

            case SkillAttribute.Water:
                return SkillElement.Water;

            case SkillAttribute.Wind:
                return SkillElement.Wind;

            case SkillAttribute.Earth:
                return SkillElement.Earth;
        }

        return SkillElement.Fire;
    }

    //스프라이트 로드+캐시
    private Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName))
        {
            return null;
        }

        if (cacheSprites)
        {
            if (spriteCache.TryGetValue(spriteName, out Sprite cached))
            {
                return cached;
            }
        }

        string path = iconBasePath + "/" + spriteName;
        Sprite loaded = Resources.Load<Sprite>(path);

        if (cacheSprites)
        {
            spriteCache[spriteName] = loaded;
        }

        return loaded;
    }
}