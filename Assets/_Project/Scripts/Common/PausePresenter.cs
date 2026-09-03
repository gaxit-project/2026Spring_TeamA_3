using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace DeadLab
{
    /// <summary>
    /// ポーズ入力処理およびゲーム一時停止制御用プレゼンタークラス
    /// </summary>
    public class PausePresenter : MonoBehaviour
    {
        [SerializeField] private PauseView view;
        [SerializeField] private InputActionReference pauseAction;

        private bool isPaused = false;

        private void Start()
        {
            if (view == null) return;

            view.OnResumeClicked += ResumeGame;
            view.OnRetryClicked += RetryGame;
            view.OnSettingsClicked += OpenSettings;
            view.OnTitleClicked += GoToTitle;
            view.OnReturnClicked += CloseSettings;
            view.OnTutorialClicked += OpenTutorial;
            view.OnTutorialReturnClicked += CloseTutorial;
        }

        private void OnEnable()
        {
            if (pauseAction != null)
            {
                pauseAction.action.Enable();
                pauseAction.action.performed += OnPauseActionPerformed;
            }
        }

        private void OnDisable()
        {
            if (pauseAction != null)
            {
                pauseAction.action.performed -= OnPauseActionPerformed;
            }
        }

        private void OnPauseActionPerformed(InputAction.CallbackContext context)
        {
            TogglePause();
        }

        private void TogglePause()
        {
            if (!isPaused)
            {
                if (ShutterTransitionUI.Instance != null && ShutterTransitionUI.Instance.IsInputBlocked)
                {
                    return;
                }
            }

            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }

        private void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;

            EnsureEventSystem();

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            if (view != null) view.Show();
        }

        private void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            if (view != null) view.Hide();
        }

        private void EnsureEventSystem()
        {
            if (EventSystem.current == null)
            {
                EventSystem existing = FindFirstObjectByType<EventSystem>();
                if (existing != null)
                {
                    existing.enabled = true;
                }
            }
            else
            {
                EventSystem.current.enabled = true;
            }
        }

        private void RetryGame()
        {
            Time.timeScale = 1f;
            string sceneToLoad = string.IsNullOrEmpty(GameResultData.LastPlaySceneName) ? "TestScene" : GameResultData.LastPlaySceneName;

            ShutterTransitionUI shutter = ShutterTransitionUI.EnsureExists();
            shutter.CloseShutter(() =>
            {
                SceneManager.LoadScene(sceneToLoad);
            });
        }

        private void OpenSettings()
        {
            if (pauseAction != null) pauseAction.action.Disable();
            if (view != null) view.OpenAudioSettings();
        }

        private void CloseSettings()
        {
            if (pauseAction != null) pauseAction.action.Enable();
            if (view != null) view.CloseAudioSettings();
        }

        private void OpenTutorial()
        {
            if (pauseAction != null) pauseAction.action.Disable();
            if (view != null) view.OpenTutorial();
        }

        private void CloseTutorial()
        {
            if (pauseAction != null) pauseAction.action.Enable();
            if (view != null) view.CloseTutorial();
        }

        private void GoToTitle()
        {
            Time.timeScale = 1f;

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            ShutterTransitionUI shutter = ShutterTransitionUI.EnsureExists();
            shutter.CloseShutter(() =>
            {
                SceneManager.LoadScene("TitleScene");
            });
        }
    }
}
