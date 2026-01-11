using System;
using PuzzleGame.Core.DataObject;
using PuzzleGame.Core.Enums;
using PuzzleGame.Core.ScriptableObjects;

namespace PuzzleGame.Domain.Board
{
    /// <summary>
    /// 런타임 타일 상태
    /// </summary>
    public class RuntimeTileState
    {
        public ETileKind Kind { get; set; }
        public int Value { get; set; }
        public int BlockerTypeId { get; set; }
        public bool IsRemoved { get; set; }
        
        public RuntimeTileState(TileData data)
        {
            Kind = data.Kind;
            Value = data.Value;
            BlockerTypeId = data.BlockerTypeId;
            IsRemoved = false;
        }
    }
    
    /// <summary>
    /// 보드 모델 - 타일 상태 관리
    /// </summary>
    public class BoardModel
    {
        private readonly int _width;
        private readonly int _height;
        private readonly int _targetNumber;
        private readonly RuntimeTileState[,] _tiles;
        private int _removableTileCount;
        
        public int Width => _width;
        public int Height => _height;
        public int TargetNumber => _targetNumber;
        public int RemovableTileCount => _removableTileCount;
        
        public event Action<int, int, RuntimeTileState> OnTileRemoved;
        public event Action OnAllTilesRemoved;
        
        public BoardModel(StagePreset preset)
        {
            _width = preset.Width;
            _height = preset.Height;
            _targetNumber = preset.TargetNumber;
            _tiles = new RuntimeTileState[_width, _height];
            _removableTileCount = 0;
            
            // 프리셋에서 타일 데이터 로드
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var tileData = preset.GetTile(x, y);
                    _tiles[x, y] = new RuntimeTileState(tileData);
                    
                    if (tileData.Kind == ETileKind.Number)
                    {
                        _removableTileCount++;
                    }
                }
            }
        }
        
        /// <summary>
        /// 특정 좌표의 타일 상태 가져오기
        /// </summary>
        public RuntimeTileState GetTile(int x, int y)
        {
            if (x < 0 || x >= _width || y < 0 || y >= _height)
                return null;
            return _tiles[x, y];
        }
        
        /// <summary>
        /// 타일이 유효한 좌표인지 확인
        /// </summary>
        public bool IsValidPosition(int x, int y)
        {
            return x >= 0 && x < _width && y >= 0 && y < _height;
        }
        
        /// <summary>
        /// 타일 제거 (숫자 타일만)
        /// </summary>
        public bool RemoveTile(int x, int y)
        {
            var tile = GetTile(x, y);
            if (tile == null) return false;
            
            // 숫자 타일만 제거 가능
            if (tile.Kind != ETileKind.Number) return false;
            
            // 이미 제거됨
            if (tile.IsRemoved) return false;
            
            tile.IsRemoved = true;
            _removableTileCount--;
            
            OnTileRemoved?.Invoke(x, y, tile);
            
            if (_removableTileCount <= 0)
            {
                OnAllTilesRemoved?.Invoke();
            }
            
            return true;
        }
        
        /// <summary>
        /// 선택 영역 해석
        /// </summary>
        public SelectionResult InterpretSelection(int startX, int startY, int endX, int endY)
        {
            var result = new SelectionResult();
            
            // 축 정렬 사각형으로 정규화
            int minX = Math.Min(startX, endX);
            int maxX = Math.Max(startX, endX);
            int minY = Math.Min(startY, endY);
            int maxY = Math.Max(startY, endY);
            
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    var tile = GetTile(x, y);
                    if (tile == null || tile.IsRemoved) continue;
                    
                    switch (tile.Kind)
                    {
                        case ETileKind.Number:
                            result.NumberTiles.Add((x, y));
                            result.NumberSum += tile.Value;
                            break;
                        case ETileKind.Zero:
                            result.ZeroTiles.Add((x, y));
                            // 0 타일은 합산에 0으로 포함 (이미 0이므로 추가 안 함)
                            break;
                        case ETileKind.Blocker:
                            result.ContainsBlocker = true;
                            break;
                        case ETileKind.Empty:
                            // Empty는 무시
                            break;
                    }
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 클리어 조건 확인
        /// </summary>
        public bool IsCleared()
        {
            return _removableTileCount <= 0;
        }
    }
}
