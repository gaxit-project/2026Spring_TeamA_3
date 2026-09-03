using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace DeadLab
{
    /// <summary>
    /// PlayerInputコンポーネント経由の入力取得およびIPlayerInputView実装クラス
    /// </summary>
    [RequireComponent(typeof(PlayerInput))]
    public class PlayerInputView : MonoBehaviour, IPlayerInputView
    {
        private PlayerInput playerInput;
        private InputAction moveAction;
        private InputAction lookAction;
        private InputAction fireAction;
        private InputAction aimAction;
        private InputAction reloadAction;

        public Vector2 MoveInput { get; private set; }
        public Vector2 LookInput { get; private set; }
        public bool IsFiring { get; private set; }
        public bool IsAiming { get; private set; }

        public event Action OnReloadRequested;

        private void Awake()
        {
            playerInput = GetComponent<PlayerInput>();
            
            // アセット（Low Poly Shooter Pack）のキー割り当てアクション名との紐付け
            moveAction = playerInput.actions.FindAction("Movement", false);
            lookAction = playerInput.actions.FindAction("Look", false);
            fireAction = playerInput.actions.FindAction("Fire", false);
            aimAction = playerInput.actions.FindAction("Aiming", false);
            reloadAction = playerInput.actions.FindAction("Reload", false);
        }

        private void OnEnable()
        {
            if (reloadAction != null)
            {
                reloadAction.started += OnReloadPerformed;
            }
        }

        private void OnDisable()
        {
            if (reloadAction != null)
            {
                reloadAction.started -= OnReloadPerformed;
            }
        }

        /// <summary>
        /// リロード入力検出時のコールバック関数
        /// </summary>
        private void OnReloadPerformed(InputAction.CallbackContext context)
        {
            OnReloadRequested?.Invoke();
        }

        private void Update()
        {
            // 移動入力ベクトルの更新
            if (moveAction != null)
            {
                MoveInput = moveAction.ReadValue<Vector2>();
            }

            // 視点移動入力ベクトルの更新
            if (lookAction != null)
            {
                LookInput = lookAction.ReadValue<Vector2>();
            }

            // 射撃入力状態の更新（ホールド対応）
            if (fireAction != null)
            {
                IsFiring = fireAction.IsPressed();
            }

            // エイム入力状態の更新（ホールド対応）
            if (aimAction != null)
            {
                IsAiming = aimAction.IsPressed();
            }
        }
    }
}
