using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DeadLab
{
    /// <summary>
    /// UIフォーカス追従型カード自動スクロール制御用クラス
    /// </summary>
    [RequireComponent(typeof(ScrollRect))]
    public class StageCardScrollOnSelect : MonoBehaviour
    {
        [Header("スクロール設定")]
        [Tooltip("スクロール所要時間（秒）")]
        [SerializeField] private float scrollDuration = 0.2f;

        [Tooltip("画面端からのマージン（ピクセル）")]
        [SerializeField] private float padding = 50f;

        [Header("矢印UI設定")]
        [Tooltip("左スクロールインジケーター（左矢印UI）")]
        [SerializeField] private GameObject leftArrow;
        [Tooltip("右スクロールインジケーター（右矢印UI）")]
        [SerializeField] private GameObject rightArrow;

        private ScrollRect scrollRect;
        private RectTransform viewportRect;
        private RectTransform contentRect;

        private GameObject lastSelectedObject;
        private Coroutine scrollCoroutine;

        private void Awake()
        {
            scrollRect = GetComponent<ScrollRect>();
            viewportRect = scrollRect.viewport != null ? scrollRect.viewport : (RectTransform)scrollRect.transform;
            contentRect = scrollRect.content;
        }

        private void Start()
        {
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
            }

            // 初期状態の矢印表示更新
            UpdateArrowVisibility();
        }

        private void OnDestroy()
        {
            if (scrollRect != null)
            {
                scrollRect.onValueChanged.RemoveListener(OnScrollValueChanged);
            }
        }

        private void OnEnable()
        {
            UpdateArrowVisibility();
        }

        /// <summary>
        /// スクロール位置変更時のイベントハンドラ
        /// </summary>
        private void OnScrollValueChanged(Vector2 position)
        {
            UpdateArrowVisibility();
        }

        /// <summary>
        /// スクロール位置に基づく左右矢印の表示・非表示制御
        /// </summary>
        public void UpdateArrowVisibility()
        {
            if (viewportRect == null || contentRect == null) return;

            float viewportWidth = viewportRect.rect.width;
            float contentWidth = contentRect.rect.width;

            // スクロール可能な領域が存在しない（全カードが収まっている）場合は両方非表示
            if (contentWidth <= viewportWidth + 1f)
            {
                if (leftArrow != null) leftArrow.SetActive(false);
                if (rightArrow != null) rightArrow.SetActive(false);
                return;
            }

            float currentX = contentRect.anchoredPosition.x;
            float maxScrollX = contentWidth - viewportWidth;
            float threshold = 5f; // 端判定の遊び（ピクセル）

            // 左側に隠れているコンテンツがある（左へスクロール可能）
            bool canScrollLeft = currentX < -threshold;

            // 右側に隠れているコンテンツがある（右へスクロール可能）
            bool canScrollRight = currentX > -maxScrollX + threshold;

            if (leftArrow != null) leftArrow.SetActive(canScrollLeft);
            if (rightArrow != null) rightArrow.SetActive(canScrollRight);
        }

        private void Update()
        {
            if (EventSystem.current == null) return;

            GameObject currentSelected = EventSystem.current.currentSelectedGameObject;
            if (currentSelected != null && currentSelected != lastSelectedObject)
            {
                lastSelectedObject = currentSelected;

                if (currentSelected.transform.IsChildOf(contentRect))
                {
                    RectTransform targetRect = currentSelected.GetComponent<RectTransform>();
                    if (targetRect != null)
                    {
                        UpdateScrollPosition(targetRect);
                    }
                }
            }
        }

        /// <summary>
        /// 選択オブジェクトの表示領域内スクロール位置補正処理
        /// </summary>
        private void UpdateScrollPosition(RectTransform target)
        {
            if (viewportRect == null || contentRect == null) return;

            float viewportWidth = viewportRect.rect.width;
            float viewportLeft = -viewportWidth * viewportRect.pivot.x;
            float viewportRight = viewportWidth * (1f - viewportRect.pivot.x);

            Vector3 targetLocalInViewport = viewportRect.InverseTransformPoint(target.position);
            
            float targetLeftInViewport = targetLocalInViewport.x - (target.rect.width * target.pivot.x);
            float targetRightInViewport = targetLocalInViewport.x + (target.rect.width * (1f - target.pivot.x));

            float diff = 0f;

            if (targetLeftInViewport < viewportLeft + padding)
            {
                diff = (viewportLeft + padding) - targetLeftInViewport;
            }
            else if (targetRightInViewport > viewportRight - padding)
            {
                diff = (viewportRight - padding) - targetRightInViewport;
            }
            else
            {
                return;
            }

            float targetContentX = contentRect.anchoredPosition.x + diff;

            float maxScroll = Mathf.Max(0f, contentRect.rect.width - viewportWidth);
            targetContentX = Mathf.Clamp(targetContentX, -maxScroll, 0f);

            if (scrollCoroutine != null)
            {
                StopCoroutine(scrollCoroutine);
            }
            scrollCoroutine = StartCoroutine(AnimateScroll(targetContentX));
        }

        /// <summary>
        /// スムーズスクロール移動ルーチン
        /// </summary>
        private IEnumerator AnimateScroll(float targetX)
        {
            Vector2 startPos = contentRect.anchoredPosition;
            Vector2 targetPos = new Vector2(targetX, startPos.y);
            float elapsed = 0f;

            while (elapsed < scrollDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / scrollDuration;
                t = Mathf.SmoothStep(0f, 1f, t);
                
                contentRect.anchoredPosition = Vector2.Lerp(startPos, targetPos, t);
                UpdateArrowVisibility();
                yield return null;
            }

            contentRect.anchoredPosition = targetPos;
            UpdateArrowVisibility();
            scrollCoroutine = null;
        }
    }
}
