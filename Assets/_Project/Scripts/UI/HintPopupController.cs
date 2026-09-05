using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadLab
{
    /// <summary>
    /// ステージ開始時のヒント表示および時間停止制御クラス
    /// シャッターの奥で表示待機し、シャッター開放後にAボタン等の入力で非表示＆時間再開します。
    /// </summary>
    [RequireComponent(typeof(Canvas))]
    public class HintPopupController : MonoBehaviour
    {
        [Header("SE設定")]
        [Tooltip("閉じる時の決定SE")]
        [SerializeField] private AudioClip closeSeClip;

        private Canvas canvas;
        private bool isClosed = false;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            // シャッター（sortingOrder=9999）の奥、通常UIの手前に設定
            if (canvas != null)
            {
                canvas.sortingOrder = 100;
            }

            // シャッター上昇前から奥で待機・表示させておく
            gameObject.SetActive(true);

            // ゲーム時間を停止（BGMはTime.timeScale非依存のため再生継続）
            Time.timeScale = 0f;
        }

        private void Start()
        {
            // インスペクターで未設定の場合、ResourcesまたはButtonSE互換のSE取得
            if (closeSeClip == null)
            {
                closeSeClip = Resources.Load<AudioClip>("決定ボタン");
            }
        }

        private void Update()
        {
            if (isClosed) return;

            // シャッター開閉中に入力を誤認しないよう、シャッター開放中のみ受付
            if (ShutterTransitionUI.Instance != null && ShutterTransitionUI.Instance.IsInputBlocked)
            {
                return;
            }

            if (CheckInput())
            {
                CloseHint();
            }
        }

        /// <summary>
        /// Aボタン（コントローラー）、Space/Enter（キーボード）、マウスクリック入力を検知
        /// </summary>
        private bool CheckInput()
        {
            // 1. ゲームパッド Aボタン (buttonSouth)
            if (Gamepad.current != null)
            {
                if (Gamepad.current.buttonSouth.wasPressedThisFrame ||
                    Gamepad.current.aButton.wasPressedThisFrame)
                {
                    return true;
                }
            }

            // 2. キーボード Space / Enter
            if (Keyboard.current != null)
            {
                if (Keyboard.current.spaceKey.wasPressedThisFrame ||
                    Keyboard.current.enterKey.wasPressedThisFrame ||
                    Keyboard.current.numpadEnterKey.wasPressedThisFrame)
                {
                    return true;
                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return))
                {
                    return true;
                }
            }

            // 3. マウスクリック
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// ヒントを閉じてゲーム時間を再開
        /// </summary>
        public void CloseHint()
        {
            if (isClosed) return;
            isClosed = true;

            // 決定SEの再生
            if (closeSeClip != null)
            {
                AudioSource.PlayClipAtPoint(closeSeClip, Camera.main != null ? Camera.main.transform.position : Vector3.zero);
            }

            // Canvasを非表示
            gameObject.SetActive(false);

            // ゲーム時間を再開
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            // 万が一破棄された場合に時間が止まったままになるのを防止
            if (Time.timeScale == 0f)
            {
                Time.timeScale = 1f;
            }
        }
    }
}
