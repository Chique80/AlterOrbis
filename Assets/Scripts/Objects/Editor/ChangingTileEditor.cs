using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace Objects.Editor
{
    [CustomEditor(typeof(ChangingTile))]
    public class ChangingTileEditor : UnityEditor.Editor
    {
        private ChangingTile Tile => (target as ChangingTile);


        public override void OnInspectorGUI()
        {
            EditorGUI.BeginChangeCheck();
            Tile.organic = EditorGUILayout.ObjectField("Organic Sprite", Tile.organic, typeof(Sprite), false) as Sprite;
            Tile.sprite = Tile.organic;
            Tile.mechanic =
                EditorGUILayout.ObjectField("Mechanic Sprite", Tile.mechanic, typeof(Sprite), false) as Sprite;
            Tile.colliderType = (Tile.ColliderType) EditorGUILayout.EnumPopup("Default Collider", Tile.colliderType);
            if (!EditorGUI.EndChangeCheck()) return;
            EditorUtility.SetDirty(target);
            Undo.RecordObject(target, "Changed tile");

        }

        public override Texture2D RenderStaticPreview(string assetPath, Object[] subAssets, int width, int height)
        {
            if (Tile.sprite == null) return base.RenderStaticPreview(assetPath, subAssets, width, height);
            var t = GetType("UnityEditor.SpriteUtility");
            if (t == null) return base.RenderStaticPreview(assetPath, subAssets, width, height);
            var method = t.GetMethod("RenderStaticPreview",
                new[] {typeof(Sprite), typeof(Color), typeof(int), typeof(int)});
            if (method == null) return base.RenderStaticPreview(assetPath, subAssets, width, height);
            var ret = method.Invoke("RenderStaticPreview", new object[] {Tile.sprite, Color.white, width, height});
            if (ret is Texture2D texture2D)
                return texture2D;

            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }

        private static Type GetType(string typeName)
        {
            var type = Type.GetType(typeName);
            if (type != null)
                return type;

            if (typeName.Contains("."))
            {
                var assemblyName = typeName.Substring(0, typeName.IndexOf('.'));
                var assembly = Assembly.Load(assemblyName);
                if (assembly == null)
                    return null;
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            var currentAssembly = Assembly.GetExecutingAssembly();
            var referencedAssemblies = currentAssembly.GetReferencedAssemblies();
            foreach (var assemblyName in referencedAssemblies)
            {
                var assembly = Assembly.Load(assemblyName);
                if (assembly == null) continue;
                type = assembly.GetType(typeName);
                if (type != null)
                    return type;
            }

            return null;
        }
    }
}