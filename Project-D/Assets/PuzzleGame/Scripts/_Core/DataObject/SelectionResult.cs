using System.Collections.Generic;
using PuzzleGame.Core.Enums;

namespace PuzzleGame.Core.DataObject
{
    /// <summary>
    /// 선택 영역 해석 결과
    /// </summary>
    public class SelectionResult
    {
        /// <summary>
        /// 선택된 숫자 타일 좌표 리스트
        /// </summary>
        public List<(int x, int y)> NumberTiles { get; } = new List<(int x, int y)>();
        
        /// <summary>
        /// 선택된 0 타일 좌표 리스트
        /// </summary>
        public List<(int x, int y)> ZeroTiles { get; } = new List<(int x, int y)>();
        
        /// <summary>
        /// 블로커 포함 여부
        /// </summary>
        public bool ContainsBlocker { get; set; }
        
        /// <summary>
        /// 숫자 타일 값들의 합
        /// </summary>
        public int NumberSum { get; set; }
        
        /// <summary>
        /// 숫자 타일 개수
        /// </summary>
        public int NumberCount => NumberTiles.Count;
        
        /// <summary>
        /// 선택 영역 초기화
        /// </summary>
        public void Clear()
        {
            NumberTiles.Clear();
            ZeroTiles.Clear();
            ContainsBlocker = false;
            NumberSum = 0;
        }
    }
}
