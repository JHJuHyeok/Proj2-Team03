using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Combat.UI
{
    public class DropNotificationSlot : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private TextMeshProUGUI amountText;
        [SerializeField] private CanvasGroup canvasGroup;

        private Coroutine _fadeCoroutine;

        public void Show(Sprite iconSprite, string text)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
                _fadeCoroutine = null;
            }

            icon.sprite = iconSprite;
            amountText.text = text;
            canvasGroup.alpha = 1f;
            gameObject.SetActive(true);
        }

        public void FadeOut(float duration, System.Action onComplete = null)
        {
            if (_fadeCoroutine != null)
                StopCoroutine(_fadeCoroutine);

            _fadeCoroutine = StartCoroutine(FadeOutCoroutine(duration, onComplete));
        }

        private IEnumerator FadeOutCoroutine(float duration, System.Action onComplete)
        {
            float elapsed = 0f;
            float startAlpha = canvasGroup.alpha;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
            _fadeCoroutine = null;
            onComplete?.Invoke();
        }
    }
}
