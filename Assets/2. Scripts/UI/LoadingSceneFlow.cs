using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
[승문]
LoadingSceneFlow
- 시작: 로딩패널 OFF, 텍스트 OFF
- 데이터 로드 완료: 텍스트 ON, 로딩패널 OFF
- 터치: 텍스트 OFF, 로딩패널 ON, 로비씬 비동기 로드 -> 완료되면 씬 전환
*/
public class LoadingSceneFlow : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string lobbySceneName = "Lobby Scene"; // Build Settings 이름과 정확히 일치!

    [Header("UI")]
    [SerializeField] private GameObject readyTextGO;    // 터치해서 시작
    [SerializeField] private GameObject loadingPanelGO; // 터치 후 띄울 로딩패널

    [Header("Refs")]
    [SerializeField] private LoadingController loadingController;

    [Header("Options")]
    [SerializeField] private bool dontDestroyThis = false;
    [SerializeField] private bool retryOnFail = false;
    [SerializeField] private float retryDelaySec = 1.0f;

    [Header("Transition")]
    [SerializeField] private float minTransitionShowTime = 0.5f; // 터치 후 최소 표시 시간

    private bool _isRunning;
    private bool _readyToEnter;
    private bool _isSceneLoading;

    private async void Start()
    {
        if (_isRunning) return;
        _isRunning = true;

        if (dontDestroyThis) DontDestroyOnLoad(gameObject);

        if (loadingController == null)
            loadingController = FindFirstObjectByType<LoadingController>();

        if (loadingController == null)
        {
            Debug.LogError("[LoadingSceneFlow] LoadingController가 씬에 없습니다.");
            _isRunning = false;
            return;
        }

        // 시작 상태: 둘 다 OFF
        SetReadyUI(false);
        SetLoadingUI(false);

        await RunLoadingAsync();
    }

    private async Task RunLoadingAsync()
    {
        while (true)
        {
            _readyToEnter = false;
            _isSceneLoading = false;

            // 재시도 대비: 항상 초기 UI 정리
            SetReadyUI(false);
            SetLoadingUI(false);

            bool ok = await loadingController.RunAsync();

            if (ok)
            {
                // 데이터 로드 완료 -> 텍스트 ON, 터치 대기
                _readyToEnter = true;
                SetReadyUI(true);
                return;
            }

            Debug.LogError("[LoadingSceneFlow] 로딩 실패: " + loadingController.ErrorMessage);

            if (!retryOnFail)
                return;

            await Task.Delay(Mathf.CeilToInt(retryDelaySec * 1000f));
        }
    }

    private void Update()
    {
        if (!_readyToEnter) return;
        if (_isSceneLoading) return;

        if (IsPointerDown())
        {
            _isSceneLoading = true;
            _readyToEnter = false;

            // 터치하면: 텍스트 OFF, 로딩패널 ON
            SetReadyUI(false);
            SetLoadingUI(true);

            _ = LoadLobbyAsync();
        }
    }

    private bool IsPointerDown()
    {
        if (Input.GetMouseButtonDown(0)) return true;
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began) return true;
        return false;
    }

    private async Task LoadLobbyAsync()
    {
        if (string.IsNullOrEmpty(lobbySceneName))
        {
            Debug.LogError("[LoadingSceneFlow] lobbySceneName이 비어있습니다.");
            _isSceneLoading = false;
            return;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(lobbySceneName);
        if (op == null)
        {
            Debug.LogError("[LoadingSceneFlow] 씬 로드 실패: " + lobbySceneName);
            _isSceneLoading = false;
            return;
        }

        op.allowSceneActivation = false;

        float start = Time.unscaledTime;

        while (op.progress < 0.9f)
            await Task.Yield();

        float elapsed = Time.unscaledTime - start;
        float remain = minTransitionShowTime - elapsed;
        if (remain > 0f)
            await Task.Delay(Mathf.CeilToInt(remain * 1000f));

        op.allowSceneActivation = true;

        while (!op.isDone)
            await Task.Yield();
    }

    private void SetReadyUI(bool ready)
    {
        if (readyTextGO != null) readyTextGO.SetActive(ready);
    }

    private void SetLoadingUI(bool show)
    {
        if (loadingPanelGO != null) loadingPanelGO.SetActive(show);
    }
}