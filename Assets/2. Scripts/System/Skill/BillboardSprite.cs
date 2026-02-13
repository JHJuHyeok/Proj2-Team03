using UnityEngine;

namespace SlayerLegend.Skill
{
    // 2D 스프라이트가 항상 카메라를 바라보도록 함
    public class BillboardSprite : MonoBehaviour
    {
        private Camera mainCamera;

        private void Start()
        {
            mainCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (mainCamera == null) return;

            // 카메라 방향으로 회전
            transform.LookAt(mainCamera.transform);

            // 2D 스프라이트를 위해 180도 회전 (뒤집힘 방지)
            transform.Rotate(0, 180, 0);
        }
    }
}
