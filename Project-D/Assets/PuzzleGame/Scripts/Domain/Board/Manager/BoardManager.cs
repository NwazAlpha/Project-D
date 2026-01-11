using UnityEngine;
using PuzzleGame.Core.ScriptableObjects;
using PuzzleGame.Core.DataObject;
using PuzzleGame.Module;
using System;

namespace PuzzleGame.Domain.Board.Manager
{
    /// <summary>
    /// 보드 매니저 - BoardModel + BoardView 관리
    /// </summary>
    public class BoardManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BoardView _boardView;
        
        private BoardModel _boardModel;
        private StagePreset _currentPreset;
        
        public BoardModel Model => _boardModel;
        public BoardView View => _boardView;
        public int TargetNumber => _boardModel?.TargetNumber ?? 0;
        
        public event Action OnBoardCleared;
        public event Action<int, int> OnTileRemoved;
        
        /// <summary>
        /// 스테이지 로드
        /// </summary>
        public void LoadStage(StagePreset preset)
        {
            _currentPreset = preset;
            
            // 모델 생성
            _boardModel = new BoardModel(preset);
            _boardModel.OnTileRemoved += HandleTileRemoved;
            _boardModel.OnAllTilesRemoved += HandleAllTilesRemoved;
            
            // 뷰 생성
            if (_boardView != null)
            {
                _boardView.CreateBoard(preset);
            }
            
            LogHelper.Log("BoardManager", $"Stage loaded: {preset.StageName}");
        }
        
        /// <summary>
        /// 현재 스테이지 리셋
        /// </summary>
        public void ResetStage()
        {
            if (_currentPreset != null)
            {
                LoadStage(_currentPreset);
            }
        }
        
        /// <summary>
        /// 선택 영역 해석
        /// </summary>
        public SelectionResult InterpretSelection(int startX, int startY, int endX, int endY)
        {
            if (_boardModel == null) return new SelectionResult();
            return _boardModel.InterpretSelection(startX, startY, endX, endY);
        }
        
        /// <summary>
        /// 타일 제거
        /// </summary>
        public bool RemoveTile(int x, int y)
        {
            if (_boardModel == null) return false;
            return _boardModel.RemoveTile(x, y);
        }
        
        /// <summary>
        /// 선택 영역 하이라이트
        /// </summary>
        public void HighlightSelection(int startX, int startY, int endX, int endY)
        {
            _boardView?.HighlightSelection(startX, startY, endX, endY);
        }
        
        /// <summary>
        /// 하이라이트 제거
        /// </summary>
        public void ClearHighlight()
        {
            _boardView?.ClearHighlight();
        }
        
        /// <summary>
        /// 화면 좌표를 그리드 좌표로 변환
        /// </summary>
        public bool ScreenToGridPosition(Vector2 screenPosition, out int gridX, out int gridY)
        {
            gridX = -1;
            gridY = -1;
            
            if (_boardView == null) return false;
            return _boardView.ScreenToGridPosition(screenPosition, out gridX, out gridY);
        }
        
        /// <summary>
        /// 클리어 여부 확인
        /// </summary>
        public bool IsCleared()
        {
            return _boardModel?.IsCleared() ?? false;
        }
        
        private void HandleTileRemoved(int x, int y, RuntimeTileState tile)
        {
            // 뷰 업데이트
            var tileEntity = _boardView?.GetTileEntity(x, y);
            tileEntity?.Remove();
            
            OnTileRemoved?.Invoke(x, y);
        }
        
        private void HandleAllTilesRemoved()
        {
            LogHelper.Log("BoardManager", "All tiles removed! Stage cleared!");
            OnBoardCleared?.Invoke();
        }
        
        private void OnDestroy()
        {
            if (_boardModel != null)
            {
                _boardModel.OnTileRemoved -= HandleTileRemoved;
                _boardModel.OnAllTilesRemoved -= HandleAllTilesRemoved;
            }
        }
    }
}
