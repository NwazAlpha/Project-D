using UnityEngine;

namespace PuzzleGame.Module
{
    /// <summary>
    /// 프로젝트 전용 로깅 헬퍼
    /// </summary>
    public static class LogHelper
    {
        private const string TAG = "[PuzzleGame]";
        
        public static void Log(string message)
            => Debug.Log($"{TAG} {message}");
        
        public static void LogWarning(string message)
            => Debug.LogWarning($"{TAG} {message}");
        
        public static void LogError(string message)
            => Debug.LogError($"{TAG} {message}");
        
        public static void Log(string category, string message)
            => Debug.Log($"{TAG}[{category}] {message}");
    }
}
