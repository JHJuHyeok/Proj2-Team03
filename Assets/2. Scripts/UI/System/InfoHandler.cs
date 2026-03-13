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
    public void GetDiamond1400() => AddCurrency(CurrencyType.Diamond, 1400);
    public void GetDiamond2700() => AddCurrency(CurrencyType.Diamond, 2700);
    public void GetDiamond5000() => AddCurrency(CurrencyType.Diamond, 5000);
    public void GetDiamond20000() => AddCurrency(CurrencyType.Diamond, 20000);
    public void GetDiamond40000() => AddCurrency(CurrencyType.Diamond, 40000);
    public void GetDiamond80000() => AddCurrency(CurrencyType.Diamond, 80000);
    public void GetEmerald350() => AddCurrency(CurrencyType.Emerald, 350);
    public void GetEmerald620() => AddCurrency(CurrencyType.Emerald, 620);
    public void GetEmerald1200() => AddCurrency(CurrencyType.Emerald, 1200);
    public void GetEmerald3800() => AddCurrency(CurrencyType.Emerald, 3800);
    public void GetEmerald7500() => AddCurrency(CurrencyType.Emerald, 7500);
    public void GetEmerald15000() => AddCurrency(CurrencyType.Emerald, 15000);
    #endregion

    /// <summary>
    /// 정보 변경 시 호출
    /// </summary>
    public static void UpdateUserData()
    {
        _isUpdateActive = true;
    }
}
