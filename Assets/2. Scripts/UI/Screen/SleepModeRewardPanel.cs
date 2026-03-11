using System;
using UnityEngine;
using UnityEngine.UI;

/*
[승문]
SleepRewardPanel
- 리얼타임 기준 방치 보상 패널
- 패널 자체가 버튼
- 일정 시간 후 패널 표시
- 최대 보상 시간은 24시간
- 콘솔에 "0시 : 0분" 형식으로 출력
*/
[RequireComponent(typeof(Button))]
public class SleepRewardPanel : MonoBehaviour
{
    [Header("패널 표시 시간")]
    [Tooltip("이 시간이 지나면 패널이 나타남 (초 단위)")]
    [SerializeField] private double showAfterSeconds = 60;

    [Header("보상 설정")]
    [Tooltip("최대 누적 시간 (24시간 = 86400초)")]
    [SerializeField] private double maxRewardSeconds = 86400;

    [Tooltip("초당 골드")]
    [SerializeField] private double goldPerSecond = 10;

    private DateTime startTimeUtc;
    private Button panelButton;

    private int lastLoggedMinute = -1;

    private void Awake()
    {
        panelButton = GetComponent<Button>();
        panelButton.onClick.AddListener(ClaimReward);

        startTimeUtc = DateTime.UtcNow;

        // 시작 시 패널 숨김
        gameObject.SetActive(false);
    }

    private void Update()
    {
        double elapsed = (DateTime.UtcNow - startTimeUtc).TotalSeconds;

        // 24시간 제한
        elapsed = Math.Min(elapsed, maxRewardSeconds);

        int hours = (int)(elapsed / 3600);
        int minutes = (int)((elapsed % 3600) / 60);

        // 분 단위로 콘솔 출력
        if (minutes != lastLoggedMinute)
        {
            lastLoggedMinute = minutes;
            Debug.Log($"{hours}시 : {minutes}분");
        }

        // 일정 시간 후 패널 표시
        if (!gameObject.activeSelf && elapsed >= showAfterSeconds)
        {
            Debug.Log("방치 보상 패널 표시");
            gameObject.SetActive(true);
        }
    }

    private void ClaimReward()
    {
        double elapsed = (DateTime.UtcNow - startTimeUtc).TotalSeconds;

        // 24시간 제한
        elapsed = Math.Min(elapsed, maxRewardSeconds);

        double rewardGold = elapsed * goldPerSecond;

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddCurrency(CurrencyType.Gold, rewardGold);
        }

        Debug.Log($"보상 지급 : {rewardGold}");

        // 타이머 리셋
        startTimeUtc = DateTime.UtcNow;

        // 패널 숨김
        gameObject.SetActive(false);
    }
}