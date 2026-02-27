using UnityEngine;
using TMPro;
using BackEnd;

public class UI_UserInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private string _format = "{0:00#}";

    private void OnEnable()
    {
        LevelManager.Instance.OnLevelUp += Refresh;

        _nicknameText.text = Backend.UserNickName;
    }

    private void OnDisable()
    {
        if (LevelManager.Instance != null)
            LevelManager.Instance.OnLevelUp -= Refresh;
    }

    private void Refresh(int level)
    {
        _levelText.text = string.Format(_format, level);
    }
}
