#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using PuzzleGame.Core.ScriptableObjects;
using PuzzleGame.Core.DataObject;
using PuzzleGame.Core.Enums;
using System.Collections.Generic;

namespace PuzzleGame.Editor
{
    /// <summary>
    /// Stage Preset 에디터 윈도우
    /// </summary>
    public class StagePresetEditorWindow : EditorWindow
    {
        private StagePreset _currentPreset;
        private Vector2 _scrollPosition;
        
        // 브러쉬 설정
        private enum EBrushType { Number, Zero, Blocker, Eraser }
        private EBrushType _currentBrush = EBrushType.Number;
        private int _brushValue = 1;
        private int _brushBlockerTypeId = 0;
        
        // 언두/리두
        private Stack<TileData[]> _undoStack = new Stack<TileData[]>();
        private Stack<TileData[]> _redoStack = new Stack<TileData[]>();
        
        // 클리어 검증 성능 제한
        private const int MAX_SEARCH_ITERATIONS = 10000;
        private const float MAX_SEARCH_TIME_SECONDS = 3f;
        private int _iterationCount;
        private float _searchStartTime;
        private const int MAX_UNDO = 50;
        
        // 표시 설정
        private float _tileDisplaySize = 40f;
        private Color _numberColor = new Color(0.3f, 0.5f, 0.8f);
        private Color _zeroColor = new Color(0.5f, 0.5f, 0.5f);
        private Color _blockerColor = new Color(0.8f, 0.2f, 0.2f);
        private Color _emptyColor = new Color(0.2f, 0.2f, 0.2f, 0.3f);
        
        [MenuItem("PuzzleGame/Stage Preset Editor")]
        public static void ShowWindow()
        {
            var window = GetWindow<StagePresetEditorWindow>("Stage Preset Editor");
            window.minSize = new Vector2(400, 500);
        }
        
        private void OnGUI()
        {
            DrawPresetSelector();
            
            if (_currentPreset == null)
            {
                EditorGUILayout.HelpBox("Select a StagePreset to edit.", MessageType.Info);
                return;
            }
            
            DrawGridSettings();
            DrawBrushSettings();
            DrawUndoRedoButtons();
            DrawGrid();
            DrawAutoFillButton();
            DrawValidationButton();
        }
        
        private void DrawPresetSelector()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Stage Preset", EditorStyles.boldLabel);
            
            var newPreset = (StagePreset)EditorGUILayout.ObjectField(
                "Current Preset", _currentPreset, typeof(StagePreset), false);
            
            if (newPreset != _currentPreset)
            {
                _currentPreset = newPreset;
                ClearUndoRedo();
            }
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawGridSettings()
        {
            EditorGUILayout.LabelField("Grid Settings", EditorStyles.boldLabel);
            
            EditorGUI.BeginChangeCheck();
            
            SerializedObject so = new SerializedObject(_currentPreset);
            SerializedProperty widthProp = so.FindProperty("_width");
            SerializedProperty heightProp = so.FindProperty("_height");
            SerializedProperty targetProp = so.FindProperty("_targetNumber");
            
            EditorGUILayout.PropertyField(widthProp, new GUIContent("Width"));
            EditorGUILayout.PropertyField(heightProp, new GUIContent("Height"));
            EditorGUILayout.PropertyField(targetProp, new GUIContent("Target Number"));
            
            if (EditorGUI.EndChangeCheck())
            {
                so.ApplyModifiedProperties();
                EditorUtility.SetDirty(_currentPreset);
            }
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawBrushSettings()
        {
            EditorGUILayout.LabelField("Brush Settings", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            
            if (GUILayout.Toggle(_currentBrush == EBrushType.Number, "Number", "Button"))
                _currentBrush = EBrushType.Number;
            if (GUILayout.Toggle(_currentBrush == EBrushType.Zero, "Zero", "Button"))
                _currentBrush = EBrushType.Zero;
            if (GUILayout.Toggle(_currentBrush == EBrushType.Blocker, "Blocker", "Button"))
                _currentBrush = EBrushType.Blocker;
            if (GUILayout.Toggle(_currentBrush == EBrushType.Eraser, "Eraser", "Button"))
                _currentBrush = EBrushType.Eraser;
            
            EditorGUILayout.EndHorizontal();
            
            if (_currentBrush == EBrushType.Number)
            {
                _brushValue = EditorGUILayout.IntSlider("Value", _brushValue, 1, 
                    Mathf.Max(1, _currentPreset.TargetNumber - 1));
            }
            else if (_currentBrush == EBrushType.Blocker)
            {
                _brushBlockerTypeId = EditorGUILayout.IntField("Blocker Type ID", _brushBlockerTypeId);
            }
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawUndoRedoButtons()
        {
            EditorGUILayout.BeginHorizontal();
            
            GUI.enabled = _undoStack.Count > 0;
            if (GUILayout.Button("Undo"))
            {
                Undo();
            }
            
            GUI.enabled = _redoStack.Count > 0;
            if (GUILayout.Button("Redo"))
            {
                Redo();
            }
            
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawGrid()
        {
            EditorGUILayout.LabelField("Grid", EditorStyles.boldLabel);
            
            int width = _currentPreset.Width;
            int height = _currentPreset.Height;
            
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, 
                GUILayout.Height(Mathf.Min(height * (_tileDisplaySize + 2) + 20, 400)));
            
            // Y축을 위에서 아래로 (그래서 height-1부터 0까지)
            for (int y = height - 1; y >= 0; y--)
            {
                EditorGUILayout.BeginHorizontal();
                
                for (int x = 0; x < width; x++)
                {
                    DrawTileButton(x, y);
                }
                
                EditorGUILayout.EndHorizontal();
            }
            
            EditorGUILayout.EndScrollView();
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawTileButton(int x, int y)
        {
            var tile = _currentPreset.GetTile(x, y);
            
            Color oldColor = GUI.backgroundColor;
            string label = "";
            
            switch (tile.Kind)
            {
                case ETileKind.Number:
                    GUI.backgroundColor = _numberColor;
                    label = tile.Value.ToString();
                    break;
                case ETileKind.Zero:
                    GUI.backgroundColor = _zeroColor;
                    label = "0";
                    break;
                case ETileKind.Blocker:
                    GUI.backgroundColor = _blockerColor;
                    label = "∞";
                    break;
                case ETileKind.Empty:
                    GUI.backgroundColor = _emptyColor;
                    label = "";
                    break;
            }
            
            if (GUILayout.Button(label, GUILayout.Width(_tileDisplaySize), GUILayout.Height(_tileDisplaySize)))
            {
                ApplyBrush(x, y);
            }
            
            GUI.backgroundColor = oldColor;
        }
        
        private void ApplyBrush(int x, int y)
        {
            SaveUndoState();
            
            TileData newTile;
            
            switch (_currentBrush)
            {
                case EBrushType.Number:
                    newTile = TileData.CreateNumber(_brushValue);
                    break;
                case EBrushType.Zero:
                    newTile = TileData.Zero;
                    break;
                case EBrushType.Blocker:
                    newTile = TileData.CreateBlocker(_brushBlockerTypeId);
                    break;
                case EBrushType.Eraser:
                default:
                    newTile = TileData.Empty;
                    break;
            }
            
            _currentPreset.SetTile(x, y, newTile);
            EditorUtility.SetDirty(_currentPreset);
        }
        
        private void DrawAutoFillButton()
        {
            EditorGUILayout.LabelField("Auto Fill", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Fill Empty Cells with Random Numbers"))
            {
                AutoFillEmptyCells();
            }
            
            if (GUILayout.Button("Clear All"))
            {
                if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to clear all tiles?", "Yes", "No"))
                {
                    SaveUndoState();
                    _currentPreset.InitializeTiles();
                    EditorUtility.SetDirty(_currentPreset);
                }
            }
            
            EditorGUILayout.Space(5);
        }
        
        private void DrawValidationButton()
        {
            EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Validate Preset"))
            {
                ValidatePreset();
            }
            
            if (GUILayout.Button("Check Clearability (Simple)"))
            {
                CheckClearability();
            }
        }
        
        private void AutoFillEmptyCells()
        {
            SaveUndoState();
            
            int width = _currentPreset.Width;
            int height = _currentPreset.Height;
            int target = _currentPreset.TargetNumber;
            
            // 빈 셀 좌표 수집
            List<(int x, int y)> emptyCells = new List<(int x, int y)>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (_currentPreset.GetTile(x, y).Kind == ETileKind.Empty)
                    {
                        emptyCells.Add((x, y));
                    }
                }
            }
            
            if (emptyCells.Count < 2)
            {
                Debug.LogWarning("[StagePresetEditor] Not enough empty cells for auto-fill");
                return;
            }
            
            const int MAX_ATTEMPTS = 50;
            int attempt = 0;
            bool success = false;
            
            // 원본 상태 저장 (실패 시 복원용)
            TileData[] originalState = new TileData[_currentPreset.Tiles.Count];
            for (int i = 0; i < originalState.Length; i++)
            {
                originalState[i] = _currentPreset.Tiles[i];
            }
            
            EditorUtility.DisplayProgressBar("Auto Fill", "Generating clearable stage...", 0);
            
            try
            {
                while (attempt < MAX_ATTEMPTS && !success)
                {
                    attempt++;
                    EditorUtility.DisplayProgressBar("Auto Fill", 
                        $"Attempt {attempt}/{MAX_ATTEMPTS}...", (float)attempt / MAX_ATTEMPTS);
                    
                    // 빈 셀 다시 비우기
                    foreach (var cell in emptyCells)
                    {
                        _currentPreset.SetTile(cell.x, cell.y, TileData.Empty);
                    }
                    
                    // 타일 채우기
                    FillEmptyCellsOnce(emptyCells, target);
                    
                    // 클리어 가능성 검증
                    if (VerifyClearability())
                    {
                        success = true;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
            
            if (success)
            {
                EditorUtility.SetDirty(_currentPreset);
                Debug.Log($"[StagePresetEditor] Auto-fill succeeded on attempt {attempt}!");
                EditorUtility.DisplayDialog("Auto Fill", 
                    $"✓ Clearable stage generated!\n\nAttempts: {attempt}", "OK");
            }
            else
            {
                // 실패 시 원본 복원
                for (int i = 0; i < originalState.Length; i++)
                {
                    int x = i % width;
                    int y2 = i / width;
                    _currentPreset.SetTile(x, y2, originalState[i]);
                }
                EditorUtility.SetDirty(_currentPreset);
                
                Debug.LogWarning($"[StagePresetEditor] Auto-fill failed after {MAX_ATTEMPTS} attempts");
                EditorUtility.DisplayDialog("Auto Fill", 
                    $"✗ Failed to generate clearable stage after {MAX_ATTEMPTS} attempts.\n\n" +
                    "Try adjusting the grid size or target number.", "OK");
            }
        }
        
        /// <summary>
        /// 한 번 채우기 시도 (검증 없이)
        /// </summary>
        private void FillEmptyCellsOnce(List<(int x, int y)> emptyCells, int target)
        {
            // 셀을 셔플
            ShuffleList(emptyCells);
            
            int cellIndex = 0;
            
            // 유효한 조합을 만들면서 채우기
            while (cellIndex < emptyCells.Count)
            {
                // 인접한 사각형 영역을 찾아서 유효한 조합 배치
                if (cellIndex + 1 < emptyCells.Count && TryPlaceValidGroup(emptyCells, ref cellIndex, target))
                {
                    // 성공
                }
                else
                {
                    // 남은 셀은 숫자 타일로 채우기
                    var cell = emptyCells[cellIndex];
                    int value = Random.Range(1, target);
                    _currentPreset.SetTile(cell.x, cell.y, TileData.CreateNumber(value));
                    cellIndex++;
                }
            }
        }
        
        /// <summary>
        /// 클리어 가능성 검증 (UI 없이)
        /// </summary>
        private bool VerifyClearability()
        {
            // 성능 추적 초기화
            _iterationCount = 0;
            _searchStartTime = (float)EditorApplication.timeSinceStartup;
            
            int width = _currentPreset.Width;
            int height = _currentPreset.Height;
            int target = _currentPreset.TargetNumber;
            
            // 현재 보드 상태 복사
            int[,] board = new int[width, height];
            int numberTileCount = 0;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var tile = _currentPreset.GetTile(x, y);
                    switch (tile.Kind)
                    {
                        case ETileKind.Number:
                            board[x, y] = tile.Value;
                            numberTileCount++;
                            break;
                        case ETileKind.Blocker:
                            board[x, y] = -2;
                            break;
                        default:
                            board[x, y] = -1;
                            break;
                    }
                }
            }
            
            if (numberTileCount < 2)
            {
                return false;
            }
            
            // 백트래킹 시뮬레이션 (해법 기록 없이)
            return SimulateClearabilityQuick(board, width, height, target, numberTileCount);
        }
        
        /// <summary>
        /// 빠른 클리어 가능성 체크 (해법 기록 없음)
        /// </summary>
        private bool SimulateClearabilityQuick(int[,] board, int width, int height, int target, int remainingTiles)
        {
            if (remainingTiles == 0) return true;
            
            // 성능 제한 체크
            _iterationCount++;
            if (_iterationCount > MAX_SEARCH_ITERATIONS)
            {
                return false; // 탐색 횟수 초과
            }
            
            float elapsed = (float)(EditorApplication.timeSinceStartup - _searchStartTime);
            if (elapsed > MAX_SEARCH_TIME_SECONDS)
            {
                return false; // 시간 초과
            }
            
            var validMoves = FindAllValidMoves(board, width, height, target);
            if (validMoves.Count == 0) return false;
            
            foreach (var move in validMoves)
            {
                // 이동 적용
                foreach (var (x, y) in move.tiles)
                {
                    board[x, y] = -1;
                }
                
                // 재귀 탐색
                if (SimulateClearabilityQuick(board, width, height, target, remainingTiles - move.tiles.Count))
                {
                    return true;
                }
                
                // 백트래킹: 타일 복원
                foreach (var (x, y) in move.tiles)
                {
                    var originalTile = _currentPreset.GetTile(x, y);
                    board[x, y] = originalTile.Value;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 유효한 타일 그룹 배치 시도
        /// </summary>
        private bool TryPlaceValidGroup(List<(int x, int y)> emptyCells, ref int startIndex, int target)
        {
            // 2~4개의 타일로 Target 합을 만드는 조합 생성
            int groupSize = Random.Range(2, Mathf.Min(5, emptyCells.Count - startIndex + 1));
            
            if (startIndex + groupSize > emptyCells.Count)
            {
                groupSize = emptyCells.Count - startIndex;
            }
            
            if (groupSize < 2) return false;
            
            // 인접한 사각형을 이루는 셀들인지 확인
            var groupCells = new List<(int x, int y)>();
            for (int i = 0; i < groupSize; i++)
            {
                groupCells.Add(emptyCells[startIndex + i]);
            }
            
            // 사각형 영역 계산
            int minX = int.MaxValue, maxX = int.MinValue;
            int minY = int.MaxValue, maxY = int.MinValue;
            
            foreach (var cell in groupCells)
            {
                minX = Mathf.Min(minX, cell.x);
                maxX = Mathf.Max(maxX, cell.x);
                minY = Mathf.Min(minY, cell.y);
                maxY = Mathf.Max(maxY, cell.y);
            }
            
            // 사각형이 너무 크면 (블로커나 다른 타일이 포함될 수 있음) 그룹 크기 제한
            int rectWidth = maxX - minX + 1;
            int rectHeight = maxY - minY + 1;
            
            if (rectWidth > 3 || rectHeight > 3)
            {
                // 작은 그룹으로 시도
                groupSize = 2;
                groupCells = new List<(int x, int y)>
                {
                    emptyCells[startIndex],
                    emptyCells[startIndex + 1]
                };
            }
            
            // Target을 groupSize개의 숫자로 분할
            List<int> values = GenerateValidCombination(target, groupSize);
            
            if (values == null || values.Count != groupSize)
            {
                return false;
            }
            
            // 값 배치
            for (int i = 0; i < groupSize; i++)
            {
                var cell = groupCells[i];
                _currentPreset.SetTile(cell.x, cell.y, TileData.CreateNumber(values[i]));
            }
            
            startIndex += groupSize;
            return true;
        }
        
        /// <summary>
        /// Target 합을 만드는 유효한 조합 생성
        /// </summary>
        private List<int> GenerateValidCombination(int target, int count)
        {
            if (count < 2 || target <= count) return null;
            
            List<int> values = new List<int>();
            int remaining = target;
            int maxValue = target - 1;
            
            // count-1개의 값을 랜덤하게 생성
            for (int i = 0; i < count - 1; i++)
            {
                // 남은 숫자들이 최소 1씩은 되도록 범위 제한
                int minNeeded = count - 1 - i; // 남은 슬롯 수
                int maxAllowed = Mathf.Min(maxValue, remaining - minNeeded);
                
                if (maxAllowed < 1) 
                {
                    // 유효한 조합을 만들 수 없음
                    return null;
                }
                
                int value = Random.Range(1, maxAllowed + 1);
                values.Add(value);
                remaining -= value;
            }
            
            // 마지막 값은 나머지
            if (remaining < 1 || remaining >= target)
            {
                return null;
            }
            
            values.Add(remaining);
            
            // 셔플
            ShuffleList(values);
            
            return values;
        }
        
        /// <summary>
        /// 리스트 셔플 (Fisher-Yates)
        /// </summary>
        private void ShuffleList<T>(List<T> list)
        {
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                T temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
        
        private void ValidatePreset()
        {
            if (_currentPreset.Validate(out List<string> errors))
            {
                EditorUtility.DisplayDialog("Validation", "Preset is valid!", "OK");
            }
            else
            {
                string message = "Validation failed:\n\n" + string.Join("\n", errors);
                EditorUtility.DisplayDialog("Validation", message, "OK");
            }
        }
        
        private void CheckClearability()
        {
            // 성능 추적 초기화
            _iterationCount = 0;
            _searchStartTime = (float)EditorApplication.timeSinceStartup;
            
            int width = _currentPreset.Width;
            int height = _currentPreset.Height;
            int target = _currentPreset.TargetNumber;
            
            // 현재 보드 상태 복사
            int[,] board = new int[width, height]; // -1: empty/zero, -2: blocker, 1+: number value
            int numberTileCount = 0;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var tile = _currentPreset.GetTile(x, y);
                    switch (tile.Kind)
                    {
                        case ETileKind.Number:
                            board[x, y] = tile.Value;
                            numberTileCount++;
                            break;
                        case ETileKind.Zero:
                            board[x, y] = -1; // 0 타일은 제거 대상 아님
                            break;
                        case ETileKind.Blocker:
                            board[x, y] = -2;
                            break;
                        default:
                            board[x, y] = -1;
                            break;
                    }
                }
            }
            
            if (numberTileCount < 2)
            {
                EditorUtility.DisplayDialog("Clearability", 
                    "Not clearable: Less than 2 number tiles.", "OK");
                return;
            }
            
            // 백트래킹 시뮬레이션
            List<string> solutionMoves = new List<string>();
            bool canClear = SimulateClearability(board, width, height, target, numberTileCount, solutionMoves);
            
            if (canClear)
            {
                string moves = string.Join("\n", solutionMoves);
                EditorUtility.DisplayDialog("Clearability", 
                    $"✓ Stage is CLEARABLE!\n\nSolution ({solutionMoves.Count} moves):\n{moves}", "OK");
                Debug.Log($"[ClearabilityCheck] Solution found:\n{moves}");
            }
            else
            {
                // 타임아웃/반복 초과인지 확인
                bool wasInterrupted = _iterationCount > MAX_SEARCH_ITERATIONS || 
                    (float)(EditorApplication.timeSinceStartup - _searchStartTime) > MAX_SEARCH_TIME_SECONDS;
                
                string message = wasInterrupted 
                    ? $"⚠ Search interrupted after {_iterationCount} iterations.\n\nCould not determine clearability within time limit.\nTry reducing board size or number of tiles."
                    : "✗ Stage is NOT CLEARABLE!\n\nNo combination of moves can remove all number tiles.";
                    
                EditorUtility.DisplayDialog("Clearability", message, "OK");
            }
        }
        
        /// <summary>
        /// 백트래킹으로 클리어 가능성 시뮬레이션
        /// </summary>
        private bool SimulateClearability(int[,] board, int width, int height, int target, int remainingTiles, List<string> moves)
        {
            // 클리어 조건: 모든 숫자 타일 제거됨
            if (remainingTiles == 0)
            {
                return true;
            }
            
            // 성능 제한 체크
            _iterationCount++;
            if (_iterationCount > MAX_SEARCH_ITERATIONS)
            {
                return false; // 탐색 횟수 초과
            }
            
            float elapsed = (float)(EditorApplication.timeSinceStartup - _searchStartTime);
            if (elapsed > MAX_SEARCH_TIME_SECONDS)
            {
                return false; // 시간 초과
            }
            
            // 모든 가능한 사각형 선택 시도
            List<(int x1, int y1, int x2, int y2, List<(int x, int y)> tiles)> validMoves = 
                FindAllValidMoves(board, width, height, target);
            
            if (validMoves.Count == 0)
            {
                return false; // 유효한 수가 없음
            }
            
            foreach (var move in validMoves)
            {
                // 이동 적용
                foreach (var (x, y) in move.tiles)
                {
                    board[x, y] = -1; // 제거됨 표시
                }
                
                int removedCount = move.tiles.Count;
                string moveStr = $"({move.x1},{move.y1})~({move.x2},{move.y2}): {removedCount} tiles";
                moves.Add(moveStr);
                
                // 재귀 탐색
                if (SimulateClearability(board, width, height, target, remainingTiles - removedCount, moves))
                {
                    return true;
                }
                
                // 백트래킹: 이동 취소
                moves.RemoveAt(moves.Count - 1);
                
                // 타일 복원
                foreach (var (x, y) in move.tiles)
                {
                    var originalTile = _currentPreset.GetTile(x, y);
                    board[x, y] = originalTile.Value;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// 현재 보드에서 모든 유효한 이동 찾기
        /// </summary>
        private List<(int x1, int y1, int x2, int y2, List<(int x, int y)> tiles)> FindAllValidMoves(
            int[,] board, int width, int height, int target)
        {
            var validMoves = new List<(int x1, int y1, int x2, int y2, List<(int x, int y)> tiles)>();
            
            // 모든 사각형 조합 탐색
            for (int y1 = 0; y1 < height; y1++)
            {
                for (int x1 = 0; x1 < width; x1++)
                {
                    for (int y2 = y1; y2 < height; y2++)
                    {
                        for (int x2 = (y2 == y1 ? x1 : 0); x2 < width; x2++)
                        {
                            // 최소 2셀 이상의 사각형
                            if (x1 == x2 && y1 == y2) continue;
                            
                            var result = EvaluateRectangle(board, x1, y1, x2, y2, target);
                            
                            if (result.isValid)
                            {
                                validMoves.Add((x1, y1, x2, y2, result.numberTiles));
                            }
                        }
                    }
                }
            }
            
            return validMoves;
        }
        
        /// <summary>
        /// 사각형 영역 평가
        /// </summary>
        private (bool isValid, List<(int x, int y)> numberTiles) EvaluateRectangle(
            int[,] board, int x1, int y1, int x2, int y2, int target)
        {
            int minX = Mathf.Min(x1, x2);
            int maxX = Mathf.Max(x1, x2);
            int minY = Mathf.Min(y1, y2);
            int maxY = Mathf.Max(y1, y2);
            
            int sum = 0;
            var numberTiles = new List<(int x, int y)>();
            bool hasBlocker = false;
            
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int value = board[x, y];
                    
                    if (value == -2) // 블로커
                    {
                        hasBlocker = true;
                        break;
                    }
                    else if (value > 0) // 숫자 타일
                    {
                        sum += value;
                        numberTiles.Add((x, y));
                    }
                    // value == -1은 empty/zero/removed이므로 무시
                }
                
                if (hasBlocker) break;
            }
            
            // 유효 조건: 블로커 없음, 숫자 타일 2개 이상, 합 == target
            bool isValid = !hasBlocker && numberTiles.Count >= 2 && sum == target;
            
            return (isValid, numberTiles);
        }
        
        private int CalculateRectangleSum(int x1, int y1, int x2, int y2, out int count, out bool hasBlocker)
        {
            int minX = Mathf.Min(x1, x2);
            int maxX = Mathf.Max(x1, x2);
            int minY = Mathf.Min(y1, y2);
            int maxY = Mathf.Max(y1, y2);
            
            int sum = 0;
            count = 0;
            hasBlocker = false;
            
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    var tile = _currentPreset.GetTile(x, y);
                    
                    if (tile.Kind == ETileKind.Number)
                    {
                        sum += tile.Value;
                        count++;
                    }
                    else if (tile.Kind == ETileKind.Blocker)
                    {
                        hasBlocker = true;
                    }
                }
            }
            
            return sum;
        }
        
        #region Undo/Redo
        
        private void SaveUndoState()
        {
            if (_currentPreset == null) return;
            
            TileData[] state = new TileData[_currentPreset.Tiles.Count];
            for (int i = 0; i < state.Length; i++)
            {
                state[i] = _currentPreset.Tiles[i];
            }
            
            _undoStack.Push(state);
            _redoStack.Clear();
            
            // 최대 언두 수 제한
            while (_undoStack.Count > MAX_UNDO)
            {
                var temp = new Stack<TileData[]>();
                while (_undoStack.Count > 1)
                {
                    temp.Push(_undoStack.Pop());
                }
                _undoStack.Clear();
                while (temp.Count > 0)
                {
                    _undoStack.Push(temp.Pop());
                }
            }
        }
        
        private void Undo()
        {
            if (_undoStack.Count == 0) return;
            
            // 현재 상태를 리두 스택에 저장
            TileData[] currentState = new TileData[_currentPreset.Tiles.Count];
            for (int i = 0; i < currentState.Length; i++)
            {
                currentState[i] = _currentPreset.Tiles[i];
            }
            _redoStack.Push(currentState);
            
            // 이전 상태 복원
            TileData[] previousState = _undoStack.Pop();
            ApplyState(previousState);
        }
        
        private void Redo()
        {
            if (_redoStack.Count == 0) return;
            
            // 현재 상태를 언두 스택에 저장
            TileData[] currentState = new TileData[_currentPreset.Tiles.Count];
            for (int i = 0; i < currentState.Length; i++)
            {
                currentState[i] = _currentPreset.Tiles[i];
            }
            _undoStack.Push(currentState);
            
            // 리두 상태 복원
            TileData[] redoState = _redoStack.Pop();
            ApplyState(redoState);
        }
        
        private void ApplyState(TileData[] state)
        {
            int width = _currentPreset.Width;
            
            for (int i = 0; i < state.Length; i++)
            {
                int x = i % width;
                int y = i / width;
                _currentPreset.SetTile(x, y, state[i]);
            }
            
            EditorUtility.SetDirty(_currentPreset);
        }
        
        private void ClearUndoRedo()
        {
            _undoStack.Clear();
            _redoStack.Clear();
        }
        
        #endregion
    }
}
#endif
