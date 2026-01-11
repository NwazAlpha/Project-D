using PuzzleGame.Core.DataObject;
using PuzzleGame.Domain.Board.Manager;
using PuzzleGame.Module;
using System;

namespace PuzzleGame.Domain.Board
{
    /// <summary>
    /// 판정 결과
    /// </summary>
    public enum EJudgmentResult
    {
        Success,
        FailNotEnoughTiles,    // 숫자 타일 개수 < 2
        FailContainsBlocker,   // Blocker 포함
        FailWrongSum           // 합 != Target
    }
    
    /// <summary>
    /// 판정 시스템 - 정답 판정 로직
    /// </summary>
    public class JudgmentSystem
    {
        private readonly BoardManager _boardManager;
        private int _score;
        
        public int Score => _score;
        
        public event Action<EJudgmentResult, SelectionResult> OnJudgmentMade;
        public event Action<int> OnScoreChanged;
        
        public JudgmentSystem(BoardManager boardManager)
        {
            _boardManager = boardManager;
            _score = 0;
        }
        
        /// <summary>
        /// 선택 영역 판정
        /// </summary>
        public EJudgmentResult Judge(int startX, int startY, int endX, int endY)
        {
            var selection = _boardManager.InterpretSelection(startX, startY, endX, endY);
            
            return Judge(selection);
        }
        
        /// <summary>
        /// 선택 결과 판정
        /// </summary>
        public EJudgmentResult Judge(SelectionResult selection)
        {
            EJudgmentResult result;
            
            // 실패 조건 1: 숫자 타일 개수 < 2
            if (selection.NumberCount < 2)
            {
                result = EJudgmentResult.FailNotEnoughTiles;
                LogHelper.Log("Judgment", $"Failed: Not enough tiles ({selection.NumberCount} < 2)");
                OnJudgmentMade?.Invoke(result, selection);
                return result;
            }
            
            // 실패 조건 2: Blocker 포함
            if (selection.ContainsBlocker)
            {
                result = EJudgmentResult.FailContainsBlocker;
                LogHelper.Log("Judgment", "Failed: Contains blocker");
                OnJudgmentMade?.Invoke(result, selection);
                return result;
            }
            
            // 실패 조건 3: 합 != Target
            int targetNumber = _boardManager.TargetNumber;
            if (selection.NumberSum != targetNumber)
            {
                result = EJudgmentResult.FailWrongSum;
                LogHelper.Log("Judgment", $"Failed: Wrong sum ({selection.NumberSum} != {targetNumber})");
                OnJudgmentMade?.Invoke(result, selection);
                return result;
            }
            
            // 성공: 숫자 타일 제거
            result = EJudgmentResult.Success;
            ProcessSuccess(selection);
            OnJudgmentMade?.Invoke(result, selection);
            
            return result;
        }
        
        /// <summary>
        /// 성공 처리
        /// </summary>
        private void ProcessSuccess(SelectionResult selection)
        {
            // 숫자 타일 제거 (0 타일은 유지)
            foreach (var (x, y) in selection.NumberTiles)
            {
                _boardManager.RemoveTile(x, y);
            }
            
            // 점수 증가 (제거한 타일 수만큼)
            int scoreIncrease = selection.NumberCount;
            _score += scoreIncrease;
            
            LogHelper.Log("Judgment", $"Success! Removed {selection.NumberCount} tiles. Score: {_score}");
            OnScoreChanged?.Invoke(_score);
        }
        
        /// <summary>
        /// 점수 리셋
        /// </summary>
        public void ResetScore()
        {
            _score = 0;
            OnScoreChanged?.Invoke(_score);
        }
    }
}
