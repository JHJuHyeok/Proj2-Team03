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
        CurrencyManager.Instance.AddCurrency(type, amount);
        Debug.Log($"{type}À»/¸¦ {amount}¸¸Å­ È¹µæ");
    }
}
