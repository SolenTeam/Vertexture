using System;
using System.Collections.Generic;
using System.Linq;

namespace VertextureEditor
{
    // Central scene registry - holds all game objects
    public static class Scene
    {
        private static readonly List<GameObject> _objects = new();
        private static int _nextId = 1;

        public static IReadOnlyList<GameObject> Objects => _objects;

        public static GameObject CreateObject(string name, string type)
        {
            var go = new GameObject
            {
                Name = name,
                Type = type,
                Id = _nextId++
            };
            _objects.Add(go);
            return go;
        }

        public static void RemoveObject(GameObject obj)
        {
            _objects.Remove(obj);
        }

        public static GameObject? GetById(int id)
        {
            return _objects.FirstOrDefault(o => o.Id == id);
        }

        public static void Clear()
        {
            _objects.Clear();
            _nextId = 1;
        }
    }

    public class GameObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = "Unnamed";
        public string Type { get; set; } = "cube";
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float PositionZ { get; set; }
        public float ScaleX { get; set; } = 1f;
        public float ScaleY { get; set; } = 1f;
        public float ScaleZ { get; set; } = 1f;
        public float RotationX { get; set; }
        public float RotationY { get; set; }
        public float RotationZ { get; set; }

        public string DisplayName => Type switch
        {
            "cube" => $"◆ {Name}",
            "sphere" => $"● {Name}",
            "light" => $"☀ {Name}",
            "camera" => $"◉ {Name}",
            _ => $"■ {Name}"
        };
    }
}
