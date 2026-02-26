using System.Collections.Generic;
using UnityEngine;

/*
[승문]
PopupManager
- PopupRoot 아래에 미리 배치된(비활성) 팝업들을 등록해서 SetActive로 관리
- 스택 관리: 뒤 팝업 비활성화 / 닫으면 이전 팝업 복귀
*/
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private Transform popupRoot;

    // ✅ 더 이상 "프리팹 Instantiate" 안 함 -> Root 아래에서 자동 수집
    // [Header("Popup Prefabs")]
    // [SerializeField] private UIPopup[] popupPrefabs;

    private readonly Dictionary<PopupId, UIPopup> prefabTable = new Dictionary<PopupId, UIPopup>();
    private readonly Stack<UIPopup> stack = new Stack<UIPopup>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildPrefabTable();
        // 시작 시 팝업들 모두 숨기고 스택 초기화
        HideAll();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void BuildPrefabTable()
    {
        prefabTable.Clear();

        if (popupRoot == null)
        {
            Debug.LogWarning("[PopupManager] popupRoot is null");
            return;
        }

        // ✅ popupRoot 아래에 있는 UIPopup(자식 클래스 포함)을 전부 수집 (비활성 포함)
        UIPopup[] pops = popupRoot.GetComponentsInChildren<UIPopup>(true);

        for (int i = 0; i < pops.Length; i++)
        {
            UIPopup p = pops[i];
            if (p == null) continue;

            PopupId id = p.PopupId;
            if (id == PopupId.None)
            {
                Debug.LogWarning("[PopupManager] PopupId is None. Check object: " + p.name);
                continue;
            }

            if (prefabTable.ContainsKey(id))
            {
                Debug.LogWarning("[PopupManager] Duplicate PopupId: " + id + " / object: " + p.name);
                continue;
            }

            prefabTable.Add(id, p);
        }
    }

    private void HideAll()
    {
        // popupRoot 아래 팝업 전부 비활성화
        if (popupRoot != null)
        {
            UIPopup[] pops = popupRoot.GetComponentsInChildren<UIPopup>(true);
            foreach (var p in pops)
            {
                if (p != null) p.gameObject.SetActive(false);
            }
        }

        stack.Clear();
    }

    // ✅ 팝업 열기: 현재 top 숨기고, Root에 있는 해당 팝업을 활성화해서 스택에 push
    public UIPopup Open(PopupId id, object param = null)
    {
        if (id == PopupId.None)
        {
            Debug.LogWarning("[PopupManager] Open failed. id is None");
            return null;
        }

        if (popupRoot == null)
        {
            Debug.LogWarning("[PopupManager] Open failed. popupRoot is null");
            return null;
        }

        // 혹시 런타임에 PopupRoot 자식이 바뀌었을 수 있으니, 못 찾으면 한 번 리빌드 시도
        if (!prefabTable.TryGetValue(id, out UIPopup instance) || instance == null)
        {
            BuildPrefabTable();
            prefabTable.TryGetValue(id, out instance);
        }

        if (instance == null)
        {
            Debug.LogWarning("[PopupManager] Open failed. popup not found under popupRoot: " + id);
            return null;
        }

        // 현재 top 팝업 비활성화
        if (stack.Count > 0)
        {
            UIPopup top = stack.Peek();
            if (top != null) top.gameObject.SetActive(false);
        }

        // ✅ 클론 생성 X, 기존 오브젝트 활성화
        instance.gameObject.SetActive(true);
        instance.OnOpen(param);

        stack.Push(instance);
        return instance;
    }

    // ✅ 최상단 팝업 닫기 (Destroy X)
    public void CloseTop()
    {
        if (stack.Count <= 0) return;

        UIPopup top = stack.Pop();
        if (top != null)
        {
            top.OnClose();
            top.gameObject.SetActive(false);
        }

        // 이전 팝업 복귀
        if (stack.Count > 0)
        {
            UIPopup prev = stack.Peek();
            if (prev != null) prev.gameObject.SetActive(true);
        }
    }

    // 특정 팝업이 "현재 top"일 때만 닫기
    public void CloseIfTop(UIPopup popup)
    {
        if (popup == null) return;
        if (stack.Count <= 0) return;

        UIPopup top = stack.Peek();
        if (top != popup) return;

        CloseTop();
    }

    public void CloseAll()
    {
        while (stack.Count > 0)
        {
            UIPopup p = stack.Pop();
            if (p == null) continue;

            p.OnClose();
            p.gameObject.SetActive(false);
        }
    }

    public bool IsOpenAny() => stack.Count > 0;
    public int OpenCount => stack.Count;

#if UNITY_EDITOR
    [ContextMenu("DEBUG/Rebuild Popup Table From Root")]
    private void Editor_RebuildPrefabTable()
    {
        BuildPrefabTable();
        Debug.Log("[PopupManager] Table rebuilt from popupRoot. Count=" + prefabTable.Count);
    }
#endif
}