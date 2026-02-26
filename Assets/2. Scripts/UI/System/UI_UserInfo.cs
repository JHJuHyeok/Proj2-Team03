using UnityEngine;
using TMPro;
using BackEnd;

public class UI_UserInfo : MonoBehaviour
{
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _nicknameText;

    private void OnEnable()
    {
        // 나중에 레벨 관련 스크립트 생기면 Event 만들어서 구독

        Refresh();
    }

    public void Refresh()
    {
        _levelText.text = $"Lv.{DataManager.CurrentSaveData.level}";
        _nicknameText.text = Backend.UserNickName;
    }
}
