using UnityEngine;
using PuzzleGame.Foundation.Service;
using PuzzleGame.Foundation.Manager;
using PuzzleGame.Module;

namespace PuzzleGame.Foundation.GameEntry
{
    /// <summary>
    /// 게임 진입점 - 초기화 담당
    /// </summary>
    public class GameEntry : MonoBehaviour
    {
        [Header("Persistent Managers")]
        [SerializeField] private GameManager _gameManagerPrefab;
        [SerializeField] private SceneService _sceneServicePrefab;
        
        private static bool _isInitialized = false;
        
        private void Awake()
        {
            if (_isInitialized)
            {
                Destroy(gameObject);
                return;
            }
            
            Initialize();
            _isInitialized = true;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Initialize()
        {
            LogHelper.Log("GameEntry", "Initializing game...");
            
            InitializeServices();
            InitializeManagers();
            
            LogHelper.Log("GameEntry", "Game initialized successfully!");
        }
        
        private void InitializeServices()
        {
            if (SceneService.Instance == null && _sceneServicePrefab != null)
            {
                Instantiate(_sceneServicePrefab);
            }
        }
        
        private void InitializeManagers()
        {
            if (GameManager.Instance == null && _gameManagerPrefab != null)
            {
                Instantiate(_gameManagerPrefab);
            }
        }
    }
}
