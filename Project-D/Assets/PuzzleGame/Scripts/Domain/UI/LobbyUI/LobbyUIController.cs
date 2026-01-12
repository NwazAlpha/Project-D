using UnityEngine;
using UnityEngine.UI;
using PuzzleGame.Foundation.Service;
using PuzzleGame.Domain.UI.SettingsUI;
using PuzzleGame.Module;

namespace PuzzleGame.Domain.UI.LobbyUI
{
    /// <summary>
    /// Lobby 씬 UI 컨트롤러
    /// </summary>
    public class LobbyUIController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField] private Button _stageListButton;
        [SerializeField] private Button _shopButton;
        [SerializeField] private Button _settingsButton;
        
        [Header("Panels")]
        [SerializeField] private SettingsPanel _settingsPanel;
        
        private void Start()
        {
            if (_stageListButton != null)
                _stageListButton.onClick.AddListener(OnStageListClicked);
            
            if (_shopButton != null)
            {
                _shopButton.onClick.AddListener(OnShopClicked);
                // Shop은 비활성화
                _shopButton.interactable = false;
            }
            
            if (_settingsButton != null)
                _settingsButton.onClick.AddListener(OnSettingsClicked);
            
            // 설정 패널 초기 숨김
            if (_settingsPanel != null)
                _settingsPanel.Hide();
            
            LogHelper.Log("LobbyUI", "Lobby scene initialized");
        }
        
        private void OnStageListClicked()
        {
            LogHelper.Log("LobbyUI", "Going to Stage List");
            SceneService.Instance?.GoToStageList();
        }
        
        private void OnShopClicked()
        {
            // Shop은 아직 구현되지 않음
            LogHelper.Log("LobbyUI", "Shop is not available yet");
        }
        
        private void OnSettingsClicked()
        {
            if (_settingsPanel != null)
            {
                _settingsPanel.Show();
            }
            else
            {
                LogHelper.Log("LobbyUI", "Settings panel is not assigned");
            }
        }
    }
}

