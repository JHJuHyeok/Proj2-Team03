using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SlayerLegend.Skill.Data;
using SlayerLegend.Resource;

namespace SlayerLegend.Skill.UI.Grid
{
    // 인벤토리 슬롯 UI
    // 개별 스킬 아이템 표시
    public class InventorySlotUI : MonoBehaviour
    {
        [Header("UI 요소")]
        [SerializeField] private Image iconImage;
        [SerializeField] private Image background;
        [SerializeField] private Image shapeIndicator;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI sizeText;
        [SerializeField] private Button slotButton;

        [Header("색상 설정")]
        [SerializeField] private Color activeColor = new Color(0.3f, 0.6f, 0.9f, 1f);
        [SerializeField] private Color passiveColor = new Color(0.6f, 0.4f, 0.8f, 1f);
        [SerializeField] private Color selectedColor = new Color(0.9f, 0.7f, 0.2f, 1f);

        private SkillData skillData;
        private SkillDraggableItem draggableItem;
        private bool isSelected = false;

        public event System.Action<InventorySlotUI> OnSlotClicked;

        public SkillData SkillData => skillData;
        public SkillDraggableItem DraggableItem => draggableItem;
        public bool IsSelected => isSelected;

        private void Awake()
        {
            if (slotButton != null)
            {
                slotButton.onClick.AddListener(HandleSlotClicked);
            }
        }

        private void OnDestroy()
        {
            if (slotButton != null)
            {
                slotButton.onClick.RemoveListener(HandleSlotClicked);
            }
        }

        // 초기화
        public void Initialize(SkillData data, SkillDraggableItem item)
        {
            skillData = data;
            draggableItem = item;

            UpdateUI();
        }

        // 드래그 아이템 설정 (슬롯 클릭 후 아이템 생성 시 호출)
        public void SetDraggableItem(SkillDraggableItem item)
        {
            draggableItem = item;
        }

        // UI 업데이트
        public void UpdateUI()
        {
            if (skillData == null) return;

            // 아이콘
            if (iconImage != null)
            {
                Sprite sprite = ResourceManager.Instance?.LoadSprite(skillData.spriteName);
                if (sprite != null)
                {
                    iconImage.sprite = sprite;
                }
            }

            // 이름
            if (nameText != null)
            {
                nameText.text = skillData.name;
            }

            // 크기 표시
            if (sizeText != null)
            {
                sizeText.text = skillData.GetGridSizeDescription();
            }

            // 배경색 (액티브/패시브)
            if (background != null)
            {
                background.color = skillData.type == SkillType.Active ? activeColor : passiveColor;
            }

            // 모양 표시기 (시각적 힌트)
            UpdateShapeIndicator();
        }

        // 모양 표시기 업데이트
        private void UpdateShapeIndicator()
        {
            if (shapeIndicator == null || skillData == null) return;

            var shapeType = skillData.GetShapeType();
            var shapeData = SkillShapeData.Create(shapeType);

            // 모양에 따른 크기 조정
            float baseSize = 30f;
            shapeIndicator.rectTransform.sizeDelta = new Vector2(
                shapeData.baseSize.x * baseSize / 2,
                shapeData.baseSize.y * baseSize / 2
            );
        }

        // 선택 상태 설정
        public void SetSelected(bool selected)
        {
            isSelected = selected;

            if (background != null)
            {
                if (selected)
                {
                    background.color = selectedColor;
                }
                else
                {
                    if (skillData == null)
                    {
                        background.color = passiveColor;
                        return;
                    }
                    background.color = skillData.type == SkillType.Active ? activeColor : passiveColor;
                }
            }
        }

        // 슬롯 클릭 핸들러
        private void HandleSlotClicked()
        {
            OnSlotClicked?.Invoke(this);
        }

        // 아이템이 그리드에 배치되었는지 확인
        public bool IsOnGrid()
        {
            return draggableItem != null && draggableItem.IsOnGrid;
        }
    }
}
