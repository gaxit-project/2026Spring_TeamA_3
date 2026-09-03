using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// ステージ別自己ベスト記録データ単位
    /// </summary>
    [Serializable]
    public class StageRecord
    {
        public string stageName;
        public float bestClearTime = 9999f;
        public string bestRank = "-";
        public bool isCleared = false;
    }

    /// <summary>
    /// JSONシリアライズ用セーブデータコンテナ
    /// </summary>
    [Serializable]
    public class GameSaveDataContainer
    {
        public List<StageRecord> records = new List<StageRecord>();
    }

    /// <summary>
    /// JSONファイルを使用したセーブデータ保存・読み込み管理用静的クラス
    /// </summary>
    public static class SaveDataManager
    {
        /// <summary>
        /// セーブデータJSONファイル保存パスの取得（SaveData/save_data.json）
        /// </summary>
        private static string GetSaveFilePath()
        {
            string dirPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../SaveData"));
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
            return Path.Combine(dirPath, "save_data.json");
        }

        /// <summary>
        /// 全セーブデータのJSONロード処理
        /// </summary>
        public static GameSaveDataContainer LoadAllData()
        {
            string filePath = GetSaveFilePath();

            if (!File.Exists(filePath))
            {
                return new GameSaveDataContainer();
            }

            try
            {
                string json = File.ReadAllText(filePath);
                if (string.IsNullOrEmpty(json))
                {
                    return new GameSaveDataContainer();
                }

                GameSaveDataContainer data = JsonUtility.FromJson<GameSaveDataContainer>(json);
                return data ?? new GameSaveDataContainer();
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveDataManager] セーブデータJSONの読み込み失敗: {ex.Message}");
                return new GameSaveDataContainer();
            }
        }

        /// <summary>
        /// 全セーブデータのJSON保存処理
        /// </summary>
        public static void SaveAllData(GameSaveDataContainer data)
        {
            if (data == null) return;

            try
            {
                string filePath = GetSaveFilePath();
                string json = JsonUtility.ToJson(data, true);
                File.WriteAllText(filePath, json);
                Debug.Log($"[SaveDataManager] セーブデータを保存しました: {filePath}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[SaveDataManager] セーブデータJSONの保存失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 指定ステージの記録データ取得
        /// </summary>
        public static StageRecord GetRecord(string stageName)
        {
            if (string.IsNullOrEmpty(stageName)) return null;

            GameSaveDataContainer container = LoadAllData();
            return container.records.Find(r => r.stageName == stageName);
        }

        /// <summary>
        /// クリア結果記録および自己ベスト更新判定・保存処理
        /// </summary>
        public static bool SaveClearRecord(string stageName, float clearTime, string rank)
        {
            if (string.IsNullOrEmpty(stageName)) return false;

            GameSaveDataContainer container = LoadAllData();
            StageRecord record = container.records.Find(r => r.stageName == stageName);

            bool isNewRecord = false;

            float roundedClearTime = Mathf.Round(clearTime);

            if (record == null)
            {
                record = new StageRecord
                {
                    stageName = stageName,
                    bestClearTime = roundedClearTime,
                    bestRank = rank,
                    isCleared = true
                };
                container.records.Add(record);
                isNewRecord = true;
            }
            else
            {
                record.isCleared = true;

                if (roundedClearTime < record.bestClearTime || record.bestClearTime <= 0f)
                {
                    record.bestClearTime = roundedClearTime;
                    isNewRecord = true;
                }

                if (IsHigherRank(rank, record.bestRank))
                {
                    record.bestRank = rank;
                }
            }

            SaveAllData(container);
            return isNewRecord;
        }

        /// <summary>
        /// ランク文字の優劣比較判定
        /// </summary>
        private static bool IsHigherRank(string rankA, string rankB)
        {
            int scoreA = GetRankScore(rankA);
            int scoreB = GetRankScore(rankB);
            return scoreA > scoreB;
        }

        private static int GetRankScore(string rank)
        {
            switch (rank)
            {
                case "S": return 5;
                case "A": return 4;
                case "B": return 3;
                case "C": return 2;
                case "D": return 1;
                default: return 0;
            }
        }
    }
}
