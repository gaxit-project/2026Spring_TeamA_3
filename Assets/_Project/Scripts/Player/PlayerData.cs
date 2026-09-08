using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// プレイヤーの共通設定・パラメータ保持用ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerData", menuName = "DeadLab/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Header("Movement Settings")]
        [Tooltip("前進時の常時ダッシュモード有効化フラグ")]
        public bool isSprintModeEnabled = true;

        [Tooltip("ジャンプの初速度（高さ）")]
        public float jumpForce = 5.0f;

        [Header("Look Settings")]
        [Tooltip("視点移動（カメラ操作）の感度倍率")]
        public float lookSensitivity = 1.0f;

        [Header("Health Settings")]
        [Tooltip("プレイヤーの初期最大体力値")]
        public int maxHealth = 100;
    }
}
