using Objects.Platforms;
using UnityEditor;
using UnityEngine;

namespace Objects.Editor
{
    public static class MovingPlatformPreview 
    {
        public static GameObject Preview;

        private static MovingPlatform _movingPlatform;

        static MovingPlatformPreview()
        {
            Selection.selectionChanged += SelectionChanged;
        }

        private static void SelectionChanged()
        {
            if (_movingPlatform != null && Selection.activeGameObject != _movingPlatform.gameObject)
            {
                DestroyPreview();
            }
        }

        public static void DestroyPreview()
        {
            if (Preview == null)
                return;

            Object.DestroyImmediate(Preview);
            Preview = null;
            _movingPlatform = null;
        }

        public static void CreateNewPreview(MovingPlatform origin)
        {
            if(Preview != null)
            {
                Object.DestroyImmediate(Preview);
            }

            _movingPlatform = origin; 

            Preview = Object.Instantiate(origin.gameObject);
            Preview.hideFlags = HideFlags.DontSave;
            var plt = Preview.GetComponentInChildren<MovingPlatform>();
            Object.DestroyImmediate(plt);


            var c = new Color(0.2f, 0.2f, 0.2f, 0.4f);
            var spriteRenderers = Preview.GetComponentsInChildren<SpriteRenderer>();
            foreach (var renderer in spriteRenderers)
                renderer.color = c;
        }
    }
}