using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;

namespace DeadLab
{
    /// <summary>
    /// ゲーム起動時にフレームレート固定（60FPS）、解像度最適化、マウスカーソル制御、
    /// およびシーンロードイベント駆動によるUIフォーカス保護を管理する初期化クラス
    /// </summary>
    public static class GameEnvironmentInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeEnvironment()
        {
            // 60FPS固定
            Application.targetFrameRate = 60;

            // マウスカーソルを常に非表示・ロック（コントローラー操作を基本とし、UI誤クリックによるフォーカス外れを防止）
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // イベント駆動: シーンロード時に1度だけEventSystemの背景クリック解除を無効化
            SceneManager.sceneLoaded += OnSceneLoaded;

#if !UNITY_EDITOR
            // ディスプレイのネイティブ解像度を取得してアスペクト比を判別
            Resolution currentRes = Screen.currentResolution;
            float aspectRatio = (float)currentRes.width / currentRes.height;

            int targetWidth;
            int targetHeight;

            if (aspectRatio >= 1.7f)
            {
                // 16:9 ディスプレイの場合（1920 x 1080）
                targetWidth = 1920;
                targetHeight = 1080;
            }
            else if (aspectRatio >= 1.55f && aspectRatio < 1.7f)
            {
                // 16:10 ディスプレイの場合（1920 x 1200）
                targetWidth = 1920;
                targetHeight = 1200;
            }
            else
            {
                // その他の比率（高さ1080を基準に比率維持）
                targetHeight = 1080;
                targetWidth = Mathf.RoundToInt(1080f * aspectRatio);
            }

            // モニター解像度がターゲットより高解像度（4K, WQHD等）の場合のみ負荷軽減スケーリング
            // FullScreenWindow（ボーダーレス全画面）により黒帯なしで全画面表示
            if (currentRes.width > targetWidth || currentRes.height > targetHeight)
            {
                Screen.SetResolution(targetWidth, targetHeight, FullScreenMode.FullScreenWindow);
            }
#endif
        }

        private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // シーン内のEventSystemを取得し、マウス等のクリック・ポインター入力をUIモジュールから完全切断
            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem != null && eventSystem.currentInputModule is InputSystemUIInputModule inputModule)
            {
                inputModule.deselectOnBackgroundClick = false;
                inputModule.leftClick = null;
                inputModule.rightClick = null;
                inputModule.middleClick = null;
                inputModule.point = null;
            }
        }
    }
}
