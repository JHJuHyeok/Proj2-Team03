using UnityEngine;

/*
[승문]
TopIconMenu
- 상단 아이콘 메뉴 펼침/접기 제어
- Main Button은 항상 보임
- Settings / Trophy / Sleep / Mail 버튼은 열렸을 때만 표시
- 반투명 배경 패널도 메뉴와 함께 표시/숨김
- Main Button 아이콘 변경은 Button/Image 설정에서 별도로 처리
*/
public class TopIconMenu : MonoBehaviour
{
    [Header("Sub Buttons")]
    [SerializeField] private GameObject settingsButton;
    [SerializeField] private GameObject trophyButton;
    [SerializeField] private GameObject sleepButton;
    [SerializeField] private GameObject mailButton;

    [Header("Background")]
    [SerializeField] private GameObject expandBackground; // 반투명 패널

    [Header("Options")]
    [SerializeField] private bool startClosed = true;

    private bool isOpen;

    private void Awake()
    {
        isOpen = !startClosed;
        ApplyState();
    }

    /// <summary>
    /// 메인 버튼 클릭 시 호출
    /// </summary>
    public void ToggleMenu()
    {
        isOpen = !isOpen;
        ApplyState();
    }

    /// <summary>
    /// 메뉴 열기
    /// </summary>
    public void OpenMenu()
    {
        isOpen = true;
        ApplyState();
    }

    /// <summary>
    /// 메뉴 닫기
    /// </summary>
    public void CloseMenu()
    {
        isOpen = false;
        ApplyState();
    }

    private void ApplyState()
    {
        if (settingsButton != null) settingsButton.SetActive(isOpen);
        if (trophyButton != null) trophyButton.SetActive(isOpen);
        if (sleepButton != null) sleepButton.SetActive(isOpen);
        if (mailButton != null) mailButton.SetActive(isOpen);

        if (expandBackground != null) expandBackground.SetActive(isOpen);
    }
}