using BackEnd;
using SlayerLegend.Equipment;
using SlayerLegend.Resource;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
[승문]
PlayerInfoPopup
- 현재 플레이어 정보 / 최종 스탯을 UI에 표시
- 스탯은 TMP_Text 하나로 출력
- 레벨: DataManager.CurrentSaveData
- 닉네임: Backend.UserNickName
- 최종 스탯: StatManager
- 장착 무기 / 악세: EquipmentManager
*/
public class PlayerInfoPopup : MonoBehaviour
{
    [Header("Top Info")]
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private TMP_Text nicknameText;

    [Header("Equip Info")]
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private TMP_Text accessoryNameText;
    [SerializeField] private TMP_Text weaponGradeText;
    [SerializeField] private TMP_Text accessoryGradeText;
    [SerializeField] private Image weaponIconImage;
    [SerializeField] private Image accessoryIconImage;

    [Header("Stats")]
    [SerializeField] private TMP_Text statText;

    [Header("Optional")]
    [SerializeField] private Button refreshButton;

    [Header("Equip Sprite")]
    [SerializeField] private EquipmentSpriteDatabase equipmentSpriteDatabase;

    private void Awake()
    {
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshUI);
        }
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        RefreshBasicInfo();
        RefreshEquipInfo();
        RefreshStats();
    }

    private void RefreshBasicInfo()
    {
        if (DataManager.CurrentSaveData != null)
        {
            SetText(levelText, "Lv." + DataManager.CurrentSaveData.level);
        }
        else
        {
            SetText(levelText, "Lv.0");
        }

        SetText(nicknameText, string.IsNullOrEmpty(Backend.UserNickName) ? "-" : Backend.UserNickName);
    }

    private void RefreshEquipInfo()
    {
        EquipmentManager eq = EquipmentManager.Instance;
        if (eq == null)
        {
            ClearEquipInfo();
            return;
        }

        // 무기
        EquipData weaponData = eq.GetEquippedData(EquipType.Weapon);
        if (weaponData != null)
        {
            SetText(weaponNameText, weaponData.name);

            int weaponGrade = eq.GetLevel(weaponData.id);
            SetText(weaponGradeText, weaponGrade + "등급");

            SetImage(weaponIconImage, GetEquipSprite(weaponData));
        }
        else
        {
            SetText(weaponNameText, "-");
            SetText(weaponGradeText, "");
            SetImage(weaponIconImage, null);
        }

        // 악세
        EquipData accessoryData = eq.GetEquippedData(EquipType.Accessorie);
        if (accessoryData != null)
        {
            SetText(accessoryNameText, accessoryData.name);

            int accessoryGrade = eq.GetLevel(accessoryData.id);
            SetText(accessoryGradeText, accessoryGrade + "등급");

            SetImage(accessoryIconImage, GetEquipSprite(accessoryData));
        }
        else
        {
            SetText(accessoryNameText, "-");
            SetText(accessoryGradeText, "");
            SetImage(accessoryIconImage, null);
        }
    }

    private void RefreshStats()
    {
        if (statText == null)
        {
            return;
        }

        StatManager stat = StatManager.Instance;
        if (stat == null)
        {
            statText.text = "-";
            return;
        }

        StringBuilder valueSb = new StringBuilder();

        valueSb.AppendLine(FormatNumber(stat.GetStat(StatType.STR)));
        valueSb.AppendLine(FormatNumber(stat.GetStat(StatType.HP)));
        valueSb.AppendLine(FormatNumber(stat.GetStat(StatType.VIT_HP)));
        valueSb.AppendLine(FormatPercent(stat.GetStat(StatType.CRI_Per)));
        valueSb.AppendLine(FormatPercent(stat.GetStat(StatType.CRI_DMG)));
        valueSb.AppendLine(FormatNumber(stat.GetStat(StatType.MANA)));
        valueSb.AppendLine(FormatNumber(stat.GetStat(StatType.VIT_MANA)));
        valueSb.AppendLine(FormatNumber(stat.GetStat(StatType.ACC)));
        valueSb.AppendLine(FormatNumber(stat.GetStat(StatType.DODGE)));
        valueSb.AppendLine(FormatPercent(stat.GetStat(StatType.ADD_GOLD)));
        valueSb.AppendLine(FormatPercent(stat.GetStat(StatType.ADD_EXP)));

        statText.text = valueSb.ToString();
    }

    private Sprite GetEquipSprite(EquipData data)
    {
        if (data == null || string.IsNullOrEmpty(data.spriteName))
        {
            return null;
        }

        if (equipmentSpriteDatabase != null)
        {
            Sprite dbSprite = equipmentSpriteDatabase.GetSprite(data.spriteName);
            if (dbSprite != null)
            {
                return dbSprite;
            }
        }

        Sprite sp = Resources.Load<Sprite>(data.spriteName);
        return sp;
    }

    private void ClearEquipInfo()
    {
        SetText(weaponNameText, "-");
        SetText(accessoryNameText, "-");
        SetText(weaponGradeText, "");
        SetText(accessoryGradeText, "");

        SetImage(weaponIconImage, null);
        SetImage(accessoryIconImage, null);
    }

    private void SetText(TMP_Text target, string value)
    {
        if (target != null)
        {
            target.text = value;
        }
    }

    private void SetImage(Image target, Sprite sprite)
    {
        if (target != null)
        {
            target.sprite = sprite;
            target.enabled = sprite != null;
        }
    }

    private string FormatNumber(double value)
    {
        return value.ToString("N0");
    }

    private string FormatPercent(double value)
    {
        return value.ToString("0.##") + "%";
    }
}