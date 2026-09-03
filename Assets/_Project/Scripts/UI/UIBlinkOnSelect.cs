using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace DeadLab
{
    /// <summary>
    /// フォーカス・ホバー時UIカラー点滅表示制御用クラス
    /// </summary>
    [RequireComponent(typeof(Graphic))]
    public class UIBlinkOnSelect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [Header("色の点滅設定")]
        [Tooltip("点滅スピード")]
        [SerializeField] private float blinkSpeed = 4.0f;

        private Graphic targetGraphic;
        private bool isHoveredOrSelected = false;

        private void Awake()
        {
            targetGraphic = GetComponent<Graphic>();
            
            if (targetGraphic != null)
            {
                targetGraphic.color = Color.white;
            }
        }

        private void Update()
        {
            if (targetGraphic == null) return;

            if (isHoveredOrSelected)
            {
                float t = (Mathf.Sin(Time.time * blinkSpeed) + 1.0f) / 2.0f;
                targetGraphic.color = Color.Lerp(new Color(0.8f, 0.8f, 0.8f), Color.white, t);
            }
            else
            {
                targetGraphic.color = Color.white;
            }
        }

        public void OnPointerEnter(PointerEventData eventData) => isHoveredOrSelected = true;
        public void OnPointerExit(PointerEventData eventData) => isHoveredOrSelected = false;
        public void OnSelect(BaseEventData eventData) => isHoveredOrSelected = true;
        public void OnDeselect(BaseEventData eventData) => isHoveredOrSelected = false;
    }
}
