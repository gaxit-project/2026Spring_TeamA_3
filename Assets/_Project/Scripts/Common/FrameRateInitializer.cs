using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// ゲーム起動時にフレームレート固定（60FPS）およびディスプレイ解像度の最適化を行うクラス
    /// </summary>
    public static class FrameRateInitializer
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeFrameRate()
        {
            // 1. VSync無効化 & 60FPS固定
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = 60;

#if !UNITY_EDITOR
            // 2. ディスプレイのネイティブ解像度を取得してアスペクト比を判別
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
    }
}
