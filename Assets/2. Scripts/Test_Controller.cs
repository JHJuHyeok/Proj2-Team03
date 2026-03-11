using UnityEngine;

public class Test_Controller : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.G)) AddCurrencyTest(CurrencyType.Gold);
        if (Input.GetKeyDown(KeyCode.C)) AddCurrencyTest(CurrencyType.Cube);
        if (Input.GetKeyDown(KeyCode.D)) AddCurrencyTest(CurrencyType.Diamond);
        if (Input.GetKeyDown(KeyCode.E)) AddCurrencyTest(CurrencyType.Emerald);
        if (Input.GetKeyDown(KeyCode.F)) AddCurrencyTest(CurrencyType.Feather);
        if (Input.GetKeyDown(KeyCode.S)) AddCurrencyTest(CurrencyType.StatPoint);
    }

    private void AddCurrencyTest(CurrencyType type, double amount = 1000)
    {
        // 디버그: CurrencyManager 상태 확인
        if (CurrencyManager.Instance == null)
        {
            Debug.LogError("[Test_Controller] CurrencyManager.Instance가 null입니다!");
            return;
        }

        // 디버그: 추가 전 현재 값 확인
        double beforeAmount = CurrencyManager.Instance.GetAmount(type);
        Debug.Log($"[Test_Controller] {type} 추가 전: {beforeAmount}");

        CurrencyManager.Instance.AddCurrency(type, amount);

        // 디버그: 추가 후 값 확인
        double afterAmount = CurrencyManager.Instance.GetAmount(type);
        Debug.Log($"[Test_Controller] {type} 추가 후: {afterAmount} (획득: {amount})");
    }
}
