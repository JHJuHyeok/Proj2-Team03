using UnityEngine;
using UnityEngine.UI;

/*
SkillGridCell
- 6x6 보드의 "한 칸"
- 잠금/해금(레벨 조건)과 점유 여부를 관리
- 배치 판정(SkillGridBlockOverlay)이 이 값을 읽어 처리
*/

public class SkillGridCell : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject lockIcon;
    [SerializeField] private Image placedIconImage; // 옵션: 배치된 스킬 아이콘 표시

    [Header("State")]
    [SerializeField] private bool isUnlocked = true;
    [SerializeField] private bool isOccupied;

    public bool IsUnlocked
    {
        get { return isUnlocked; }
    }

    public bool IsOccupied
    {
        get { return isOccupied; }
    }

    // 외부(레벨 시스템/팝업 오픈 시점)에서 호출
    public void SetUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;

        if (lockIcon != null)
        {
            lockIcon.SetActive(!isUnlocked);
        }
    }

    // Overlay에서 점유 처리
    public void SetOccupied(bool occupied)
    {
        isOccupied = occupied;
    }

    // 옵션: 배치된 스킬 아이콘 표시
    public void SetPlacedIcon(Sprite sprite)
    {
        if (placedIconImage == null)
        {
            return;
        }

        placedIconImage.sprite = sprite;
        placedIconImage.enabled = sprite != null;
    }
}
