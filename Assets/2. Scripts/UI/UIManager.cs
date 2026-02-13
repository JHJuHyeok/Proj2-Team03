using UnityEngine;
using System.Collections.Generic;

/*[승문]
UIManager
- 모든 UI의 표시 / 숨김 / 전환 담당
- GameManager 상태 변경에 반응
- 하단 탭 → 페이지 전환
*/
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Screens")]
    [SerializeField] private GameObject middleScreen;
    [SerializeField] private GameObject topScreen;
    [SerializeField] private GameObject bottomScreen;

    [Header("Bottom Pages")]
    [SerializeField] private Transform pageContainer;

    // PageContainer 자식들을 자동 관리
    private List<GameObject> pages = new List<GameObject>();
    private int currentPageIndex = 0;

    private void Awake()
    {
        // 싱글톤
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        CachePages();
    }

    private void OnEnable()
    {
        // GameManager 상태 구독
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged += OnGameStateChanged;
        }
    }

    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnGameStateChanged -= OnGameStateChanged;
        }
    }

    // PageContainer 자식 자동 수집
    private void CachePages()
    {
        pages.Clear();

        if (pageContainer == null) return;

        for (int i = 0; i < pageContainer.childCount; i++)
        {
            pages.Add(pageContainer.GetChild(i).gameObject);
        }
    }

    // 게임 상태에 따른 UI 반응
    private void OnGameStateChanged(GameManager.GameState state)
    {
        switch (state)
        {
            case GameManager.GameState.Lobby:
                ShowLobbyUI();
                break;

            case GameManager.GameState.Battle:
                ShowBattleUI();
                break;

            case GameManager.GameState.Menu:
                ShowMenuUI();
                break;
        }
    }

    //Screen Control
    private void ShowLobbyUI()
    {
        topScreen.SetActive(true);
        middleScreen.SetActive(true);
        bottomScreen.SetActive(true);

        ShowPage(0); // 기본 캐릭터 페이지
    }

    private void ShowBattleUI()
    {
        topScreen.SetActive(true);
        middleScreen.SetActive(true);
        bottomScreen.SetActive(false); // 전투 중 하단 숨김
    }

    private void ShowMenuUI()
    {
        topScreen.SetActive(false);
        middleScreen.SetActive(false);
        bottomScreen.SetActive(true);
    }

    //Page Control

    // 하단 탭 버튼에서 호출
    public void ShowPage(int index)
    {
        if (pages.Count == 0) return;

        if (index < 0 || index >= pages.Count) return;

        currentPageIndex = index;

        for (int i = 0; i < pages.Count; i++)
        {
            pages[i].SetActive(i == index);
        }
    }

    public int GetCurrentPageIndex()
    {
        return currentPageIndex;
    }
}
