using System;
using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// 入力(View)とアセットキャラクター(Character)の仲介用Presenterクラス
    /// </summary>
    public class PlayerPresenter : IDisposable
    {
        private readonly IPlayerInputView view;
        private readonly PlayerModel model;
        private readonly InfimaGames.LowPolyShooterPack.Character character;
        private readonly GameManager gameManager;

        /// <summary>
        /// コンストラクタ（依存オブジェクト設定およびイベント購読）
        /// </summary>
        public PlayerPresenter(IPlayerInputView view, PlayerModel model, InfimaGames.LowPolyShooterPack.Character character, GameManager gameManager)
        {
            this.view = view;
            this.model = model;
            this.character = character;
            this.gameManager = gameManager;

            // リロードイベントの購読
            this.view.OnReloadRequested += OnReloadRequested;
        }

        private bool CanAcceptInput()
        {
            // ポーズ中は入力を受け付けない
            if (PausePresenter.IsPaused) return false;

            if (gameManager == null) return true;

            // 通常プレイ中は入力可能
            if (gameManager.CurrentState == GameManager.GameState.Playing) return true;

            // ゲームクリア後も、シャッターが降り始める（入力ブロック開始）までは操作可能
            if (gameManager.CurrentState == GameManager.GameState.GameClear)
            {
                if (ShutterTransitionUI.Instance != null && (ShutterTransitionUI.Instance.IsInputBlocked || ShutterTransitionUI.Instance.IsTransitioning))
                {
                    return false;
                }
                return true;
            }

            return false;
        }

        /// <summary>
        /// フレーム毎の入力同期および状態更新（Bootstrapper等から実行）
        /// </summary>
        public void Update()
        {
            if (character == null) return;

            // 入力受付可否の判定
            if (!CanAcceptInput())
            {
                model.MoveDirection = Vector2.zero;
                model.LookDirection = Vector2.zero;
                model.IsFiring = false;
                model.IsAiming = false;
            }
            else
            {
                // Viewから入力読み取りおよびModelへの反映
                model.MoveDirection = view.MoveInput;
                model.LookDirection = view.LookInput * model.LookSensitivity;
                model.IsFiring = view.IsFiring;
                model.IsAiming = view.IsAiming;
            }

            // アセットCharacterへの入力データ注入
            character.SetInputMovement(model.MoveDirection);
            character.SetInputLook(model.LookDirection);
            character.SetHoldingButtonFire(model.IsFiring);
            character.SetHoldingButtonAim(model.IsAiming);

            // 常時ダッシュ制御（前進入力時の自動ダッシュ切り替え）
            if (model.IsSprintModeEnabled && model.MoveDirection.y > 0.01f)
            {
                character.SetHoldingButtonRun(true);
            }
            else
            {
                character.SetHoldingButtonRun(false);
            }
        }

        /// <summary>
        /// リロード要求受信時の処理
        /// </summary>
        private void OnReloadRequested()
        {
            // 入力受付不可時はリロード要求無視
            if (!CanAcceptInput()) return;

            if (character != null)
            {
                // アセットキャラクターの実行リロード呼び出し
                character.TryReload();
            }
        }

        /// <summary>
        /// リソース破棄およびイベント購読解除
        /// </summary>
        public void Dispose()
        {
            if (view != null)
            {
                view.OnReloadRequested -= OnReloadRequested;
            }
        }
    }
}
