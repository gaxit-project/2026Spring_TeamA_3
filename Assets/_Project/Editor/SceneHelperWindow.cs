using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace DeadLab.Editor
{
    /// <summary>
    /// Build Settingsに登録されたシーンへワンタッチで移動できるユーティリティを提供するエディタウィンドウ
    /// </summary>
    public class SceneHelperWindow : EditorWindow
    {
        [MenuItem("Tools/Scene Helper")]
        public static void ShowWindow()
        {
            // ウィンドウを取得または新規表示
            GetWindow<SceneHelperWindow>("Scene Helper");
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUILayout.Label("シーンのクイック切り替え (Build Settings Scenes)", EditorStyles.boldLabel);
            GUILayout.Space(10);

            // Build Settings に登録されている有効なシーンを動的に取得してボタンを表示
            EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
            bool hasValidScenes = false;

            if (scenes != null && scenes.Length > 0)
            {
                foreach (EditorBuildSettingsScene scene in scenes)
                {
                    if (scene.enabled && !string.IsNullOrEmpty(scene.path))
                    {
                        hasValidScenes = true;
                        // パスから拡張子なしのファイル名（シーン名）を抽出
                        string sceneName = System.IO.Path.GetFileNameWithoutExtension(scene.path);

                        if (GUILayout.Button($"{sceneName} を開く", GUILayout.Height(35)))
                        {
                            OpenScene(scene.path);
                        }
                        GUILayout.Space(5);
                    }
                }
            }

            if (!hasValidScenes)
            {
                EditorGUILayout.HelpBox("Build Settings に有効なシーンが登録されていません。\nFile ➔ Build Settings からシーンを登録してください。", MessageType.Warning);
            }

            GUILayout.FlexibleSpace();
        }

        /// <summary>
        /// 編集中の変更保存を確認したうえで、安全にシーンを開きます
        /// </summary>
        private void OpenScene(string path)
        {
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("[SceneHelper] 再生中はシーンを切り替えることはできません。");
                return;
            }

            // 変更の保存を確認するポップアップを出し、ユーザーがキャンセルしなかった場合のみ開く
            if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                EditorSceneManager.OpenScene(path);
                Debug.Log($"[SceneHelper] シーンをロードしました: {path}");
            }
        }
    }
}
