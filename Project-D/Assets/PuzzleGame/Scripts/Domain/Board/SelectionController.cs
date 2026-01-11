using UnityEngine;
using UnityEngine.EventSystems;
using PuzzleGame.Domain.Board.Manager;
using PuzzleGame.Module;
using System;

namespace PuzzleGame.Domain.Board
{
    /// <summary>
    /// 선택 컨트롤러 - 터치/드래그 입력 처리
    /// </summary>
    public class SelectionController : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        [Header("References")]
        [SerializeField] private BoardManager _boardManager;
        
        [Header("Settings")]
        [SerializeField] private bool _enableInput = true;
        
        private bool _isSelecting;
        private int _startX, _startY;
        private int _endX, _endY;
        
        public bool EnableInput
        {
            get => _enableInput;
            set => _enableInput = value;
        }
        
        public event Action<int, int, int, int> OnSelectionCompleted;
        public event Action OnSelectionCancelled;
        
        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_enableInput) return;
            
            if (_boardManager.ScreenToGridPosition(eventData.position, out int gridX, out int gridY))
            {
                _isSelecting = true;
                _startX = gridX;
                _startY = gridY;
                _endX = gridX;
                _endY = gridY;
                
                _boardManager.HighlightSelection(_startX, _startY, _endX, _endY);
                
                LogHelper.Log("Selection", $"Start: ({_startX}, {_startY})");
            }
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (!_enableInput || !_isSelecting) return;
            
            if (_boardManager.ScreenToGridPosition(eventData.position, out int gridX, out int gridY))
            {
                if (gridX != _endX || gridY != _endY)
                {
                    _endX = gridX;
                    _endY = gridY;
                    
                    _boardManager.HighlightSelection(_startX, _startY, _endX, _endY);
                }
            }
        }
        
        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_enableInput || !_isSelecting) return;
            
            _isSelecting = false;
            _boardManager.ClearHighlight();
            
            if (_boardManager.ScreenToGridPosition(eventData.position, out int gridX, out int gridY))
            {
                _endX = gridX;
                _endY = gridY;
            }
            
            LogHelper.Log("Selection", $"End: ({_startX}, {_startY}) to ({_endX}, {_endY})");
            
            OnSelectionCompleted?.Invoke(_startX, _startY, _endX, _endY);
        }
        
        /// <summary>
        /// 선택 취소
        /// </summary>
        public void CancelSelection()
        {
            if (_isSelecting)
            {
                _isSelecting = false;
                _boardManager.ClearHighlight();
                OnSelectionCancelled?.Invoke();
            }
        }
    }
}
