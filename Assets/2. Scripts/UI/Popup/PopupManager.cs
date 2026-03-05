using System.Collections.Generic;
using UnityEngine;

/*
[승문]
PopupManager
- PopupRoot 아래에 "미리 배치된(비활성)" 팝업들을 등록해서 SetActive로 관리
- 스택 관리: 뒤 팝업 비활성화 / 닫으면 이전 팝업 복귀
- 같은 팝업 중복 오픈 방지
- ESC(또는 Android Back)로 Top 닫기
*/
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private Transform popupRoot;

    [Header("ESC Close")]
    [SerializeField] private bool closeByEsc = true;

    private readonly Dictionary<PopupId, UIPopup> table = new();
    private readonly Stack<UIPopup> stack = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        RebuildTableFromRoot();
        HideAll();
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Update()
    {
        if (!closeByEsc) return;
        if (stack.Count <= 0) return;

        // PC ESC / Android Back
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            CloseTop();
        }
    }

    private void RebuildTableFromRoot()
    {
        table.Clear();

        if (popupRoot == null)
        {
            Debug.LogWarning("[PopupManager] popupRoot is null");
            return;
        }

        // 비활성 포함 전체 수집
        UIPopup[] pops = popupRoot.GetComponentsInChildren<UIPopup>(true);

        foreach (var p in pops)
        {
            if (p == null) continue;
            if (p.PopupId == PopupId.None)
            {
                Debug.LogWarning("[PopupManager] PopupId is None: " + p.name);
                continue;
            }

            if (table.ContainsKey(p.PopupId))
            {
                Debug.LogWarning("[PopupManager] Duplicate PopupId: " + p.PopupId + " / " + p.name);
                continue;
            }

            table.Add(p.PopupId, p);
        }
    }

    private void HideAll()
    {
        if (popupRoot != null)
        {
            var pops = popupRoot.GetComponentsInChildren<UIPopup>(true);
            foreach (var p in pops)
            {
                if (p != null) p.gameObject.SetActive(false);
            }
        }
        stack.Clear();
    }

    public UIPopup Open(PopupId id, object param = null)
    {
        if (id == PopupId.None) return null;
        if (popupRoot == null) return null;

        if (!table.TryGetValue(id, out var popup) || popup == null)
        {
            RebuildTableFromRoot();
            table.TryGetValue(id, out popup);
        }

        if (popup == null)
        {
            Debug.LogWarning("[PopupManager] Open failed. not found under popupRoot: " + id);
            return null;
        }

        if (stack.Count > 0 && stack.Peek() == popup)
        {
            popup.gameObject.SetActive(true);
            popup.OnOpen(param);
            return popup;
        }

        if (stack.Contains(popup))
        {
            while (stack.Count > 0 && stack.Peek() != popup)
                CloseTop();

            popup.gameObject.SetActive(true);
            popup.OnOpen(param);
            return popup;
        }

        if (stack.Count > 0)
        {
            var top = stack.Peek();
            if (top != null) top.gameObject.SetActive(false);
        }

        popup.gameObject.SetActive(true);
        popup.OnOpen(param);
        stack.Push(popup);

        return popup;
    }

    //Top팝업 닫기
    public void CloseTop()
    {
        if (stack.Count <= 0) return;

        var top = stack.Pop();
        if (top != null)
        {
            top.OnClose();
            top.gameObject.SetActive(false);
        }

        if (stack.Count > 0)
        {
            var prev = stack.Peek();
            if (prev != null) prev.gameObject.SetActive(true);
        }
    }

    //열린팝업이 하나라도 있는지 확인
    public bool IsOpenAny()
    {
        return stack.Count > 0;
    }

    //요청한팝업이 Top일때만 닫기
    public void CloseIfTop(UIPopup popup)
    {
        if (popup == null) return;
        if (stack.Count <= 0) return;

        if (stack.Peek() != popup)
        {
            return;
        }

        CloseTop();
    }
}