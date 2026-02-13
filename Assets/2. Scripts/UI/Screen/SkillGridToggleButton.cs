using UnityEngine;

/*
[승문]
SkillGridToggleButton
-스킬탭에서 버튼 클릭 시 스킬 그리드 패널 토글
*/
public class SkillGridToggleButton : MonoBehaviour
{
    [SerializeField] private SkillGridPanelController gridPanel;

    public void ToggleGrid()
    {
        if (gridPanel == null) return;
        gridPanel.Toggle();
    }

    public void OpenGrid()
    {
        if (gridPanel == null) return;
        gridPanel.Show();
    }

    public void CloseGrid()
    {
        if (gridPanel == null) return;
        gridPanel.Hide();
    }
}
