using System.Collections.Generic;
using UnityEngine;

/*
[승문]
PopupManager
- 팝업 등록(프리팹) / 생성 / 스택 관리
- 뒤 팝업은 비활성화하고, 닫으면 이전 팝업 복귀
- UIManager와 분리(기능 세분화)
*/
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private Transform popupRoot;

    [Header("Popup Prefabs")]
    [SerializeField] private UIPopup[] popupPrefabs;

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
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void BuildPrefabTable()
    {
        prefabTable.Clear();

        if (popupPrefabs == null) return;

        for (int i = 0; i < popupPrefabs.Length; i++)
        {
            UIPopup p = popupPrefabs[i];
            if (p == null) continue;

            PopupId id = p.PopupId;
            if (id == PopupId.None)
            {
                Debug.LogWarning("[PopupManager] PopupId is None. Check prefab: " + p.name);
                continue;
            }

            if (prefabTable.ContainsKey(id))
            {
                Debug.LogWarning("[PopupManager] Duplicate PopupId: " + id + " / prefab: " + p.name);
                continue;
            }

            prefabTable.Add(id, p);
        }
    }

    // 팝업 열기: 현재 top을 숨기고, 새 팝업을 생성해 스택에 push
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

        if (!prefabTable.TryGetValue(id, out UIPopup prefab) || prefab == null)
        {
            Debug.LogWarning("[PopupManager] Open failed. prefab not found: " + id);
            return null;
        }

        // 현재 top 팝업 비활성화
        if (stack.Count > 0)
        {
            UIPopup top = stack.Peek();
            if (top != null) top.gameObject.SetActive(false);
        }

        // 새 팝업 생성
        UIPopup instance = Instantiate(prefab, popupRoot);

        // (안전) 생성 직후 활성화 보장
        instance.gameObject.SetActive(true);

        // 팝업 open 콜백
        instance.OnOpen(param);

        // 스택 push
        stack.Push(instance);

        return instance;
    }

    // 최상단 팝업 닫기
    public void CloseTop()
    {
        if (stack.Count <= 0) return;

        UIPopup top = stack.Pop();
        if (top != null)
        {
            top.OnClose();
            Destroy(top.gameObject);
        }

        // 이전 팝업 복귀
        if (stack.Count > 0)
        {
            UIPopup prev = stack.Peek();
            if (prev != null) prev.gameObject.SetActive(true);
        }
    }

    // 특정 ID 팝업이 "현재 top"일 때만 닫기 (닫기 버튼이 안전하게 동작하도록)
    public void CloseIfTop(UIPopup popup)
    {
        if (popup == null) return;
        if (stack.Count <= 0) return;

        UIPopup top = stack.Peek();
        if (top != popup) return;

        CloseTop();
    }

    // 모든 팝업 닫기
    public void CloseAll()
    {
        while (stack.Count > 0)
        {
            UIPopup p = stack.Pop();
            if (p == null) continue;

            p.OnClose();
            Destroy(p.gameObject);
        }
    }

    public bool IsOpenAny()
    {
        return stack.Count > 0;
    }

    public int OpenCount => stack.Count;

#if UNITY_EDITOR
    [ContextMenu("DEBUG/Rebuild Prefab Table")]
    private void Editor_RebuildPrefabTable()
    {
        BuildPrefabTable();
        Debug.Log("[PopupManager] Prefab table rebuilt. Count=" + prefabTable.Count);
    }
#endif
}