using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace PuzzleGame.Domain.UI.InGame
{
    /// <summary>
    /// InGame UI 뷰
    /// </summary>
    public class InGameUIView : MonoBehaviour
    {
        [Header("Score")]
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private TextMeshProUGUI _targetText;
        
        [Header("Buttons")]
        [SerializeField] private Button _pauseButton;
        [SerializeField] private Button _restartButton;
        
        [Header("Panels")]
        [SerializeField] private GameObject _pausePanel;
        [SerializeField] private GameObject _clearPanel;
        
        [Header("Pause Panel")]
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _exitButton;
        
        [Header("Clear Panel")]
        [SerializeField] private TextMeshProUGUI _clearScoreText;
        [SerializeField] private Button _nextStageButton;
        [SerializeField] private Button _returnToListButton;
        
        // Properties for Presenter
        public TextMeshProUGUI ScoreText => _scoreText;
        public TextMeshProUGUI TargetText => _targetText;
        public Button PauseButton => _pauseButton;
        public Button RestartButton => _restartButton;
        public GameObject PausePanel => _pausePanel;
        public GameObject ClearPanel => _clearPanel;
        public Button ResumeButton => _resumeButton;
        public Button SettingsButton => _settingsButton;
        public Button ExitButton => _exitButton;
        public TextMeshProUGUI ClearScoreText => _clearScoreText;
        public Button NextStageButton => _nextStageButton;
        public Button ReturnToListButton => _returnToListButton;
        
        /// <summary>
        /// 점수 업데이트
        /// </summary>
        public void UpdateScore(int score)
        {
            if (_scoreText != null)
                _scoreText.text = $"Score: {score}";
        }
        
        /// <summary>
        /// 타겟 숫자 표시
        /// </summary>
        public void UpdateTarget(int target)
        {
            if (_targetText != null)
                _targetText.text = $"Target: {target}";
        }
        
        /// <summary>
        /// 퍼즈 패널 표시
        /// </summary>
        public void ShowPausePanel(bool show)
        {
            if (_pausePanel != null)
                _pausePanel.SetActive(show);
        }
        
        /// <summary>
        /// 클리어 패널 표시
        /// </summary>
        public void ShowClearPanel(bool show, int score = 0)
        {
            if (_clearPanel != null)
            {
                _clearPanel.SetActive(show);
                if (show && _clearScoreText != null)
                {
                    _clearScoreText.text = $"Final Score: {score}";
                }
            }
        }
    }
}
