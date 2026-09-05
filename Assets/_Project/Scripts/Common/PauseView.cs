using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using TMPro;

namespace DeadLab
{
    /// <summary>
    /// ポーズメニューUIパネル表示・操作イベント発行用クラス
    /// </summary>
    public class PauseView : MonoBehaviour
    {
        [Header("Panels & Selection")]
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private GameObject audioSettingsPanel;
        [SerializeField] private GameObject tutorialPanel;
        [SerializeField] private GameObject defaultSelectedButton;
        
        [Header("Audio Settings")]
        [Tooltip("音量制御用AudioMixer")]
        [SerializeField] private AudioMixer audioMixer;

        [Header("BGM Volume Settings")]
        [SerializeField] private Slider bgmSettingSlider;
        [SerializeField] private TextMeshProUGUI bgmVolumeValueText;

        [Header("SE Volume Settings")]
        [SerializeField] private Slider seSettingSlider;
        [SerializeField] private TextMeshProUGUI seVolumeValueText;

        [Header("Sensitivity Settings")]
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private TextMeshProUGUI sensitivityValueText;

        public event Action OnResumeClicked;
        public event Action OnRetryClicked;
        public event Action OnSettingsClicked;
        public event Action OnTitleClicked;
        public event Action OnReturnClicked;
        public event Action OnTutorialClicked;
        public event Action OnTutorialReturnClicked;

        [Header("Buttons")]
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button retryButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private Button returnToPauseMenuButton;
        [SerializeField] private Button tutorialButton;
        [SerializeField] private Button returnFromTutorialButton;

        private void Awake()
        {
            if (resumeButton != null) resumeButton.onClick.AddListener(() => OnResumeClicked?.Invoke());
            if (retryButton != null) retryButton.onClick.AddListener(() => OnRetryClicked?.Invoke());
            if (settingsButton != null) settingsButton.onClick.AddListener(() => OnSettingsClicked?.Invoke());
            if (quitButton != null) quitButton.onClick.AddListener(() => OnTitleClicked?.Invoke());
            if (returnToPauseMenuButton != null) returnToPauseMenuButton.onClick.AddListener(() => OnReturnClicked?.Invoke());
            if (tutorialButton != null) tutorialButton.onClick.AddListener(() => OnTutorialClicked?.Invoke());
            if (returnFromTutorialButton != null) returnFromTutorialButton.onClick.AddListener(() => OnTutorialReturnClicked?.Invoke());

            if (bgmSettingSlider != null)
            {
                float savedBgm = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
                bgmSettingSlider.value = savedBgm;
                bgmSettingSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
                UpdateBgmVolumeText(savedBgm);
                SetAudioMixerVolume("BGMVolume", savedBgm);
            }

            if (seSettingSlider != null)
            {
                float savedSe = PlayerPrefs.GetFloat("SEVolume", 1.0f);
                seSettingSlider.value = savedSe;
                seSettingSlider.onValueChanged.AddListener(OnSeVolumeChanged);
                UpdateSeVolumeText(savedSe);
                SetAudioMixerVolume("SEVolume", savedSe);
            }

            if (sensitivitySlider != null)
            {
                sensitivitySlider.minValue = 0.5f;
                sensitivitySlider.maxValue = 5.0f;
                float savedSens = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
                sensitivitySlider.value = savedSens;
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
                UpdateSensitivityText(savedSens);
            }

            Hide();
        }

        public void Show()
        {
            if (pausePanel != null) pausePanel.SetActive(true);

            if (EventSystem.current != null)
            {
                EventSystem.current.enabled = true;
                if (defaultSelectedButton != null)
                {
                    EventSystem.current.SetSelectedGameObject(null);
                    EventSystem.current.SetSelectedGameObject(defaultSelectedButton);
                }
            }
        }

        public void Hide()
        {
            if (pausePanel != null) pausePanel.SetActive(false);
        }

        public void OpenAudioSettings()
        {
            if (audioSettingsPanel != null) audioSettingsPanel.SetActive(true);
            Hide();

            if (bgmSettingSlider != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(bgmSettingSlider.gameObject);
            }
        }

        public void CloseAudioSettings()
        {
            if (audioSettingsPanel != null) audioSettingsPanel.SetActive(false);
            Show();
        }

        public void OpenTutorial()
        {
            if (tutorialPanel != null) tutorialPanel.SetActive(true);
            Hide();

            if (returnFromTutorialButton != null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(returnFromTutorialButton.gameObject);
            }
        }

        public void CloseTutorial()
        {
            if (tutorialPanel != null) tutorialPanel.SetActive(false);
            Show();
        }

        #region Audio Volume Control

        private void OnBgmVolumeChanged(float val)
        {
            PlayerPrefs.SetFloat("BGMVolume", val);
            PlayerPrefs.Save();
            UpdateBgmVolumeText(val);
            SetAudioMixerVolume("BGMVolume", val);
        }

        private void UpdateBgmVolumeText(float val)
        {
            if (bgmVolumeValueText != null)
            {
                bgmVolumeValueText.text = $"{Mathf.RoundToInt(val * 100)}%";
            }
        }

        private void OnSeVolumeChanged(float val)
        {
            PlayerPrefs.SetFloat("SEVolume", val);
            PlayerPrefs.Save();
            UpdateSeVolumeText(val);
            SetAudioMixerVolume("SEVolume", val);
        }

        private void UpdateSeVolumeText(float val)
        {
            if (seVolumeValueText != null)
            {
                seVolumeValueText.text = $"{Mathf.RoundToInt(val * 100)}%";
            }
        }

        private void SetAudioMixerVolume(string exposedParamName, float sliderValue)
        {
            if (audioMixer != null)
            {
                float decibel = Mathf.Log10(Mathf.Max(sliderValue, 0.0001f)) * 20f;
                audioMixer.SetFloat(exposedParamName, decibel);
            }
        }

        private void OnSensitivityChanged(float val)
        {
            float snapped = Mathf.Round(val * 2f) / 2f;
            snapped = Mathf.Clamp(snapped, 0.5f, 5.0f);

            if (sensitivitySlider != null && Mathf.Abs(sensitivitySlider.value - snapped) > 0.001f)
            {
                sensitivitySlider.value = snapped;
                return;
            }

            PlayerPrefs.SetFloat("Sensitivity", snapped);
            PlayerPrefs.Save();
            UpdateSensitivityText(snapped);
        }

        private void UpdateSensitivityText(float val)
        {
            if (sensitivityValueText != null)
            {
                sensitivityValueText.text = $"{val:F1}";
            }
        }

        #endregion
    }
}
