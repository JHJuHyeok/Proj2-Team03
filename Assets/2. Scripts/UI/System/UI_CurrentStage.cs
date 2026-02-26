using UnityEngine;
using TMPro;

public class UI_CurrentStage : MonoBehaviour
{
    [SerializeField] private TMP_Text _stageCountText;
    [SerializeField] private TMP_Text _stageNameText;

    private void OnEnable()
    {
        // 나중에 스테이지 변경 관련 스크립트 있을 경우 이벤트 구독

        // 마지막으로 클리어한 스테이지로 초기화
    }

    private void Refresh(StageData stage)
    {
        _stageCountText.text = stage.id;
        _stageNameText.text = stage.name;
    }
}
