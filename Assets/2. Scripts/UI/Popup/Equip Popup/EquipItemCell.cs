using UnityEngine;
using TMPro;
using UnityEngine.UI;
using SlayerLegend.Equipment;

/*
EquipItemCell
-장비 리스트 셀
-아이콘 / 클릭 처리
*/
public class EquipItemCell : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private Button button;
    [SerializeField] private GameObject selectedFrame;

    private InventoryItem item;
    private System.Action<InventoryItem> onClick;
    private System.Func<string, Sprite> spriteResolver;
    public InventoryItem Item => item;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(OnClick);
        }
    }

    public void Bind(InventoryItem data, System.Action<InventoryItem> click, System.Func<string, Sprite> resolver)
    {
        item = data;
        onClick = click;
        spriteResolver = resolver;

        if (icon != null && data != null)
        {
            icon.sprite = spriteResolver != null ? resolver(data.equipment.spriteName) : null;
        }
    }

    private void OnClick()
    {
        if (item == null) return;

        onClick?.Invoke(item);
    }

    public void SetSelected(bool selected)
    {
        if (selectedFrame != null)
            selectedFrame.SetActive(selected);
    }
}