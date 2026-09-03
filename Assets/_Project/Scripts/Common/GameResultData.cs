namespace DeadLab
{
    /// <summary>
    /// シーン間（ゲームプレイ〜リザルト）での結果データ保持用静的クラス
    /// </summary>
    public static class GameResultData
    {
        public static bool IsClear { get; set; }
        public static int KillCount { get; set; }
        public static int Score { get; set; }
        public static string StageName { get; set; } = "";
        public static string LastPlaySceneName { get; set; } = "TestScene";

        /// <summary>
        /// 自己ベスト更新フラグ
        /// </summary>
        public static bool IsNewRecord { get; set; }

        /// <summary>
        /// クリア時の残り時間（秒）
        /// </summary>
        public static float RemainingTime { get; set; }

        /// <summary>
        /// クリア所要時間（秒）
        /// </summary>
        public static float ClearTime { get; set; }

        /// <summary>
        /// クリア目標撃破数
        /// </summary>
        public static int TargetKillCount { get; set; }

        /// <summary>
        /// リザルト評価ランク（S, A, B, C, D）
        /// </summary>
        public static string Rank { get; set; } = "D";

        /// <summary>
        /// 選択ステージデータアセット
        /// </summary>
        public static StageData SelectedStageData { get; set; }
        public static StageData[] AllStages { get; set; }
        public static int CurrentStageIndex { get; set; }

        /// <summary>
        /// タイトル移行時のステージ詳細直開きフラグ
        /// </summary>
        public static bool ShowStageDetailOnLoad { get; set; }

        /// <summary>
        /// リザルトデータのリセット処理
        /// </summary>
        public static void Reset()
        {
            IsClear = false;
            IsNewRecord = false;
            KillCount = 0;
            Score = 0;
            StageName = "";
            RemainingTime = 0f;
            ClearTime = 0f;
            TargetKillCount = 0;
            Rank = "D";
            ShowStageDetailOnLoad = false;
        }
    }
}
