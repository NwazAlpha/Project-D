using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame.Foundation.Manager;
using PuzzleGame.Module;

namespace PuzzleGame.Domain.UI.SettingsUI
{
    /// <summary>
    /// 설정 패널 컨트롤러
    /// </summary>
    public class SettingsPanel : MonoBehaviour
    {
        [Header("Panel")]
        [SerializeField] private GameObject _panel;
        
        [Header("Toggles")]
        [SerializeField] private Toggle _bgmToggle;
        [SerializeField] private Toggle _sfxToggle;
        
        [Header("Buttons")]
        [SerializeField] private Button _resetDataButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private Button _closeButton;
        
        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI _bgmLabel;
        [SerializeField] private TextMeshProUGUI _sfxLabel;
        
        private const string PREF_BGM_ENABLED = "Settings_BGM_Enabled";
        private const string PREF_SFX_ENABLED = "Settings_SFX_Enabled";
        
        private void Awake()
        {
            // 초기 상태 로드
            LoadSettings();
            
            // 이벤트 연결
            if (_bgmToggle != null)
                _bgmToggle.onValueChanged.AddListener(OnBGMToggleChanged);
            
            if (_sfxToggle != null)
                _sfxToggle.onValueChanged.AddListener(OnSFXToggleChanged);
            
            if (_resetDataButton != null)
                _resetDataButton.onClick.AddListener(OnResetDataClicked);
            
            if (_quitButton != null)
                _quitButton.onClick.AddListener(OnQuitClicked);
            
            if (_closeButton != null)
                _closeButton.onClick.AddListener(Hide);
        }
        
        private void LoadSettings()
        {
            bool bgmEnabled = PlayerPrefs.GetInt(PREF_BGM_ENABLED, 1) == 1;
            bool sfxEnabled = PlayerPrefs.GetInt(PREF_SFX_ENABLED, 1) == 1;
            
            if (_bgmToggle != null)
                _bgmToggle.isOn = bgmEnabled;
            
            if (_sfxToggle != null)
                _sfxToggle.isOn = sfxEnabled;
            
            UpdateLabels();
        }
        
        private void UpdateLabels()
        {
            if (_bgmLabel != null)
                _bgmLabel.text = _bgmToggle != null && _bgmToggle.isOn ? "BGM: ON" : "BGM: OFF";
            
            if (_sfxLabel != null)
                _sfxLabel.text = _sfxToggle != null && _sfxToggle.isOn ? "SFX: ON" : "SFX: OFF";
        }
        
        private void OnBGMToggleChanged(bool isOn)
        {
            PlayerPrefs.SetInt(PREF_BGM_ENABLED, isOn ? 1 : 0);
            PlayerPrefs.Save();
            UpdateLabels();
            LogHelper.Log("Settings", $"BGM: {(isOn ? "ON" : "OFF")}");
            
            // TODO: 실제 오디오 시스템 연동
            // AudioManager.Instance?.SetBGMEnabled(isOn);
        }
        
        private void OnSFXToggleChanged(bool isOn)
        {
            PlayerPrefs.SetInt(PREF_SFX_ENABLED, isOn ? 1 : 0);
            PlayerPrefs.Save();
            UpdateLabels();
            LogHelper.Log("Settings", $"SFX: {(isOn ? "ON" : "OFF")}");
            
            // TODO: 실제 오디오 시스템 연동
            // AudioManager.Instance?.SetSFXEnabled(isOn);
        }
        
        private void OnResetDataClicked()
        {
            LogHelper.Log("Settings", "Reset data requested");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ResetAllProgress();
                LogHelper.Log("Settings", "All progress has been reset!");
            }
            
            // 설정도 초기화
            PlayerPrefs.SetInt(PREF_BGM_ENABLED, 1);
            PlayerPrefs.SetInt(PREF_SFX_ENABLED, 1);
            PlayerPrefs.Save();
            
            LoadSettings();
        }
        
        private void OnQuitClicked()
        {
            LogHelper.Log("Settings", "Quit game requested");
            
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
        
        /// <summary>
        /// 설정 패널 표시
        /// </summary>
        public void Show()
        {
            if (_panel != null)
                _panel.SetActive(true);
            
            LoadSettings();
            LogHelper.Log("Settings", "Settings panel opened");
        }
        
        /// <summary>
        /// 설정 패널 숨김
        /// </summary>
        public void Hide()
        {
            if (_panel != null)
                _panel.SetActive(false);
            
            LogHelper.Log("Settings", "Settings panel closed");
        }
        
        /// <summary>
        /// 패널 표시 여부
        /// </summary>
        public bool IsVisible => _panel != null && _panel.activeSelf;
    }
}

