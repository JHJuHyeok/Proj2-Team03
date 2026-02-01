using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/*
ScrollScript
-자식(세로) ScrollRect 전용 스크립트
-세로 드래그만 허용하고 가로 드래그는 무시
-가로 페이지 이동은 버튼 클릭으로만 처리
*/
public class ScrollScript : ScrollRect
{
    [SerializeField] private float lockThreshold = 8f;//방향 고정 임계치(픽셀)

    private bool decided;//방향이 확정됐는지
    private bool allowVertical;//세로 드래그 허용 여부
    private Vector2 accum;//누적 드래그

    //드래그 시작
    public override void OnBeginDrag(PointerEventData eventData)
    {
        decided = false;
        allowVertical = false;
        accum = Vector2.zero;

        base.OnBeginDrag(eventData);
    }

    //드래그 중
    public override void OnDrag(PointerEventData eventData)
    {
        accum += eventData.delta;

        if (!decided && accum.magnitude >= lockThreshold)
        {
            //세로가 더 크면 세로 스크롤 허용, 가로가 더 크면 무시
            allowVertical = Mathf.Abs(accum.y) >= Mathf.Abs(accum.x);
            decided = true;
        }

        //방향이 아직 확정 전이면(임계치 전) 자식 스크롤이 튀지 않게 일단 무시
        if (!decided) return;

        //가로로 확정되면 계속 무시
        if (!allowVertical) return;

        base.OnDrag(eventData);
    }

    //드래그 종료
    public override void OnEndDrag(PointerEventData eventData)
    {
        //가로로 확정된 경우 자식 종료도 호출하지 않는다
        if (decided && !allowVertical) return;

        base.OnEndDrag(eventData);
    }
}
