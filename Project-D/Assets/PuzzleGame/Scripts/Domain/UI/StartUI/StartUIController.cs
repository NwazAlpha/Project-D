using UnityEngine;
using UnityEngine.UI;
using PuzzleGame.Foundation.Service;
using PuzzleGame.Module;

namespace PuzzleGame.Domain.UI.StartUI
{
    /// <summary>
    /// Start 씬 UI 컨트롤러
    /// Tap anywhere → Lobby 이동
    /// </summary>
    public class StartUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button _tapButton;
        
        [Header("Dummy Login (Optional)")]
        [SerializeField] private GameObject _loginPanel;
        [SerializeField] private Button _loginButton;
        
        private void Start()
        {
            if (_tapButton != null)
            {
                _tapButton.onClick.AddListener(OnTapAnywhere);
            }
            
            if (_loginButton != null)
            {
                _loginButton.onClick.AddListener(OnLoginClicked);
            }
            
            LogHelper.Log("StartUI", "Start scene initialized");
        }
        
        private void OnTapAnywhere()
        {
            LogHelper.Log("StartUI", "Tap anywhere - going to Lobby");
            SceneService.Instance?.GoToLobby();
        }
        
        private void OnLoginClicked()
        {
            // 더미 로그인 (실제 로그인 구현 없음)
            LogHelper.Log("StartUI", "Dummy login clicked");
            OnTapAnywhere();
        }
    }
}
