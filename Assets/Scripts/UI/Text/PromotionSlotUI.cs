using TMPro;
using UnityEngine;
using UnityEngine.UI;

/*
[승문]
PromotionSlotUI
-승급 탭 슬롯 전용
-표시:
  등급명
  공격력 xN, 체력 xN
  권장 레벨 기본/Lv000
-아이콘: SlotKeyIconBinder가 '슬롯 순서'대로 넘겨준 Sprite로 세팅
*/
public class PromotionSlotUI : MonoBehaviour
{
    [Header("Key")]
    [SerializeField] private EnumUI.SlotKey key;

    [Header("Icon (Prefab Local)")]
    [SerializeField] private Image iconImage;

    [Header("Texts")]
    [SerializeField] private TMP_Text gradeNameText;
    [SerializeField] private TMP_Text multiText;
    [SerializeField] private TMP_Text recommendText;

    [Header("State UI")]
    [SerializeField] private Button challengeButton;
    [SerializeField] private GameObject completeRoot;
    [SerializeField] private GameObject lockRoot;

    [Header("Format")]
    [SerializeField] private string recommendPrefix = "권장 레벨 ";
    [SerializeField] private string baseLabel = "기본";
    [SerializeField] private bool hideButtonWhenCompleted = true;

    private void Awake()
    {
        ApplyStaticText();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        ApplyStaticText();
    }
#endif

    // 컨테이너 자동 배정에서 호출
    public void SetKey(EnumUI.SlotKey newKey, Sprite iconSprite)
    {
        key = newKey;
        ApplyStaticText();

        if (iconImage != null && iconSprite != null)
        {
            iconImage.sprite = iconSprite;
        }
    }

    private void ApplyStaticText()
    {
        if (gradeNameText != null)
        {
            gradeNameText.text = EnumUITables.GetKoreanName(key);
        }

        EnumUITables.PromotionInfo info = EnumUITables.GetPromotionInfo(key);

        if (multiText != null)
        {
            multiText.text = "공격력 x" + info.growthMul.ToString("N0") + ", 체력 x" + info.growthMul.ToString("N0");
        }

        if (recommendText != null)
        {
            if (info.recommendLevel <= 0)
            {
                recommendText.text = recommendPrefix + baseLabel;
            }
            else
            {
                recommendText.text = recommendPrefix + "Lv" + info.recommendLevel.ToString("N0");
            }
        }
    }

    public void RefreshState(EnumUI.SlotKey currentRankKey)
    {
        int myIdx = EnumUITables.GetPromotionIndex(currentRankKey);
        int slotIdx = EnumUITables.GetPromotionIndex(key);

        if (myIdx < 0 || slotIdx < 0)
        {
            SetLocked(true);
            SetCompleted(false);
            SetChallenge(false);
            return;
        }

        if (slotIdx <= myIdx)
        {
            SetLocked(false);
            SetChallenge(false);
            SetCompleted(true);
            return;
        }

        if (slotIdx == myIdx + 1)
        {
            SetLocked(false);
            SetCompleted(false);
            SetChallenge(true);
            return;
        }

        SetChallenge(false);
        SetCompleted(false);
        SetLocked(true);
    }

    private void SetChallenge(bool on)
    {
        if (challengeButton != null)
        {
            challengeButton.gameObject.SetActive(on);
            challengeButton.interactable = on;
        }
    }

    private void SetCompleted(bool on)
    {
        if (completeRoot != null) completeRoot.SetActive(on);

        if (challengeButton != null)
        {
            if (on)
            {
                if (hideButtonWhenCompleted)
                {
                    challengeButton.gameObject.SetActive(false);
                }
                else
                {
                    challengeButton.gameObject.SetActive(true);
                    challengeButton.interactable = false;
                }
            }
        }

        if (on && lockRoot != null) lockRoot.SetActive(false);
    }

    private void SetLocked(bool on)
    {
        if (lockRoot != null) lockRoot.SetActive(on);

        if (challengeButton != null && on)
        {
            challengeButton.gameObject.SetActive(true);
            challengeButton.interactable = false;
        }

        if (on && completeRoot != null) completeRoot.SetActive(false);
    }
}
