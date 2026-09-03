using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// プレイヤーの状態およびパラメータ保持用Modelクラス
    /// </summary>
    public class PlayerModel
    {
        private readonly PlayerData playerData;

        /// <summary>
        /// コンストラクタ（SOデータおよび初期感度設定）
        /// </summary>
        public PlayerModel(PlayerData playerData, float lookSensitivity = 1.0f)
        {
            this.playerData = playerData;
            this.LookSensitivity = lookSensitivity;
        }

        /// <summary>
        /// 移動入力ベクトル
        /// </summary>
        public Vector2 MoveDirection { get; set; } = Vector2.zero;
        
        /// <summary>
        /// 視点移動（カメラ操作）入力ベクトル
        /// </summary>
        public Vector2 LookDirection { get; set; } = Vector2.zero;

        /// <summary>
        /// 射撃ボタン押下フラグ
        /// </summary>
        public bool IsFiring { get; set; } = false;

        /// <summary>
        /// エイム（照準）状態フラグ
        /// </summary>
        public bool IsAiming { get; set; } = false;

        /// <summary>
        /// 常時ダッシュモード有効化状態の取得
        /// </summary>
        public bool IsSprintModeEnabled => playerData != null ? playerData.isSprintModeEnabled : true;

        /// <summary>
        /// 視点移動（カメラ旋回）の感度倍率
        /// </summary>
        public float LookSensitivity { get; set; }
    }
}
