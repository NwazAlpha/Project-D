using System;
using UnityEngine;

namespace PuzzleGame.Core.DataObject
{
    /// <summary>
    /// 블로커 타입 정의
    /// </summary>
    [Serializable]
    public struct BlockerType
    {
        [SerializeField] private int _id;
        [SerializeField] private string _name;
        [SerializeField] private Sprite _icon;
        [SerializeField] private Color _color;
        
        public int Id => _id;
        public string Name => _name;
        public Sprite Icon => _icon;
        public Color Color => _color;
        
        public BlockerType(int id, string name, Sprite icon = null, Color? color = null)
        {
            _id = id;
            _name = name;
            _icon = icon;
            _color = color ?? Color.red;
        }
    }
}
