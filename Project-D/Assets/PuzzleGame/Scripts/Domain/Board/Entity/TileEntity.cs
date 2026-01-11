using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame.Core.Enums;
using PuzzleGame.Module;

namespace PuzzleGame.Domain.Board.Entity
{
    /// <summary>
    /// 개별 타일 엔티티
    /// </summary>
    public class TileEntity : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _background;
        [SerializeField] private TextMeshProUGUI _valueText;
        [SerializeField] private Image _blockerIcon;
        [SerializeField] private GameObject _selectedHighlight;
        
        [Header("Colors")]
        [SerializeField] private Color _numberColor = new Color(0.3f, 0.5f, 0.8f);
        [SerializeField] private Color _zeroColor = new Color(0.5f, 0.5f, 0.5f);
        [SerializeField] private Color _blockerColor = new Color(0.8f, 0.2f, 0.2f);
        [SerializeField] private Color _emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        [SerializeField] private Color _selectedColor = new Color(1f, 0.9f, 0.3f); // 선택 시 색상
        
        private int _x;
        private int _y;
        private ETileKind _kind;
        private int _value;
        private bool _isRemoved;
        private bool _isSelected;
        private Color _originalColor;
        
        public int X => _x;
        public int Y => _y;
        public ETileKind Kind => _kind;
        public int Value => _value;
        public bool IsRemoved => _isRemoved;
        
        /// <summary>
        /// 타일 초기화
        /// </summary>
        public void Initialize(int x, int y, ETileKind kind, int value = 0)
        {
            _x = x;
            _y = y;
            _kind = kind;
            _value = value;
            _isRemoved = false;
            _isSelected = false;
            
            UpdateVisual();
        }
        
        /// <summary>
        /// 시각 업데이트
        /// </summary>
        private void UpdateVisual()
        {
            if (_background == null) return;
            
            // 선택 하이라이트 비활성화
            if (_selectedHighlight != null)
                _selectedHighlight.SetActive(false);
            
            // 블로커 아이콘 숨김
            if (_blockerIcon != null)
                _blockerIcon.gameObject.SetActive(false);
            
            switch (_kind)
            {
                case ETileKind.Number:
                    _originalColor = _numberColor;
                    _background.color = _numberColor;
                    if (_valueText != null)
                    {
                        _valueText.gameObject.SetActive(true);
                        _valueText.text = _value.ToString();
                    }
                    break;
                    
                case ETileKind.Zero:
                    _originalColor = _zeroColor;
                    _background.color = _zeroColor;
                    if (_valueText != null)
                    {
                        _valueText.gameObject.SetActive(true);
                        _valueText.text = "0";
                    }
                    break;
                    
                case ETileKind.Blocker:
                    _originalColor = _blockerColor;
                    _background.color = _blockerColor;
                    if (_valueText != null)
                        _valueText.gameObject.SetActive(false);
                    if (_blockerIcon != null)
                        _blockerIcon.gameObject.SetActive(true);
                    break;
                    
                case ETileKind.Empty:
                    _originalColor = _emptyColor;
                    _background.color = _emptyColor;
                    if (_valueText != null)
                        _valueText.gameObject.SetActive(false);
                    break;
            }
        }
        
        /// <summary>
        /// 선택 하이라이트 설정
        /// </summary>
        public void SetSelected(bool selected)
        {
            _isSelected = selected;
            
            // GameObject 하이라이트 (선택 사항)
            if (_selectedHighlight != null)
                _selectedHighlight.SetActive(selected);
            
            // 배경 색상 변경
            if (_background != null)
            {
                _background.color = selected ? _selectedColor : _originalColor;
            }
        }
        
        /// <summary>
        /// 제거 처리
        /// </summary>
        public void Remove()
        {
            if (_isRemoved) return;
            _isRemoved = true;
            
            // 제거 애니메이션 (기본: 페이드 아웃)
            PlayRemoveAnimation();
        }
        
        /// <summary>
        /// 제거 애니메이션 재생
        /// </summary>
        private void PlayRemoveAnimation()
        {
            // TODO: DOTween 등을 사용한 애니메이션
            // 현재는 단순히 비활성화
            gameObject.SetActive(false);
            
            LogHelper.Log("TileEntity", $"Tile ({_x}, {_y}) removed");
        }
    }
}
