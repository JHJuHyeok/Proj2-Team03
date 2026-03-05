using UnityEngine;
using UnityEngine.UI;

/*
[승문]
ShopSummonButton
-팝업 내부 뽑기 버튼 공용 스크립트
-횟수(1/11/33)만 전달
-현재 탭은 ShopPopupContentController가 ShopSummonSystem에 SetCurrentTab으로 동기화해줌
*/
[RequireComponent(typeof(Button))]
public class ShopSummonButton : MonoBehaviour
{
    [SerializeField] private int summonCount = 1;//1,11,33
    [SerializeField] private MonoBehaviour providerBehaviour;//ShopSummonSystem 드래그(선택)

    private Button btn;
    private ShopSummonSystem summonSystem;

    private void Awake()
    {
        btn = GetComponent<Button>();
        summonSystem = ResolveSummonSystem();

        if (summonSystem == null)
        {
            Debug.LogError("[ShopSummonButton] ShopSummonSystem not found.");
        }

        if (btn != null)
        {
            btn.onClick.AddListener(OnClick);
        }
    }

    //버튼 클릭 처리
    private void OnClick()
    {
        if (summonSystem == null)
        {
            summonSystem = ResolveSummonSystem();
            if (summonSystem == null)
            {
                Debug.LogError("[ShopSummonButton] ShopSummonSystem not found on click.");
                return;
            }
        }

        if (summonCount != 1 && summonCount != 11 && summonCount != 33)
        {
            Debug.LogWarning("[ShopSummonButton] Invalid summonCount.");
            return;
        }

        bool ok = summonSystem.TryBuyBatch(summonCount);
        Debug.Log(ok ? "[ShopSummonButton] Summon ok." : "[ShopSummonButton] Summon fail.");
    }

    //SummonSystem 확정(인스펙터 우선)
    private ShopSummonSystem ResolveSummonSystem()
    {
        if (providerBehaviour != null)
        {
            ShopSummonSystem sys0 = providerBehaviour as ShopSummonSystem;
            if (sys0 != null)
            {
                return sys0;
            }

            Debug.LogError("[ShopSummonButton] providerBehaviour is not ShopSummonSystem.");
        }

        //씬에서 자동 탐색(비활성 포함,씬 오브젝트만)
        MonoBehaviour[] all = Resources.FindObjectsOfTypeAll<MonoBehaviour>();
        for (int i = 0; i < all.Length; i++)
        {
            MonoBehaviour mb = all[i];
            if (mb == null)
            {
                continue;
            }

            if (mb.gameObject.scene.IsValid() == false)
            {
                continue;
            }

            ShopSummonSystem sys = mb as ShopSummonSystem;
            if (sys != null)
            {
                return sys;
            }
        }

        return null;
    }
}