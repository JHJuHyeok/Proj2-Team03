using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
[승문]
LoadingSceneFlow
-로딩씬에서 데이터 로드 완료 후, "화면 터치"로 로비씬 이동
-버튼 없음
*/
public class LoadingSceneFlow : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string lobbySceneName = "Lobby Scene";

    [Header("Refs")]
    [SerializeField] private LoadingController loadingController;

    [Header("Options")]
    [SerializeField] private bool dontDestroyThis = false;
    [SerializeField] private bool retryOnFail = false;
    [SerializeField] private float retryDelaySec = 1.0f;

    private bool _isRunning;
    private bool _readyToEnter;   // 로드 완료 후 터치 대기
    private bool _loadingOk;

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

        await RunLoadingAsync();
    }

    private async Task RunLoadingAsync()
    {
        while (true)
        {
            _readyToEnter = false;
            _loadingOk = false;

            bool ok = await loadingController.RunAsync();
            _loadingOk = ok;

            if (ok)
            {
                // 이제부터 터치하면 넘어가게 대기
                _readyToEnter = true;
                break;
            }

            Debug.LogError("[LoadingSceneFlow] 로딩 실패: " + loadingController.ErrorMessage);

            if (!retryOnFail)
                break;

            await Task.Delay((int)(retryDelaySec * 1000f));
        }
    }

    private void Update()
    {
        if (!_readyToEnter) return;

        if (IsPointerDown())
        {
            // 터치(클릭)하면 로비로 이동
            _readyToEnter = false;
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
            return;
        }

        AsyncOperation op = SceneManager.LoadSceneAsync(lobbySceneName);
        if (op == null)
        {
            Debug.LogError("[LoadingSceneFlow] 씬 로드 실패: " + lobbySceneName);
            return;
        }

        while (!op.isDone)
            await Task.Yield();
    }
}