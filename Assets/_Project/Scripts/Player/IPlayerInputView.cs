using System;
using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// プレイヤーの入力イベントおよび入力データ提供用インターフェース
    /// </summary>
    public interface IPlayerInputView
    {
        /// <summary>
        /// 移動入力ベクトルの取得
        /// </summary>
        Vector2 MoveInput { get; }

        /// <summary>
        /// 視点移動（カメラ操作）入力ベクトルの取得
        /// </summary>
        Vector2 LookInput { get; }

        /// <summary>
        /// 射撃ボタン押下状態の取得
        /// </summary>
        bool IsFiring { get; }

        /// <summary>
        /// エイムボタン押下状態の取得
        /// </summary>
        bool IsAiming { get; }

        /// <summary>
        /// リロード要求イベント
        /// </summary>
        event Action OnReloadRequested;

        /// <summary>
        /// ジャンプ要求イベント
        /// </summary>
        event Action OnJumpRequested;
    }
}
