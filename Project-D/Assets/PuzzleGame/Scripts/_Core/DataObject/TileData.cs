using System;
using UnityEngine;
using PuzzleGame.Core.Enums;

namespace PuzzleGame.Core.DataObject
{
    /// <summary>
    /// 타일 데이터 (프리셋용)
    /// </summary>
    [Serializable]
    public struct TileData
    {
        [SerializeField] private ETileKind _kind;
        [SerializeField] private int _value;
        [SerializeField] private int _blockerTypeId;
        
        public ETileKind Kind => _kind;
        
        /// <summary>
        /// 숫자 타일의 값 (Number 타입일 때만 유효)
        /// </summary>
        public int Value => _value;
        
        /// <summary>
        /// 블로커 타입 ID (Blocker 타입일 때만 유효)
        /// </summary>
        public int BlockerTypeId => _blockerTypeId;
        
        public static TileData Empty => new TileData { _kind = ETileKind.Empty };
        public static TileData Zero => new TileData { _kind = ETileKind.Zero };
        
        public static TileData CreateNumber(int value)
        {
            return new TileData
            {
                _kind = ETileKind.Number,
                _value = value
            };
        }
        
        public static TileData CreateBlocker(int blockerTypeId)
        {
            return new TileData
            {
                _kind = ETileKind.Blocker,
                _blockerTypeId = blockerTypeId
            };
        }
    }
}
