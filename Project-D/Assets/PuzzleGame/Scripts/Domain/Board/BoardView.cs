using UnityEngine;
using PuzzleGame.Core.ScriptableObjects;
using PuzzleGame.Domain.Board.Entity;
using PuzzleGame.Module;

namespace PuzzleGame.Domain.Board
{
    /// <summary>
    /// 보드 뷰 - 그리드 렌더링
    /// </summary>
    public class BoardView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform _boardContainer;
        [SerializeField] private TileEntity _tilePrefab;
        [SerializeField] private RectTransform _dragArea; // 드래그 영역 (선택 사항)
        
        [Header("Layout Settings")]
        [SerializeField] private float _tileSize = 100f;
        [SerializeField] private float _tileSpacing = 5f;
        
        private TileEntity[,] _tileEntities;
        private int _width;
        private int _height;
        
        public TileEntity[,] TileEntities => _tileEntities;
        public RectTransform BoardContainer => _boardContainer;
        
        /// <summary>
        /// 보드 생성
        /// </summary>
        public void CreateBoard(StagePreset preset)
        {
            _width = preset.Width;
            _height = preset.Height;
            
            ClearBoard();
            
            _tileEntities = new TileEntity[_width, _height];
            
            // 보드 컨테이너 크기 설정
            float boardWidth = _width * (_tileSize + _tileSpacing) - _tileSpacing;
            float boardHeight = _height * (_tileSize + _tileSpacing) - _tileSpacing;
            
            if (_boardContainer != null)
            {
                _boardContainer.sizeDelta = new Vector2(boardWidth, boardHeight);
            }
            
            // 드래그 영역 크기 동기화
            SyncDragAreaSize();
            
            // 타일 생성
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var tileData = preset.GetTile(x, y);
                    var tile = CreateTile(x, y, tileData);
                    _tileEntities[x, y] = tile;
                }
            }
            
            LogHelper.Log("BoardView", $"Board created: {_width}x{_height}");
        }
        
        /// <summary>
        /// 드래그 영역 크기를 보드와 동기화
        /// </summary>
        public void SyncDragAreaSize()
        {
            if (_dragArea != null && _boardContainer != null)
            {
                _dragArea.sizeDelta = _boardContainer.sizeDelta;
                _dragArea.anchoredPosition = _boardContainer.anchoredPosition;
                _dragArea.anchorMin = _boardContainer.anchorMin;
                _dragArea.anchorMax = _boardContainer.anchorMax;
                _dragArea.pivot = _boardContainer.pivot;
                
                LogHelper.Log("BoardView", "Drag area synced with board container");
            }
        }
        
        /// <summary>
        /// 타일 생성
        /// </summary>
        private TileEntity CreateTile(int x, int y, Core.DataObject.TileData data)
        {
            if (_tilePrefab == null)
            {
                LogHelper.LogError("BoardView: Tile prefab is not assigned!");
                return null;
            }
            
            var tile = Instantiate(_tilePrefab, _boardContainer);
            
            // 위치 설정 (좌하단 원점)
            var rectTransform = tile.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                float posX = x * (_tileSize + _tileSpacing) + _tileSize / 2f;
                float posY = y * (_tileSize + _tileSpacing) + _tileSize / 2f;
                
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.zero;
                rectTransform.pivot = new Vector2(0.5f, 0.5f);
                rectTransform.anchoredPosition = new Vector2(posX, posY);
                rectTransform.sizeDelta = new Vector2(_tileSize, _tileSize);
            }
            
            tile.Initialize(x, y, data.Kind, data.Value);
            tile.name = $"Tile_{x}_{y}";
            
            return tile;
        }
        
        /// <summary>
        /// 보드 클리어
        /// </summary>
        public void ClearBoard()
        {
            if (_tileEntities != null)
            {
                for (int y = 0; y < _height; y++)
                {
                    for (int x = 0; x < _width; x++)
                    {
                        if (_tileEntities[x, y] != null)
                        {
                            Destroy(_tileEntities[x, y].gameObject);
                        }
                    }
                }
                _tileEntities = null;
            }
            
            // 혹시 남은 자식 오브젝트도 정리
            if (_boardContainer != null)
            {
                foreach (Transform child in _boardContainer)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        
        /// <summary>
        /// 특정 좌표의 타일 엔티티 가져오기
        /// </summary>
        public TileEntity GetTileEntity(int x, int y)
        {
            if (_tileEntities == null) return null;
            if (x < 0 || x >= _width || y < 0 || y >= _height) return null;
            return _tileEntities[x, y];
        }
        
        /// <summary>
        /// 선택 영역 하이라이트
        /// </summary>
        public void HighlightSelection(int startX, int startY, int endX, int endY)
        {
            ClearHighlight();
            
            int minX = Mathf.Min(startX, endX);
            int maxX = Mathf.Max(startX, endX);
            int minY = Mathf.Min(startY, endY);
            int maxY = Mathf.Max(startY, endY);
            
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    var tile = GetTileEntity(x, y);
                    if (tile != null)
                    {
                        tile.SetSelected(true);
                    }
                }
            }
        }
        
        /// <summary>
        /// 모든 하이라이트 제거
        /// </summary>
        public void ClearHighlight()
        {
            if (_tileEntities == null) return;
            
            for (int y = 0; y < _height; y++)
            {
                for (int x = 0; x < _width; x++)
                {
                    var tile = _tileEntities[x, y];
                    if (tile != null)
                    {
                        tile.SetSelected(false);
                    }
                }
            }
        }
        
        /// <summary>
        /// 화면 좌표를 그리드 좌표로 변환
        /// </summary>
        public bool ScreenToGridPosition(Vector2 screenPosition, out int gridX, out int gridY)
        {
            gridX = -1;
            gridY = -1;
            
            if (_boardContainer == null) return false;
            
            // 스크린 좌표를 로컬 좌표로 변환
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _boardContainer, screenPosition, null, out Vector2 localPoint);
            
            // 피벗 오프셋 보정 (피벗이 (0,0)이 아닐 경우)
            // 로컬 좌표는 피벗 기준이므로, 좌하단 기준으로 변환
            Vector2 size = _boardContainer.rect.size;
            Vector2 pivot = _boardContainer.pivot;
            
            // 피벗 기준 좌표를 좌하단 기준 좌표로 변환
            float adjustedX = localPoint.x + (size.x * pivot.x);
            float adjustedY = localPoint.y + (size.y * pivot.y);
            
            // 그리드 좌표 계산
            gridX = Mathf.FloorToInt(adjustedX / (_tileSize + _tileSpacing));
            gridY = Mathf.FloorToInt(adjustedY / (_tileSize + _tileSpacing));
            
            // 범위 체크
            if (gridX < 0 || gridX >= _width || gridY < 0 || gridY >= _height)
            {
                return false;
            }
            
            return true;
        }
    }
}
