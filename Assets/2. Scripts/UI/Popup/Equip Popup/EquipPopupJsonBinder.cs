using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
[승문]
EquipPopupJsonBinder
-Addressables Address(키)로 Weapon/Accessorie JSON을 로드해서(id->데이터) 캐싱
-SetEquipId로 팝업 내부 텍스트/아이콘을 JSON 기준으로 갱신
-장비 시스템이 아직 미연결이어도 defaultEquipId로 테스트 가능
*/
public class EquipPopupJsonBinder : MonoBehaviour
{
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

    [Header("Json(Addressables Key)")]
    [SerializeField] private bool autoLoadOnAwake = true;//Awake에서 자동 로드
    [SerializeField] private string weaponJsonKey = "Json/Equip/WeaponList";//Addressables Address(키)
    [SerializeField] private string accessorieJsonKey = "Json/Equip/AccessorieList";//Addressables Address(키)

    [Header("Icon(Resources)")]
    [SerializeField] private string iconBasePath = "Icons";//Resources/Icons/...
    [SerializeField] private bool cacheSprites = true;//스프라이트 캐시 사용

    [Header("UI")]
    [SerializeField] private Image iconImage;//장비 아이콘
    [SerializeField] private TMP_Text gradeText;//등급명(일반/고급/레어/영웅/레전)
    [SerializeField] private TMP_Text nameText;//장비 이름
    [SerializeField] private TMP_Text gradeStepText;//등급수치(예:4등급)
    [SerializeField] private TMP_Text ownedCountText;//보유량(예:3/5)(선택)

    [Header("Test(Default)")]
    [SerializeField] private string defaultEquipId = "WP_000";//미연결 상태 테스트용

    private readonly Dictionary<string, EquipJson> equipTable = new Dictionary<string, EquipJson>(256);
    private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>(128);

    private bool isLoaded;
    private string pendingEquipId;

    private void Awake()
    {
        //필수참조 자동 연결(가능한 것만)
        if (iconImage == null)
        {
            iconImage = GetComponentInChildren<Image>(true);
        }

        if (autoLoadOnAwake)
        {
            LoadAll();
        }
    }

    private void OnEnable()
    {
        //팝업이 켜질 때 기본값이라도 보여주기
        if (string.IsNullOrEmpty(defaultEquipId) == false)
        {
            SetEquipId(defaultEquipId);
        }
    }

    //외부에서 선택된 장비 id를 넘겨주면 UI 갱신
    public void SetEquipId(string equipId)
    {
        if (string.IsNullOrEmpty(equipId)) return;

        if (isLoaded == false)
        {
            pendingEquipId = equipId;
            return;
        }

        if (equipTable.TryGetValue(equipId, out EquipJson data) == false || data == null)
        {
            Debug.LogWarning("[EquipPopupJsonBinder] Equip id not found: " + equipId);
            return;
        }

        if (iconImage != null)
        {
            iconImage.sprite = ResolveSprite(data.spriteName);
        }

        if (gradeText != null)
        {
            gradeText.text = ConvertGradeToKorean(data.grade);
        }

        if (nameText != null)
        {
            nameText.text = data.name != null ? data.name : "";
        }

        if (gradeStepText != null)
        {
            gradeStepText.text = data.gradeStep.ToString() + "등급";
        }
    }

    //보유량 표시는 장비매니저 연결 후 외부에서 값만 넣어주면 됨
    public void SetOwnedCount(int owned, int need)
    {
        if (ownedCountText == null) return;
        ownedCountText.text = owned.ToString() + "/" + need.ToString();
    }

    //JSON 전체 로드(Weapon+Accessorie)
    public void LoadAll()
    {
        isLoaded = false;
        equipTable.Clear();

        int pending = 2;

        AddressableTextAssetCache.Load(weaponJsonKey, ta =>
        {
            if (ta == null)
            {
                Debug.LogError("[EquipPopupJsonBinder] WeaponList json not found(key): " + weaponJsonKey);
            }
            else
            {
                ApplyJson(ta.text);
            }

            pending--;
            if (pending <= 0)
            {
                FinishLoad();
            }
        });

        AddressableTextAssetCache.Load(accessorieJsonKey, ta =>
        {
            if (ta == null)
            {
                Debug.LogError("[EquipPopupJsonBinder] AccessorieList json not found(key): " + accessorieJsonKey);
            }
            else
            {
                ApplyJson(ta.text);
            }

            pending--;
            if (pending <= 0)
            {
                FinishLoad();
            }
        });
    }

    private void FinishLoad()
    {
        isLoaded = true;
        Debug.Log("[EquipPopupJsonBinder] Json loaded by Addressables.");

        if (string.IsNullOrEmpty(pendingEquipId) == false)
        {
            string id = pendingEquipId;
            pendingEquipId = null;
            SetEquipId(id);
        }
    }

    //JSON텍스트 파싱 후 테이블 구성
    private void ApplyJson(string jsonText)
    {
        if (string.IsNullOrEmpty(jsonText)) return;

        EquipListRoot root = JsonUtility.FromJson<EquipListRoot>(jsonText);
        if (root == null || root.equipList == null) return;

        for (int i = 0; i < root.equipList.Length; i++)
        {
            EquipJson e = root.equipList[i];
            if (e == null) continue;
            if (string.IsNullOrEmpty(e.id)) continue;

            if (equipTable.ContainsKey(e.id))
            {
                continue;
            }

            equipTable.Add(e.id, e);
        }
    }

    // [조민희] 스프라이트 로드 (AssetBundleLoader 사용)
    private Sprite ResolveSprite(string spriteName)
    {
        if (string.IsNullOrEmpty(spriteName)) return null;

        if (cacheSprites && spriteCache.TryGetValue(spriteName, out Sprite cached))
        {
            return cached;
        }

        // AssetBundleLoader를 통해 스프라이트 로드
        Sprite loaded = SlayerLegend.Resource.ResourceManager.Instance.LoadSprite(spriteName);

        if (cacheSprites && loaded != null)
        {
            spriteCache[spriteName] = loaded;
        }

        return loaded;
    }

    //등급 한글 변환
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