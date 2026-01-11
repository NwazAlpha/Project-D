namespace PuzzleGame.Core.Enums
{
    /// <summary>
    /// 타일 종류
    /// </summary>
    public enum ETileKind
    {
        /// <summary>
        /// 빈 셀 (에디터용)
        /// </summary>
        Empty = 0,
        
        /// <summary>
        /// 숫자 타일 (1 ~ Target-1, 제거 대상)
        /// </summary>
        Number = 1,
        
        /// <summary>
        /// 0 타일 (합산에 포함, 제거되지 않음)
        /// </summary>
        Zero = 2,
        
        /// <summary>
        /// 무한 타일 (Blocker) - 선택 시 즉시 실패
        /// </summary>
        Blocker = 3
    }
}
