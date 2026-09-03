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

        /// <summary>
        /// フレーム毎の入力同期および状態更新（Bootstrapper等から実行）
        /// </summary>
        public void Update()
        {
            if (character == null) return;

            // ゲームプレイ中以外の入力無効化
            if (gameManager != null && gameManager.CurrentState != GameManager.GameState.Playing)
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
            // ゲームプレイ中以外のリロード要求無視
            if (gameManager != null && gameManager.CurrentState != GameManager.GameState.Playing) return;

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
