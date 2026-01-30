using UnityEngine;

/*
ResolutionFixed 스크립트 동작
-목표 비율(setWidth:setHeight)을 유지하도록 카메라에 레터박스를 적용
-UI는 Canvas Scaler가 담당하고, 이 스크립트는 월드/배경 카메라 화면 비율 고정용
*/
public class ResolutionFixed : MonoBehaviour
{
    //기준 해상도(비율 계산 기준)
    [SerializeField] private int setWidth = 1080;
    [SerializeField] private int setHeight = 1920;

    //레터박스를 적용할 카메라
    [SerializeField] private Camera targetCamera;

    //해상도 강제 여부(원치 않으면 끄고 레터박스만 사용)
    [SerializeField] private bool forceResolution = false;

    //이전 상태 저장(변경 감지)
    private int lastWidth;
    private int lastHeight;
    private FullScreenMode lastMode;

    private void Awake()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    private void Start()
    {
        ApplyResolution(forceResolution);
        CacheScreenState();
    }

    private void Update()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }

        if (Screen.width != lastWidth ||
            Screen.height != lastHeight ||
            Screen.fullScreenMode != lastMode)
        {
            ApplyResolution(false);
            CacheScreenState();
        }
    }

    //현재 화면 상태 저장
    private void CacheScreenState()
    {
        lastWidth = Screen.width;
        lastHeight = Screen.height;
        lastMode = Screen.fullScreenMode;
    }

    //레터박스 적용 함수
    private void ApplyResolution(bool applySetResolution)
    {
        if (applySetResolution)
        {
            Screen.SetResolution(setWidth, setHeight, FullScreenMode.FullScreenWindow);
        }

        if (targetCamera == null) return;

        int deviceWidth = Screen.width;
        int deviceHeight = Screen.height;

        //목표 비율(1080 / 1920)
        float targetAspect = (float)setWidth / setHeight;

        //현재 화면 비율
        float deviceAspect = (float)deviceWidth / deviceHeight;

        if (deviceAspect > targetAspect)
        {
            //가로가 더 긴 화면: 좌우 레터박스
            float newWidth = targetAspect / deviceAspect;
            targetCamera.rect = new Rect((1f - newWidth) / 2f, 0f, newWidth, 1f);
        }
        else if (deviceAspect < targetAspect)
        {
            //세로가 더 긴 화면: 상하 레터박스
            float newHeight = deviceAspect / targetAspect;
            targetCamera.rect = new Rect(0f, (1f - newHeight) / 2f, 1f, newHeight);
        }
        else
        {
            //비율 동일: 레터박스 없음
            targetCamera.rect = new Rect(0f, 0f, 1f, 1f);
        }
    }
}
