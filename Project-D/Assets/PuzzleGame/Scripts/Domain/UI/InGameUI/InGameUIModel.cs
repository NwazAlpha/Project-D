using System;

namespace PuzzleGame.Domain.UI.InGame
{
    /// <summary>
    /// InGame UI 모델
    /// </summary>
    public class InGameUIModel
    {
        public int Score { get; set; }
        public int TargetNumber { get; set; }
        public bool IsPaused { get; set; }
        public bool IsCleared { get; set; }
        
        public void Reset()
        {
            Score = 0;
            IsPaused = false;
            IsCleared = false;
        }
    }
}
