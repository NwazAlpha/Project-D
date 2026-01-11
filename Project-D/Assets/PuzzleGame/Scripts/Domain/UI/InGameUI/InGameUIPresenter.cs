using UnityEngine;
using PuzzleGame.Domain.Game;
using PuzzleGame.Domain.Board;
using PuzzleGame.Foundation.Service;
using PuzzleGame.Module;

namespace PuzzleGame.Domain.UI.InGame
{
    /// <summary>
    /// InGame UI 프레젠터
    /// </summary>
    public class InGameUIPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private InGameUIView _view;
        [SerializeField] private InGameCoordinator _gameCoordinator;
        
        private InGameUIModel _model;
        
        private void Awake()
        {
            _model = new InGameUIModel();
        }
        
        private void Start()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            // 버튼 이벤트 연결
            if (_view.PauseButton != null)
                _view.PauseButton.onClick.AddListener(OnPauseClicked);
            
            if (_view.RestartButton != null)
                _view.RestartButton.onClick.AddListener(OnRestartClicked);
            
            if (_view.ResumeButton != null)
                _view.ResumeButton.onClick.AddListener(OnResumeClicked);
            
            if (_view.ExitButton != null)
                _view.ExitButton.onClick.AddListener(OnExitClicked);
            
            if (_view.NextStageButton != null)
                _view.NextStageButton.onClick.AddListener(OnNextStageClicked);
            
            if (_view.ReturnToListButton != null)
                _view.ReturnToListButton.onClick.AddListener(OnReturnToListClicked);
            
            // 게임 코디네이터 이벤트 연결
            if (_gameCoordinator != null)
            {
                _gameCoordinator.OnScoreChanged += HandleScoreChanged;
                _gameCoordinator.OnGameStateChanged += HandleGameStateChanged;
                _gameCoordinator.OnStageClear += HandleStageClear;
                _gameCoordinator.OnJudgmentResult += HandleJudgmentResult;
            }
            
            // 초기 UI 상태
            _view.ShowPausePanel(false);
            _view.ShowClearPanel(false);
        }
        
        private void HandleScoreChanged(int score)
        {
            _model.Score = score;
            _view.UpdateScore(score);
        }
        
        private void HandleGameStateChanged(EGameState state)
        {
            _model.IsPaused = (state == EGameState.Paused);
            _view.ShowPausePanel(_model.IsPaused);
        }
        
        private void HandleStageClear()
        {
            _model.IsCleared = true;
            _view.ShowClearPanel(true, _model.Score);
        }
        
        private void HandleJudgmentResult(EJudgmentResult result)
        {
            // 판정 결과 피드백 (예: 실패 시 화면 흔들림)
            if (result != EJudgmentResult.Success)
            {
                LogHelper.Log("InGameUIPresenter", $"Judgment failed: {result}");
                // TODO: 실패 피드백 애니메이션
            }
        }
        
        private void OnPauseClicked()
        {
            _gameCoordinator?.Pause();
        }
        
        private void OnResumeClicked()
        {
            _gameCoordinator?.Resume();
        }
        
        private void OnRestartClicked()
        {
            _view.ShowPausePanel(false);
            _view.ShowClearPanel(false);
            _gameCoordinator?.Restart();
            _model.Reset();
        }
        
        private void OnExitClicked()
        {
            SceneService.Instance?.GoToStageList();
        }
        
        private void OnNextStageClicked()
        {
            // TODO: 다음 스테이지 로드
            LogHelper.Log("InGameUIPresenter", "Next stage clicked");
        }
        
        private void OnReturnToListClicked()
        {
            SceneService.Instance?.GoToStageList();
        }
        
        private void OnDestroy()
        {
            if (_gameCoordinator != null)
            {
                _gameCoordinator.OnScoreChanged -= HandleScoreChanged;
                _gameCoordinator.OnGameStateChanged -= HandleGameStateChanged;
                _gameCoordinator.OnStageClear -= HandleStageClear;
                _gameCoordinator.OnJudgmentResult -= HandleJudgmentResult;
            }
        }
    }
}
