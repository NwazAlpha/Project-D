using System;
using System.Collections.Generic;
using UnityEngine;
using PuzzleGame.Core.DataObject;
using PuzzleGame.Core.Enums;
using PuzzleGame.Module;

namespace PuzzleGame.Core.ScriptableObjects
{
    /// <summary>
    /// 스테이지 프리셋 ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "StagePreset", menuName = "PuzzleGame/Stage Preset")]
    public class StagePreset : ScriptableObject
    {
        [Header("Stage Info")]
        [SerializeField] private int _stageId;
        [SerializeField] private string _stageName;
        
        [Header("Board Settings")]
        [SerializeField, Min(2)] private int _width = 5;
        [SerializeField, Min(2)] private int _height = 5;
        [SerializeField, Min(1)] private int _targetNumber = 10;
        
        [Header("Tile Data")]
        [SerializeField] private TileData[] _tiles;
        
        [Header("Blocker Types")]
        [SerializeField] private BlockerType[] _blockerTypes;
        
        // Properties
        public int StageId => _stageId;
        public string StageName => _stageName;
        public int Width => _width;
        public int Height => _height;
        public int TargetNumber => _targetNumber;
        public IReadOnlyList<TileData> Tiles => _tiles;
        public IReadOnlyList<BlockerType> BlockerTypes => _blockerTypes;
        
        /// <summary>
        /// 특정 좌표의 타일 데이터 가져오기
        /// </summary>
        public TileData GetTile(int x, int y)
        {
            int index = y * _width + x;
            if (index < 0 || index >= _tiles.Length)
                return TileData.Empty;
            return _tiles[index];
        }
        
        /// <summary>
        /// 특정 좌표에 타일 데이터 설정
        /// </summary>
        public void SetTile(int x, int y, TileData tile)
        {
            int index = y * _width + x;
            if (index >= 0 && index < _tiles.Length)
            {
                _tiles[index] = tile;
            }
        }
        
        /// <summary>
        /// 보드 크기 변경 (에디터용)
        /// </summary>
        public void Resize(int newWidth, int newHeight)
        {
            var newTiles = new TileData[newWidth * newHeight];
            
            // 기존 데이터 복사
            for (int y = 0; y < Mathf.Min(_height, newHeight); y++)
            {
                for (int x = 0; x < Mathf.Min(_width, newWidth); x++)
                {
                    int oldIndex = y * _width + x;
                    int newIndex = y * newWidth + x;
                    if (oldIndex < _tiles.Length)
                    {
                        newTiles[newIndex] = _tiles[oldIndex];
                    }
                }
            }
            
            _width = newWidth;
            _height = newHeight;
            _tiles = newTiles;
        }
        
        /// <summary>
        /// 타일 배열 초기화
        /// </summary>
        public void InitializeTiles()
        {
            _tiles = new TileData[_width * _height];
            for (int i = 0; i < _tiles.Length; i++)
            {
                _tiles[i] = TileData.Empty;
            }
        }
        
        /// <summary>
        /// 유효성 검증
        /// </summary>
        public bool Validate(out List<string> errors)
        {
            errors = new List<string>();
            
            // Target 검증
            if (_targetNumber <= 0)
            {
                errors.Add("TargetNumber must be greater than 0");
            }
            
            // 타일 배열 크기 검증
            if (_tiles == null || _tiles.Length != _width * _height)
            {
                errors.Add($"Tile array size mismatch. Expected {_width * _height}, got {_tiles?.Length ?? 0}");
            }
            
            if (_tiles != null)
            {
                for (int i = 0; i < _tiles.Length; i++)
                {
                    var tile = _tiles[i];
                    
                    // 숫자 타일 범위 검증
                    if (tile.Kind == ETileKind.Number)
                    {
                        if (tile.Value < 1 || tile.Value >= _targetNumber)
                        {
                            int x = i % _width;
                            int y = i / _width;
                            errors.Add($"Tile at ({x}, {y}): Number value {tile.Value} must be between 1 and {_targetNumber - 1}");
                        }
                    }
                    
                    // 블로커 타입 ID 검증
                    if (tile.Kind == ETileKind.Blocker)
                    {
                        bool validBlocker = false;
                        if (_blockerTypes != null)
                        {
                            foreach (var blockerType in _blockerTypes)
                            {
                                if (blockerType.Id == tile.BlockerTypeId)
                                {
                                    validBlocker = true;
                                    break;
                                }
                            }
                        }
                        
                        if (!validBlocker)
                        {
                            int x = i % _width;
                            int y = i / _width;
                            errors.Add($"Tile at ({x}, {y}): Invalid blocker type ID {tile.BlockerTypeId}");
                        }
                    }
                }
            }
            
            return errors.Count == 0;
        }
        
        /// <summary>
        /// 제거 가능한 숫자 타일 수 계산
        /// </summary>
        public int CountRemovableTiles()
        {
            int count = 0;
            if (_tiles != null)
            {
                foreach (var tile in _tiles)
                {
                    if (tile.Kind == ETileKind.Number)
                    {
                        count++;
                    }
                }
            }
            return count;
        }
        
#if UNITY_EDITOR
        private void OnValidate()
        {
            // 에디터에서 크기 변경 시 타일 배열 조정
            if (_tiles == null || _tiles.Length != _width * _height)
            {
                Resize(_width, _height);
            }
            
            // 유효성 검증 로그
            if (Validate(out var errors))
            {
                LogHelper.Log("StagePreset", $"{name} validation passed");
            }
            else
            {
                foreach (var error in errors)
                {
                    LogHelper.LogWarning($"[{name}] {error}");
                }
            }
        }
#endif
    }
}
