using UnityEngine;
using UnityEngine.UI;
using TMPro;
using PuzzleGame.Core.ScriptableObjects;
using PuzzleGame.Foundation.Manager;
using PuzzleGame.Foundation.Service;
using PuzzleGame.Module;
using System.Collections.Generic;

namespace PuzzleGame.Domain.UI.StageListUI
{
    /// <summary>
    /// Stage List 씬 UI 컨트롤러
    /// </summary>
    public class StageListUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform _stageListContainer;
        [SerializeField] private StageListItemView _stageItemPrefab;
        [SerializeField] private Button _backButton;
        
        [Header("Stage Data")]
        [SerializeField] private StagePreset[] _stagePresets;
        
        private List<StageListItemView> _stageItems = new List<StageListItemView>();
        
        private void Start()
        {
            if (_backButton != null)
                _backButton.onClick.AddListener(OnBackClicked);
            
            PopulateStageList();
            LogHelper.Log("StageListUI", "Stage list scene initialized");
        }
        
        private void PopulateStageList()
        {
            ClearStageList();
            
            if (_stagePresets == null || _stageItemPrefab == null) return;
            
            foreach (var preset in _stagePresets)
            {
                var item = Instantiate(_stageItemPrefab, _stageListContainer);
                
                bool isUnlocked = GameManager.Instance?.IsStageUnlocked(preset.StageId) ?? true;
                bool isCleared = GameManager.Instance?.IsStageClear(preset.StageId) ?? false;
                
                item.Setup(preset.StageId, preset.StageName, isUnlocked, isCleared);
                item.OnStageSelected += OnStageSelected;
                
                _stageItems.Add(item);
            }
        }
        
        private void ClearStageList()
        {
            foreach (var item in _stageItems)
            {
                if (item != null)
                {
                    item.OnStageSelected -= OnStageSelected;
                    Destroy(item.gameObject);
                }
            }
            _stageItems.Clear();
        }
        
        private void OnStageSelected(int stageId)
        {
            LogHelper.Log("StageListUI", $"Stage {stageId} selected");
            
            if (GameManager.Instance != null)
            {
                GameManager.Instance.StartStage(stageId);
            }
            
            SceneService.Instance?.GoToInGame();
        }
        
        private void OnBackClicked()
        {
            SceneService.Instance?.GoToLobby();
        }
    }
}
