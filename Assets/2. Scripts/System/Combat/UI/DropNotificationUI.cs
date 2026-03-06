using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Combat.Drop;

namespace Combat.UI
{
    public class DropNotificationUI : MonoBehaviour
    {
        public static DropNotificationUI Instance { get; private set; }

        [SerializeField] private DropNotificationSlot slotPrefab;
        [SerializeField] private int maxSlots = 5;
        [SerializeField] private float slotHeight = 50f;
        [SerializeField] private float scaleStep = 0.1f;
        [SerializeField] private float displayDuration = 3f;
        [SerializeField] private float slideSpeed = 20f;
        [SerializeField] private float fadeOutDuration = 0.5f;

        [Header("Icons")]
        [SerializeField] private Sprite goldIcon;
        [SerializeField] private Sprite expIcon;

        private readonly List<SlotEntry> _activeSlots = new();
        private readonly Queue<DropNotificationSlot> _pool = new();

        private class SlotEntry
        {
            public DropNotificationSlot Slot;
            public int Index; // 0 = bottom, higher = top
            public Coroutine AutoFadeCoroutine;
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public void ShowNotification(DropType type, long amount)
        {
            // Get or create slot
            DropNotificationSlot slot = GetSlotFromPool();
            RectTransform rt = (RectTransform)slot.transform;

            // Set content
            Sprite icon = type == DropType.Gold ? goldIcon : expIcon;
            string label = type == DropType.Gold ? "Gold" : "Exp";
            string text = $"{label} +{amount:N0}";
            slot.Show(icon, text);

            // Start below visible area so it slides up into position
            rt.anchoredPosition = new Vector2(0, -slotHeight);
            rt.localScale = Vector3.one;

            // Push existing slots up
            foreach (var entry in _activeSlots)
                entry.Index++;

            // Remove overflow
            if (_activeSlots.Count >= maxSlots)
            {
                var overflow = _activeSlots[_activeSlots.Count - 1];
                RemoveSlot(overflow);
            }

            // Add new entry at index 0
            var newEntry = new SlotEntry
            {
                Slot = slot,
                Index = 0
            };
            newEntry.AutoFadeCoroutine = StartCoroutine(AutoFadeCoroutine(newEntry));
            _activeSlots.Insert(0, newEntry);
        }

        private void Update()
        {
            // Smoothly move slots to their target positions/scales
            for (int i = 0; i < _activeSlots.Count; i++)
            {
                var entry = _activeSlots[i];
                RectTransform rt = (RectTransform)entry.Slot.transform;

                Vector2 targetPos = new Vector2(0, entry.Index * slotHeight);
                float targetScale = 1f - entry.Index * scaleStep;
                targetScale = Mathf.Max(targetScale, 0.1f);

                float lerpT = Time.deltaTime * slideSpeed;
                rt.anchoredPosition = Vector2.Lerp(rt.anchoredPosition, targetPos, lerpT);
                float currentScale = Mathf.Lerp(rt.localScale.x, targetScale, lerpT);
                rt.localScale = new Vector3(currentScale, currentScale, 1f);
            }
        }

        private IEnumerator AutoFadeCoroutine(SlotEntry entry)
        {
            yield return new WaitForSeconds(displayDuration);

            entry.Slot.FadeOut(fadeOutDuration, () =>
            {
                ReturnSlotToPool(entry.Slot);
                _activeSlots.Remove(entry);
            });
        }

        private void RemoveSlot(SlotEntry entry)
        {
            if (entry.AutoFadeCoroutine != null)
                StopCoroutine(entry.AutoFadeCoroutine);

            entry.Slot.FadeOut(fadeOutDuration, () =>
            {
                ReturnSlotToPool(entry.Slot);
            });
            _activeSlots.Remove(entry);
        }

        private DropNotificationSlot GetSlotFromPool()
        {
            if (_pool.Count > 0)
            {
                var slot = _pool.Dequeue();
                slot.gameObject.SetActive(true);
                return slot;
            }

            var newSlot = Instantiate(slotPrefab, transform);
            return newSlot;
        }

        private void ReturnSlotToPool(DropNotificationSlot slot)
        {
            slot.gameObject.SetActive(false);
            _pool.Enqueue(slot);
        }
    }
}
