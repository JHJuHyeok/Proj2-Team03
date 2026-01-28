using UnityEngine;

  [RequireComponent(typeof(UnityEngine.UI.ScrollRect))]
  public class MouseWheelScroll : MonoBehaviour
  {
      [SerializeField] private float scrollSpeed = 0.1f;  

      private UnityEngine.UI.ScrollRect scrollRect;

      private void Awake()
      {
          scrollRect = GetComponent<UnityEngine.UI.ScrollRect>();
      }

      private void Update()
      {
          float scrollInput = Input.GetAxis("Mouse ScrollWheel");

          if (Mathf.Abs(scrollInput) > 0.01f)
          {
              scrollRect.verticalNormalizedPosition += scrollInput * scrollSpeed;
              scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
          }
      }
  }