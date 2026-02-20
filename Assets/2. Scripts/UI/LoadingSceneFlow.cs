using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
[승문]
LoadingSceneFlow
-로딩씬에서 "매니저 준비 + 데이터 초기화"를 끝낸 뒤 로비씬으로 이동
-LoadingController는 팀원이 작업 중이므로 여기서는 '호출'만 담당

동작 순서:
  1) 싱글톤 매니저들을 미리 생성(필요 시)
  2) LoadingController.Initialize()
  3) await LoadingController.InitDataAsync()
  4) 성공하면 lobbySceneName 씬 로드
*/
public class LoadingSceneFlow : MonoBehaviour
{
    [Header("Scene")]
    [Tooltip("초기화가 끝나면 이동할 로비 씬 이름")]
    [SerializeField] private string lobbySceneName = "Lobby Scene";

    [Header("Refs")]
    [Tooltip("로딩씬에 있는 LoadingController를 연결(없으면 Find)")]
    [SerializeField] private LoadingController loadingController;

    [Header("Options")]
    [Tooltip("로딩 중에 로딩씬 오브젝트를 파괴하지 않게 유지할지")]
    [SerializeField] private bool dontDestroyThis = false;

    [Tooltip("실패 시 자동 재시도 여부(개발 중엔 꺼두는 걸 추천)")]
    [SerializeField] private bool retryOnFail = false;

    [Tooltip("실패 시 재시도 딜레이(초)")]
    [SerializeField] private float retryDelaySec = 1.0f;

    private bool _isRunning;

    private async void Start()
    {
        if (_isRunning) return;
        _isRunning = true;

        if (dontDestroyThis)
        {
            DontDestroyOnLoad(gameObject);
        }

        // 0) 로딩 컨트롤러 자동 탐색
        if (loadingController == null)
        {
            loadingController = FindFirstObjectByType<LoadingController>();
        }

        // 1) 매니저 미리 생성(싱글톤 Instance를 "한번" 호출해서 만들어둠)
        BootstrapManagers();

        // 2) 로딩 실행
        await RunLoadingFlowAsync();
    }

    private void BootstrapManagers()
    {
        // GameManager는 Singleton<T>가 아니라도 Awake에서 Instance 세팅하도록 되어있음(프로젝트 구성에 맞춰)
        // 여기서 "참조만" 해서 미리 생성되게 하는 목적

        // Singleton<T> 기반 매니저들
        //_ = BackendManager.Instance;
        //_ = DataManager.Instance;
        //_ = SaveManager.Instance;

        // 필요하면 여기 추가:
        // _ = SomeOtherManager.Instance;
    }

    private async Task RunLoadingFlowAsync()
    {
        if (loadingController == null)
        {
            Debug.LogError("[LoadingSceneFlow] LoadingController를 찾을 수 없습니다. 로딩씬 오브젝트에 추가/연결하세요.");
            _isRunning = false;
            return;
        }

        while (true)
        {
            // 한 프레임 쉬어서(캔버스/이벤트시스템/Addressables 등) 초기화 안정화
            await Task.Yield();

            // LoadingController 초기화 + 데이터 준비
            //loadingController.Initialize();

            bool ok = false;
            try
            {
                //ok = await loadingController.InitDataAsync();
            }
            catch (System.Exception e)
            {
                Debug.LogError("[LoadingSceneFlow] InitDataAsync 예외: " + e);
                ok = false;
            }

            if (ok)
            {
                await LoadLobbyAsync();
                break;
            }

            Debug.LogError("[LoadingSceneFlow] 로딩 실패!");

            if (!retryOnFail)
            {
                // 실패 처리 UI(재시도 버튼 등)는 LoadingController쪽에서 처리하는 구조가 보통이라
                // 여기서는 일단 멈춰둠
                break;
            }

            // 재시도 옵션
            await Task.Delay((int)(retryDelaySec * 1000f));
        }

        _isRunning = false;
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

        // 로딩 완료까지 대기
        while (!op.isDone)
        {
            await Task.Yield();
        }
    }
}