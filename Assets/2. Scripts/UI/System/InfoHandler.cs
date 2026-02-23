using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using BackEnd;

public class InfoHandler : MonoBehaviour
{
    [Header("유저 정보")]
    [SerializeField] private TMP_Text _levelText;
    [SerializeField] private TMP_Text _nicknameText;
    [SerializeField] private Image _rankImage;

    [Header("재화 정보")]
    [SerializeField] private TMP_Text _jewelText;
    [SerializeField] private TMP_Text _diamondText;

    [Header("재화 획득 버튼")]
    [SerializeField] private Button[] getCurrButtons;

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

    public void AddCurrency(CurrencyType type, double amount)
    {
        CurrencyManager.Instance.AddCurrency(type, amount);
    }
    #region 재화 획득 함수
    public void GetDiamond3500() => AddCurrency(CurrencyType.Diamond, 3500);
    public void GetDiamond6000() => AddCurrency(CurrencyType.Diamond, 6000);
    public void GetDiamond9900() => AddCurrency(CurrencyType.Diamond, 9900);
    public void GetDiamond30000() => AddCurrency(CurrencyType.Diamond, 30000);
    public void GetDiamond60000() => AddCurrency(CurrencyType.Diamond, 60000);
    public void GetDiamond100000() => AddCurrency(CurrencyType.Diamond, 100000);
    public void GetEmerald3500() => AddCurrency(CurrencyType.Emerald, 3500);
    public void GetEmerald6000() => AddCurrency(CurrencyType.Emerald, 6000);
    public void GetEmerald9900() => AddCurrency(CurrencyType.Emerald, 9900);
    public void GetEmerald30000() => AddCurrency(CurrencyType.Emerald, 30000);
    public void GetEmerald60000() => AddCurrency(CurrencyType.Emerald, 60000);
    public void GetEmerald100000() => AddCurrency(CurrencyType.Emerald, 100000);
    #endregion

    /// <summary>
    /// 정보 변경 시 호출
    /// </summary>
    public static void UpdateUserData()
    {
        _isUpdateActive = true;
    }
}
