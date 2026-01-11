using UnityEngine;
using PuzzleGame.Core.ScriptableObjects;
using PuzzleGame.Domain.Board;
using PuzzleGame.Domain.Board.Manager;
using PuzzleGame.Foundation.Manager;
using PuzzleGame.Module;
using System;

namespace PuzzleGame.Domain.Game
{
    /// <summary>
    /// 게임 상태
    /// </summary>
    public enum EGameState
    {
        Initializing,
        Playing,
        Paused,
        Cleared,
        Failed
    }
    
    /// <summary>
    /// 인게임 코디네이터 - 게임 플레이 흐름 관리
    /// </summary>
    public class InGameCoordinator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardManager _boardManager;
        [SerializeField] private SelectionController _selectionController;
        [SerializeField] private StagePreset _testPreset; // 테스트용
        
        private JudgmentSystem _judgmentSystem;
        private EGameState _gameState;
        
        public EGameState GameState => _gameState;
        public int CurrentScore => _judgmentSystem?.Score ?? 0;
        
        public event Action<EGameState> OnGameStateChanged;
        public event Action<int> OnScoreChanged;
        public event Action<EJudgmentResult> OnJudgmentResult;
        public event Action OnStageClear;
        
        private void Start()
        {
            Initialize();
        }
        
        /// <summary>
        /// 초기화
        /// </summary>
        private void Initialize()
        {
            _gameState = EGameState.Initializing;
            
            // JudgmentSystem 생성
            _judgmentSystem = new JudgmentSystem(_boardManager);
            _judgmentSystem.OnScoreChanged += HandleScoreChanged;
            _judgmentSystem.OnJudgmentMade += HandleJudgmentMade;
            
            // SelectionController 이벤트 연결
            if (_selectionController != null)
            {
                _selectionController.OnSelectionCompleted += HandleSelectionCompleted;
            }
            
            // BoardManager 이벤트 연결
            if (_boardManager != null)
            {
                _boardManager.OnBoardCleared += HandleBoardCleared;
            }
            
            // 테스트 프리셋 로드
            if (_testPreset != null)
            {
                LoadStage(_testPreset);
            }
        }
        
        /// <summary>
        /// 스테이지 로드
        /// </summary>
        public void LoadStage(StagePreset preset)
        {
            LogHelper.Log("InGameCoordinator", $"Loading stage: {preset.StageName}");
            
            _boardManager.LoadStage(preset);
            _judgmentSystem.ResetScore();
            
            SetGameState(EGameState.Playing);
        }
        
        /// <summary>
        /// 게임 일시정지
        /// </summary>
        public void Pause()
        {
            if (_gameState != EGameState.Playing) return;
            
            SetGameState(EGameState.Paused);
            if (_selectionController != null)
                _selectionController.EnableInput = false;
            
            LogHelper.Log("InGameCoordinator", "Game paused");
        }
        
        /// <summary>
        /// 게임 재개
        /// </summary>
        public void Resume()
        {
            if (_gameState != EGameState.Paused) return;
            
            SetGameState(EGameState.Playing);
            if (_selectionController != null)
                _selectionController.EnableInput = true;
            
            LogHelper.Log("InGameCoordinator", "Game resumed");
        }
        
        /// <summary>
        /// 스테이지 리스타트
        /// </summary>
        public void Restart()
        {
            _boardManager.ResetStage();
            _judgmentSystem.ResetScore();
            SetGameState(EGameState.Playing);
            
            if (_selectionController != null)
                _selectionController.EnableInput = true;
            
            LogHelper.Log("InGameCoordinator", "Stage restarted");
        }
        
        private void HandleSelectionCompleted(int startX, int startY, int endX, int endY)
        {
            if (_gameState != EGameState.Playing) return;
            
            var result = _judgmentSystem.Judge(startX, startY, endX, endY);
            OnJudgmentResult?.Invoke(result);
        }
        
        private void HandleJudgmentMade(EJudgmentResult result, Core.DataObject.SelectionResult selection)
        {
            // 판정 결과에 따른 피드백 (애니메이션, 사운드 등)
            // 현재는 로그만 출력
        }
        
        private void HandleScoreChanged(int score)
        {
            OnScoreChanged?.Invoke(score);
        }
        
        private void HandleBoardCleared()
        {
            SetGameState(EGameState.Cleared);
            
            if (_selectionController != null)
                _selectionController.EnableInput = false;
            
            // 스테이지 클리어 저장
            if (GameManager.Instance != null)
            {
                int stageId = GameManager.Instance.CurrentStageId;
                GameManager.Instance.SaveStageClear(stageId, CurrentScore);
            }
            
            OnStageClear?.Invoke();
            LogHelper.Log("InGameCoordinator", "Stage cleared!");
        }
        
        private void SetGameState(EGameState newState)
        {
            if (_gameState == newState) return;
            
            _gameState = newState;
            OnGameStateChanged?.Invoke(_gameState);
        }
        
        private void OnDestroy()
        {
            if (_judgmentSystem != null)
            {
                _judgmentSystem.OnScoreChanged -= HandleScoreChanged;
                _judgmentSystem.OnJudgmentMade -= HandleJudgmentMade;
            }
            
            if (_selectionController != null)
            {
                _selectionController.OnSelectionCompleted -= HandleSelectionCompleted;
            }
            
            if (_boardManager != null)
            {
                _boardManager.OnBoardCleared -= HandleBoardCleared;
            }
        }
    }
}
