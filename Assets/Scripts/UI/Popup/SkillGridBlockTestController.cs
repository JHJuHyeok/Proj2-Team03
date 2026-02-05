using UnityEngine;
using UnityEngine.UI;

/*
SkillGridBlockTestController
- 버튼으로 블럭 생성 / 회전 테스트용
- 테트리스처럼 90도 회전(좌/우)
- 작동 확인 목적이므로 "칸 기준" 회전만 처리
*/

public class SkillGridBlockTestController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private SkillGridBlockOverlay overlay; //이전에 만든 Overlay 스크립트

    [Header("Test Block Data")]
    [SerializeField] private Vector2Int startCell = new Vector2Int(1, 1);
    [SerializeField] private Vector2Int blockSize = new Vector2Int(2, 3); //가로2, 세로3

    private RectTransform currentBlock;
    private int rotationIndex; //0,1,2,3 -> 0/90/180/270

    //버튼: 생성
    public void SpawnBlock()
    {
        if (overlay == null)
        {
            Debug.LogError("[BlockTest] Overlay not assigned");
            return;
        }

        Clear();

        rotationIndex = 0;
        currentBlock = overlay.SpawnBlock(startCell, blockSize);
    }

    //버튼: 왼쪽 회전(-90)
    public void RotateLeft()
    {
        Rotate(-1);
    }

    //버튼: 오른쪽 회전(+90)
    public void RotateRight()
    {
        Rotate(1);
    }

    private void Rotate(int dir)
    {
        if (currentBlock == null)
        {
            return;
        }

        rotationIndex = (rotationIndex + dir) % 4;
        if (rotationIndex < 0) rotationIndex += 4;

        Vector2Int size = blockSize;

        //90도 / 270도면 가로세로 교체
        if (rotationIndex % 2 == 1)
        {
            size = new Vector2Int(blockSize.y, blockSize.x);
        }

        //기존 블럭 제거 후 다시 생성(검증용이라 단순)
        Clear();
        currentBlock = overlay.SpawnBlock(startCell, size);
    }

    private void Clear()
    {
        if (overlay == null)
        {
            return;
        }

        overlay.ClearBlocks();
        currentBlock = null;
    }
}
