using UnityEngine;

namespace DeadLab
{
    /// <summary>
    /// シーン開始時のMVP初期化およびアセットキャラクター統合用ブートストラップクラス
    /// </summary>
    public class PlayerBootstrapper : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("アセットのプレイヤーキャラクター（Characterコンポーネント）")]
        [SerializeField] private InfimaGames.LowPolyShooterPack.Character character;
        [Tooltip("プレイヤーの各種設定・パラメータデータ(ScriptableObject)")]
        [SerializeField] private PlayerData playerData;

        private PlayerInputView inputView;
        private PlayerModel playerModel;
        private PlayerPresenter playerPresenter;

        private void Start()
        {
            // character未設定時の自動取得（自GameObject優先、未発見時はシーン検索）
            if (character == null)
            {
                character = GetComponent<InfimaGames.LowPolyShooterPack.Character>()
                         ?? FindFirstObjectByType<InfimaGames.LowPolyShooterPack.Character>();
            }

            if (character == null)
            {
                Debug.LogError("[PlayerBootstrapper] アセットの Character がアタッチされておらず、シーン上にも見つかりません。");
                return;
            }

            // Character.cs の標準入力無効化（外部入力注入モード有効化）
            character.useExternalInput = true;

            // PlayerInputView の動的アタッチ（または取得）
            inputView = character.GetComponent<PlayerInputView>();
            if (inputView == null)
            {
                inputView = character.gameObject.AddComponent<PlayerInputView>();
            }

            // Model生成およびPresenter接続
            if (playerData == null)
            {
                playerData = ScriptableObject.CreateInstance<PlayerData>();
            }

            // 保存済み設定値の取得（未保存時はPlayerDataの初期値を使用）
            float defaultSensitivity = playerData != null ? playerData.lookSensitivity : 1.0f;
            float initialSensitivity = PlayerPrefs.GetFloat("Sensitivity", defaultSensitivity);

            playerModel = new PlayerModel(playerData, initialSensitivity);
            GameManager gameManager = GameManager.Instance ?? FindFirstObjectByType<GameManager>();
            playerPresenter = new PlayerPresenter(inputView, playerModel, character, gameManager);

            Debug.Log("[PlayerBootstrapper] MVP制御によるプレイヤー初期化が完了しました。");
        }

        private void Update()
        {
            // フレーム毎の入力読み込みおよびキャラクターデータ注入
            if (playerPresenter != null)
            {
                playerPresenter.Update();
            }
        }

        private void OnDestroy()
        {
            // 購読イベント解除
            if (playerPresenter != null)
            {
                playerPresenter.Dispose();
            }
        }
    }
}
