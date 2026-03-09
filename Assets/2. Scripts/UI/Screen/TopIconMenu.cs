using UnityEngine;
using UnityEngine.UI;

/*
[승문]
TopIconMenu
- 메인 버튼으로 상단 아이콘 메뉴 펼침/접기
- 열렸을 때: 서브 버튼 + Dim 패널 표시
- 닫혔을 때: 메인 버튼만 표시
- 메인 버튼 아이콘은 햄버거 <-> X로 직접 교체
*/
public class TopIconMenu : MonoBehaviour
{
    [Header("Sub Buttons")]
    [SerializeField] private GameObject settingsButton;
    [SerializeField] private GameObject trophyButton;
    [SerializeField] private GameObject sleepButton;
    [SerializeField] private GameObject mailButton;

    [Header("Background")]
    [SerializeField] private GameObject dimPanel;

    [Header("Main Button Icon")]
    [SerializeField] private Image mainButtonImage;
    [SerializeField] private Sprite closedSprite; // 햄버거
    [SerializeField] private Sprite openedSprite; // X

    [Header("Options")]
    [SerializeField] private bool startClosed = true;

    private bool isOpen;

    private void Awake()
    {
        isOpen = !startClosed;
        ApplyState();
    }

    public void ToggleMenu()
    {
        isOpen = !isOpen;
        ApplyState();
    }

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
        if (dimPanel != null) dimPanel.SetActive(isOpen);

        if (mainButtonImage != null)
            mainButtonImage.sprite = isOpen ? openedSprite : closedSprite;
    }
}