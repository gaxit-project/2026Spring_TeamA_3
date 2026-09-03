using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

namespace DeadLab
{
    /// <summary>
    /// 設定画面（音量・感度スライダーおよび保存）ロジック制御用クラス
    /// </summary>
    public class SettingsManager : MonoBehaviour
    {
        public static AudioMixerGroup SeMixerGroupInstance { get; private set; }

        [Header("Audio Configuration")]
        [Tooltip("音量制御用AudioMixer")]
        [SerializeField] private AudioMixer audioMixer;
        [Tooltip("SE用AudioMixerGroup")]
        [SerializeField] private AudioMixerGroup seMixerGroup;

        [Header("BGM Volume Settings")]
        [SerializeField] private Slider bgmVolumeSlider;
        [SerializeField] private TextMeshProUGUI bgmVolumeValueText;

        [Header("SE Volume Settings")]
        [SerializeField] private Slider seVolumeSlider;
        [SerializeField] private TextMeshProUGUI seVolumeValueText;

        [Header("Sensitivity Settings")]
        [SerializeField] private Slider sensitivitySlider;
        [SerializeField] private TextMeshProUGUI sensitivityValueText;

        [Header("UI References")]
        [SerializeField] private Button backButton;

        public event Action OnBackRequested;

        private void Start()
        {
            if (seMixerGroup != null)
            {
                SeMixerGroupInstance = seMixerGroup;
            }

            // AudioManagerService へのSE用ミキサーグループ自動適用
            var amService = InfimaGames.LowPolyShooterPack.ServiceLocator.Current.Get<InfimaGames.LowPolyShooterPack.IAudioManagerService>() as InfimaGames.LowPolyShooterPack.AudioManagerService;
            if (amService != null && seMixerGroup != null)
            {
                amService.SetOutputMixerGroup(seMixerGroup);
            }

            if (backButton != null)
            {
                backButton.onClick.AddListener(() => OnBackRequested?.Invoke());
            }

            // BGM音量スライダーの初期化
            if (bgmVolumeSlider != null)
            {
                float savedBgm = PlayerPrefs.GetFloat("BGMVolume", 1.0f);
                bgmVolumeSlider.value = savedBgm;
                bgmVolumeSlider.onValueChanged.AddListener(OnBgmVolumeChanged);
                UpdateBgmVolumeText(savedBgm);
                SetAudioMixerVolume("BGMVolume", savedBgm);
            }

            // SE音量スライダーの初期化
            if (seVolumeSlider != null)
            {
                float savedSe = PlayerPrefs.GetFloat("SEVolume", 1.0f);
                seVolumeSlider.value = savedSe;
                seVolumeSlider.onValueChanged.AddListener(OnSeVolumeChanged);
                UpdateSeVolumeText(savedSe);
                SetAudioMixerVolume("SEVolume", savedSe);
            }

            // 感度スライダーの初期化
            if (sensitivitySlider != null)
            {
                sensitivitySlider.minValue = 0.1f;
                sensitivitySlider.maxValue = 5.0f;
                float savedSens = PlayerPrefs.GetFloat("Sensitivity", 1.0f);
                sensitivitySlider.value = savedSens;
                sensitivitySlider.onValueChanged.AddListener(OnSensitivityChanged);
                UpdateSensitivityText(savedSens);
            }
        }

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

        /// <summary>
        /// AudioMixerパラメータへのデシベル値変換および適用処理
        /// </summary>
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
            PlayerPrefs.SetFloat("Sensitivity", val);
            PlayerPrefs.Save();
            UpdateSensitivityText(val);
        }

        private void UpdateSensitivityText(float val)
        {
            if (sensitivityValueText != null)
            {
                sensitivityValueText.text = $"{val:F1}";
            }
        }
    }
}
