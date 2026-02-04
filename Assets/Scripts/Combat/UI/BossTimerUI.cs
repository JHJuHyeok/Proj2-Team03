using UnityEngine;
using UnityEngine.UI;

public class BossTimerUI : MonoBehaviour
{
    [SerializeField] private Image timerImage;
    [SerializeField] private GameObject timerPanel;

    private void Update()
    {
        if (CombatManager.Instance == null) return;
        if (timerPanel == null) return;
        if (timerImage == null) return;

        if (CombatManager.Instance.CurrentState == CombatState.BossBattle)
        {
            timerPanel.SetActive(true);

            float remaining = CombatManager.Instance.BossTimeRemaining;
            float maxTime = CombatManager.BOSS_TIME_LIMIT;

            // 0초 미만 보정
            remaining = Mathf.Max(0, remaining);

            if (timerImage != null)
            {
                // 남은 시간 비율로 fillAmount 설정 (0.0 ~ 1.0)
                timerImage.fillAmount = remaining / maxTime;
            }
        }
        else
        {
            timerPanel.SetActive(false);
            timerImage.fillAmount = 1f;
        }
    }
}
