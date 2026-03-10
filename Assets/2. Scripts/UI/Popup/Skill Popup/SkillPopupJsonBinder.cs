using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
[승문]
SkillPopupJsonBinder
-Addressables Address(키)로 SkillList JSON을 로드해서(id->데이터) 캐싱
-SetSkillId로 팝업 상세 UI를 JSON 기준으로 갱신
-스킬 속성 아이콘은 SkillPopup이 담당
*/
public class SkillPopupJsonBinder : MonoBehaviour
{
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
        public string grade;
        public int coolTime;
        public int attackCount;
        public int mpCost;
        public string description;
    }

    [Header("Json(Addressables Key)")]
    [SerializeField] private bool autoLoadOnAwake = true;//Awake에서 자동 로드
    [SerializeField] private string skillJsonKey = "Json/Skill/SkillList";//Addressables Address(키)

    [Header("Icon(Resources)")]
    [SerializeField] private string iconBasePath = "Icons";//Resources/Icons/...
    [SerializeField] private bool cacheSprites = true;//스프라이트 캐시 사용

    [Header("UI")]
    [SerializeField] private Image iconImage;//스킬 아이콘(선택)
    [SerializeField] private TMP_Text gradeAndNameText;//등급+이름(한 텍스트)
    [SerializeField] private TMP_Text descText;//설명(선택)
    [SerializeField] private TMP_Text mpValueText;//MP소모 숫자만
    [SerializeField] private TMP_Text delayLabelText;//라벨(쿨타임/필요공격수)
    [SerializeField] private TMP_Text delayValueText;//값(쿨타임/필요공격수)
    [SerializeField] private TMP_Text attackCountValueText;//공격횟수 숫자(선택)

    [Header("Delay Mode")]
    [SerializeField] private bool useCoolTimeAsDelay = true;//true:쿨타임,false:필요공격수

    [Header("Test(Default)")]
    [SerializeField] private string defaultSkillId = "SK_000";//미연결 상태 테스트용

    private readonly Dictionary<string, SkillJson> table = new Dictionary<string, SkillJson>(256);
    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(128);

    private bool isLoaded;
    private string pendingSkillId;

    private void Awake()
    {
        if (autoLoadOnAwake)
        {
            LoadAll();
        }
    }

    private void OnEnable()
    {
        if (string.IsNullOrEmpty(defaultSkillId) == false)
        {
            SetSkillId(defaultSkillId);
        }
    }

    public void SetSkillId(string skillId)
    {
        if (string.IsNullOrEmpty(skillId)) return;

        if (isLoaded == false)
        {
            pendingSkillId = skillId;
            return;
        }

        if (table.TryGetValue(skillId, out SkillJson data) == false || data == null)
        {
            Debug.LogWarning("[SkillPopupJsonBinder] Skill id not found: " + skillId);
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = ResolveSprite(data.spriteName);
        }

        if (gradeAndNameText != null)
        {
            gradeAndNameText.text = ConvertGradeToKorean(data.grade) + " " + (data.name != null ? data.name : "");
        }

        if (descText != null)
        {
            descText.text = data.description != null ? data.description : "";
        }

        if (mpValueText != null)
        {
            mpValueText.text = data.mpCost.ToString();
        }

        if (useCoolTimeAsDelay)
        {
            if (delayLabelText != null) delayLabelText.text = "쿨타임";
            if (delayValueText != null) delayValueText.text = data.coolTime.ToString();
        }
        else
        {
            if (delayLabelText != null) delayLabelText.text = "필요공격수";
            if (delayValueText != null) delayValueText.text = data.attackCount.ToString();
        }

        if (attackCountValueText != null)
        {
            attackCountValueText.text = data.attackCount.ToString();
        }
    }

    public void LoadAll()
    {
        isLoaded = false;
        table.Clear();

        AddressableTextAssetCache.Load(skillJsonKey, ta =>
        {
            if (ta == null)
            {
                Debug.LogError("[SkillPopupJsonBinder] SkillList json not found(key): " + skillJsonKey);
                isLoaded = true;
                return;
            }

            ApplyJson(ta.text);
            isLoaded = true;

            Debug.Log("[SkillPopupJsonBinder] Json loaded by Addressables.");

            if (string.IsNullOrEmpty(pendingSkillId) == false)
            {
                string id = pendingSkillId;
                pendingSkillId = null;
                SetSkillId(id);
            }
        });
    }

    private void ApplyJson(string jsonText)
    {
        if (string.IsNullOrEmpty(jsonText)) return;

        SkillListRoot root = JsonUtility.FromJson<SkillListRoot>(jsonText);
        if (root == null || root.skillList == null) return;

        for (int i = 0; i < root.skillList.Length; i++)
        {
            SkillJson s = root.skillList[i];
            if (s == null) continue;
            if (string.IsNullOrEmpty(s.id)) continue;

            if (table.ContainsKey(s.id))
            {
                continue;
            }

            table.Add(s.id, s);
        }
    }

    private Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return null;

        if (cacheSprites && spriteCache.TryGetValue(spriteName, out Sprite cached))
        {
            return cached;
        }

        // [조민희] Resources.Load 대신 ResourceManager.Instance.LoadSprite 사용
        Sprite loaded = null;
        if (SlayerLegend.Resource.ResourceManager.Instance != null)
        {
            loaded = SlayerLegend.Resource.ResourceManager.Instance.LoadSprite(spriteName);
        }
        else
        {
            // Fallback: Resources 폴더에서 로드
            string path = iconBasePath + "/" + spriteName;
            loaded = Resources.Load<Sprite>(path);
        }

        if (cacheSprites && loaded != null)
        {
            spriteCache[spriteName] = loaded;
        }

        return loaded;
    }

    private string ConvertGradeToKorean(string grade)
    {
        if (string.IsNullOrEmpty(grade)) return "일반";
        if (grade == "Common") return "일반";
        if (grade == "Uncommon") return "고급";
        if (grade == "Rare") return "레어";
        if (grade == "Epic") return "영웅";
        if (grade == "Legendary") return "레전";
        return grade;
    }
}