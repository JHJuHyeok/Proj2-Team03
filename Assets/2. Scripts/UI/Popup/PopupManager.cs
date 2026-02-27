using System.Collections.Generic;
using UnityEngine;

/*
[승문]
PopupManager
- PopupRoot 아래에 미리 배치된(비활성) 팝업들을 등록해서 SetActive로 관리
- 스택 관리: 뒤 팝업 비활성화 / 닫으면 이전 팝업 복귀
- 같은 팝업 중복 Open 방지
- ESC(또는 Android Back)로 CloseTop
*/
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private Transform popupRoot;

    [Header("Options")]
    [SerializeField] private bool closeOnEsc = true;
    [SerializeField] private bool preventDuplicateOpen = true;

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
        HideAll();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (!closeOnEsc) return;

        // PC: ESC, Android: Back 버튼도 KeyCode.Escape로 들어옴
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTop();
        }
    }

    private void BuildPrefabTable()
    {
        prefabTable.Clear();

        if (popupRoot == null)
        {
            Debug.LogWarning("[PopupManager] popupRoot is null");
            return;
        }

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

    // 팝업 열기: 현재 top 숨기고, Root에 있는 해당 팝업을 활성화해서 스택에 push
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

        // 중복 방지 1) 지금 top이 같은 팝업이면: 스택에 다시 안 쌓고 "갱신만"
        if (preventDuplicateOpen && stack.Count > 0 && stack.Peek() == instance)
        {
            instance.gameObject.SetActive(true);
            instance.OnOpen(param); // 내용 갱신이 필요 없으면 이 줄 빼도 됨
            return instance;
        }

        // 중복 방지 2) 스택 어딘가에 이미 있으면: 그 위에 있는 팝업들만 닫아서 "그 팝업을 top으로"
        if (preventDuplicateOpen && stack.Contains(instance))
        {
            BringToTop(instance, param);
            return instance;
        }

        // 현재 top 팝업 비활성화
        if (stack.Count > 0)
        {
            UIPopup top = stack.Peek();
            if (top != null) top.gameObject.SetActive(false);
        }

        instance.gameObject.SetActive(true);
        instance.OnOpen(param);

        stack.Push(instance);
        return instance;
    }

    // target이 스택 중간에 있을 때, target 위에 있는 팝업만 닫고 target을 top으로 올림
    private void BringToTop(UIPopup target, object param)
    {
        while (stack.Count > 0 && stack.Peek() != target)
        {
            CloseTop();
        }

        if (stack.Count > 0 && stack.Peek() == target)
        {
            target.gameObject.SetActive(true);
            target.OnOpen(param);
        }
    }

    public void CloseTop()
    {
        if (stack.Count <= 0) return;

        UIPopup top = stack.Pop();
        if (top != null)
        {
            top.OnClose();
            top.gameObject.SetActive(false);
        }

        if (stack.Count > 0)
        {
            UIPopup prev = stack.Peek();
            if (prev != null) prev.gameObject.SetActive(true);
        }
    }

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