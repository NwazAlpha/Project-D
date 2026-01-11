using UnityEngine;
using PuzzleGame.Core;
using PuzzleGame.Module;
using System;

namespace PuzzleGame.Foundation.Manager
{
    /// <summary>
    /// 게임 매니저 - 전역 게임 상태 관리
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;
        public static GameManager Instance => _instance;
        
        [Header("Stage Progress")]
        private int _currentStageId;
        private int _highScore;
        
        public int CurrentStageId => _currentStageId;
        public int HighScore => _highScore;
        
        public event Action<int> OnStageCleared;
        public event Action<int> OnHighScoreUpdated;
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                LoadProgress();
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        /// <summary>
        /// 스테이지 시작
        /// </summary>
        public void StartStage(int stageId)
        {
            _currentStageId = stageId;
            LogHelper.Log("GameManager", $"Starting stage: {stageId}");
        }
        
        /// <summary>
        /// 스테이지 클리어 저장
        /// </summary>
        public void SaveStageClear(int stageId, int score)
        {
            string key = Constants.PlayerPrefsKey.StageClearPrefix + stageId;
            
            // 이전 최고 점수 확인
            int previousScore = PlayerPrefs.GetInt(key, 0);
            if (score > previousScore)
            {
                PlayerPrefs.SetInt(key, score);
                LogHelper.Log("GameManager", $"New high score for stage {stageId}: {score}");
            }
            
            // 글로벌 하이스코어 업데이트
            if (score > _highScore)
            {
                _highScore = score;
                PlayerPrefs.SetInt(Constants.PlayerPrefsKey.HighScore, _highScore);
                OnHighScoreUpdated?.Invoke(_highScore);
            }
            
            PlayerPrefs.Save();
            
            OnStageCleared?.Invoke(stageId);
        }
        
        /// <summary>
        /// 스테이지 클리어 여부 확인
        /// </summary>
        public bool IsStageClear(int stageId)
        {
            string key = Constants.PlayerPrefsKey.StageClearPrefix + stageId;
            return PlayerPrefs.GetInt(key, 0) > 0;
        }
        
        /// <summary>
        /// 스테이지 해금 여부 확인
        /// </summary>
        public bool IsStageUnlocked(int stageId)
        {
            // 첫 번째 스테이지는 항상 해금
            if (stageId <= 1) return true;
            
            // 이전 스테이지 클리어 시 해금
            return IsStageClear(stageId - 1);
        }
        
        /// <summary>
        /// 진행 상황 로드
        /// </summary>
        private void LoadProgress()
        {
            _highScore = PlayerPrefs.GetInt(Constants.PlayerPrefsKey.HighScore, 0);
            LogHelper.Log("GameManager", $"Progress loaded. High score: {_highScore}");
        }
        
        /// <summary>
        /// 모든 진행 상황 리셋 (디버그용)
        /// </summary>
        public void ResetAllProgress()
        {
            PlayerPrefs.DeleteAll();
            _highScore = 0;
            LogHelper.Log("GameManager", "All progress reset");
        }
    }
}
