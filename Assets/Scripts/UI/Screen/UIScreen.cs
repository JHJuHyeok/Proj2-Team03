using UnityEngine;

/*[승문]
UIScreen
-로비/전투HUD/결과 같은 큰 화면의 베이스
-UIManager가 Show/Hide를 호출
*/
public abstract class UIScreen : MonoBehaviour
{
    public virtual void OnShow(object param)
    {
        gameObject.SetActive(true);
    }

    public virtual void OnHide()
    {
        gameObject.SetActive(false);
    }
}
