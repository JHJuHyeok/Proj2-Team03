using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using BackEnd;

public class InfoProvider : MonoBehaviour
{
    [Header("유저 정보")]
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private Image _rankImage;

    [Header("재화 정보")]
    [SerializeField] private TMP_Text _jewelText;
    [SerializeField] private TMP_Text _diamondText;

    private static bool _isUpdateActive = false;
    private Action _infoAction;

    private void Awake()
    {
        Init();
    }

    /// <summary>
    /// 이벤트에 변경되는 정보 표시 UI 할당
    /// </summary>
    public void Init()
    {
        _infoAction = () =>
        {
            _levelText.text = DataManager.CurrentSaveData.level.ToString();
            _nicknameText.text = Backend.UserNickName;
            //_rankImage.sprite = SpriteManager.GetSprite("");

            _jewelText.text = CurrencyManager.Instance.GetAmount(CurrencyType.Emerald).ToString();
            _diamondText.text = CurrencyManager.Instance.GetAmount(CurrencyType.Diamond).ToString();
        };
    }

    // 정보 변경 시 갱신
    private void Update()
    {
        if (_isUpdateActive)
        {
            _isUpdateActive = false;
            try
            {
                _infoAction.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"UI 정보 전달에 실패했습니다. {e}");
            }
        }
    }

    /// <summary>
    /// 정보 변경 시 호출
    /// </summary>
    public static void UpdateUserData()
    {
        _isUpdateActive = true;
    }
}
