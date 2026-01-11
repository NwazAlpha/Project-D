using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace PuzzleGame.Domain.UI.StageListUI
{
    /// <summary>
    /// 스테이지 리스트 아이템 뷰
    /// </summary>
    public class StageListItemView : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button _button;
        [SerializeField] private TextMeshProUGUI _stageNameText;
        [SerializeField] private GameObject _lockIcon;
        [SerializeField] private GameObject _clearIcon;
        
        [Header("Colors")]
        [SerializeField] private Color _unlockedColor = Color.white;
        [SerializeField] private Color _lockedColor = Color.gray;
        
        private int _stageId;
        private bool _isUnlocked;
        
        public event Action<int> OnStageSelected;
        
        private void Awake()
        {
            if (_button != null)
            {
                _button.onClick.AddListener(OnButtonClicked);
            }
        }
        
        /// <summary>
        /// 스테이지 아이템 설정
        /// </summary>
        public void Setup(int stageId, string stageName, bool isUnlocked, bool isCleared)
        {
            _stageId = stageId;
            _isUnlocked = isUnlocked;
            
            if (_stageNameText != null)
            {
                _stageNameText.text = stageName;
                _stageNameText.color = isUnlocked ? _unlockedColor : _lockedColor;
            }
            
            if (_lockIcon != null)
                _lockIcon.SetActive(!isUnlocked);
            
            if (_clearIcon != null)
                _clearIcon.SetActive(isCleared);
            
            if (_button != null)
                _button.interactable = isUnlocked;
        }
        
        private void OnButtonClicked()
        {
            if (_isUnlocked)
            {
                OnStageSelected?.Invoke(_stageId);
            }
        }
    }
}
