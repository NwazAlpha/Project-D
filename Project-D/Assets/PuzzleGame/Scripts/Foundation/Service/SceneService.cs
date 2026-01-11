using UnityEngine;
using PuzzleGame.Core;
using PuzzleGame.Module;
using System;

namespace PuzzleGame.Foundation.Service
{
    /// <summary>
    /// 씬 서비스 - 씬 전환 관리
    /// </summary>
    public class SceneService : MonoBehaviour
    {
        private static SceneService _instance;
        public static SceneService Instance => _instance;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;
        
        /// <summary>
        /// 씬 로드
        /// </summary>
        public void LoadScene(string sceneName)
        {
            LogHelper.Log("SceneService", $"Loading scene: {sceneName}");
            OnSceneLoadStarted?.Invoke(sceneName);
            
            UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
            
            OnSceneLoadCompleted?.Invoke(sceneName);
        }
        
        /// <summary>
        /// Start 씬으로 이동
        /// </summary>
        public void GoToStart()
        {
            LoadScene(Constants.SceneName.Start);
        }
        
        /// <summary>
        /// Lobby 씬으로 이동
        /// </summary>
        public void GoToLobby()
        {
            LoadScene(Constants.SceneName.Lobby);
        }
        
        /// <summary>
        /// Stage List 씬으로 이동
        /// </summary>
        public void GoToStageList()
        {
            LoadScene(Constants.SceneName.StageList);
        }
        
        /// <summary>
        /// InGame 씬으로 이동
        /// </summary>
        public void GoToInGame()
        {
            LoadScene(Constants.SceneName.InGame);
        }
    }
}
