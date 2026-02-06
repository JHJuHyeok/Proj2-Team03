using UnityEngine;
using UnityEngine.UI;

/*
SkillGridBlockTestController
- Spawn / Left / Right 버튼으로 테스트
- 현재 블록 모양(SkillBlockData)을 보관
- DraggableUI는 여기서 현재 모양만 받아서 배치 시도
*/

public class SkillGridBlockTestController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SkillGridBlockOverlay overlay;
    [SerializeField] private DraggableUI draggable;

    [Header("Preview UI (Optional)")]
    [SerializeField] private Image previewImage;
    [SerializeField] private Sprite previewSprite;

    [Header("Block Data")]
    [SerializeField] private SkillBlockData currentData;

    private void Awake()
    {
        if (draggable != null)
        {
            draggable.SetRefs(overlay, this);
        }

        // 기본 블록(예시): 2x3
        if (currentData == null)
        {
            currentData = new SkillBlockData();
            currentData.cells.Add(new Vector2Int(0, 0));
            currentData.cells.Add(new Vector2Int(1, 0));
            currentData.cells.Add(new Vector2Int(0, 1));
            currentData.cells.Add(new Vector2Int(1, 1));
            currentData.cells.Add(new Vector2Int(0, 2));
            currentData.cells.Add(new Vector2Int(1, 2));
        }

        RefreshPreview();
    }

    public SkillBlockData GetCurrentBlockData()
    {
        return currentData;
    }

    public void SpawnBlock()
    {
        // 여기서 랜덤 블록으로 바꾸고 싶으면 currentData를 교체하면 됨
        RefreshPreview();
    }

    public void RotateLeft()
    {
        if (currentData == null)
        {
            return;
        }

        currentData.RotateCCW();
        RefreshPreview();
    }

    public void RotateRight()
    {
        if (currentData == null)
        {
            return;
        }

        currentData.RotateCW();
        RefreshPreview();
    }

    public void OnPlacedSuccessfully()
    {
        // 배치 성공 후 동작(예: 다음 블록 자동 생성)
        // SpawnBlock();
    }

    private void RefreshPreview()
    {
        if (previewImage == null)
        {
            return;
        }

        previewImage.sprite = previewSprite;
        previewImage.enabled = previewSprite != null;
    }
}
