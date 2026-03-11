using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Adventure Panel 프리팹에 부착하는 매니저
/// 지역 버튼(AdventureAreaButtonUI)을 초기화하고, 클릭 시 AdventurePopupUI를 열어줌
/// </summary>
public class AdventureUIManager : MonoBehaviour
{
    [Header("지역 버튼 (Map Panel > Adventure Button 0~5)")]
    [SerializeField] private AdventureAreaButtonUI[] areaButtons;

    [Header("팝업 (별도 프리팹)")]
    [SerializeField] private AdventurePopupUI adventurePopup;

    private List<AreaData> areaList;

    private void Start()
    {
        areaList = DataManager.adventures.GetAll();
        InitializeAreaButtons();
    }

    private void InitializeAreaButtons()
    {
        if (areaList == null || areaButtons == null) return;

        for (int i = 0; i < areaButtons.Length; i++)
        {
            if (areaButtons[i] == null) continue;

            if (i < areaList.Count)
            {
                var areaData = areaList[i];
                int areaIndex = i;

                areaButtons[i].gameObject.SetActive(true);
                areaButtons[i].SetData(areaData);
                areaButtons[i].SetOnClickAction(() => OpenPopup(areaIndex));
            }
            else
            {
                areaButtons[i].gameObject.SetActive(false);
            }
        }
    }

    public void OpenPopup(int areaIndex)
    {
        if (areaList == null || areaIndex < 0 || areaIndex >= areaList.Count) return;

        if (adventurePopup != null)
            adventurePopup.Open(areaList[areaIndex]);
    }

    public void ClosePopup()
    {
        if (adventurePopup != null)
            adventurePopup.Close();
    }
}
