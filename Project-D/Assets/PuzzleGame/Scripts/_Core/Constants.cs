namespace PuzzleGame.Core
{
    /// <summary>
    /// 게임 전역 상수 관리
    /// </summary>
    public static class Constants
    {
        public static class SceneName
        {
            public const string Start = "StartScene";
            public const string Lobby = "LobbyScene";
            public const string StageList = "StageListScene";
            public const string InGame = "InGameScene";
        }
        
        public static class AnimatorKey
        {
            public const string Remove = "Remove";
            public const string Selected = "Selected";
        }
        
        public static class PlayerPrefsKey
        {
            public const string StageClearPrefix = "StageClear_";
            public const string HighScore = "HighScore";
        }
        
        public static class PoolKey
        {
            public const string Tile = "Pool_Tile";
        }
    }
}
