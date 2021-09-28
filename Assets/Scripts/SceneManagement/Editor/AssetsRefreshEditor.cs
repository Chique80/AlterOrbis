using UnityEditor;
using static SceneManagement.AssetsRefresh;

namespace SceneManagement.Editor
{
    [CustomEditor(typeof(AssetsRefresh))]
    public class AssetsRefreshEditor : UnityEditor.Editor
    {
        private AssetsRefresh _assetsRefresh;
        
        private void OnEnable()
        {
          
            _assetsRefresh = target as AssetsRefresh;
            
        }

        public override void OnInspectorGUI()
        {
            
            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            var hasSwitchedWorld = EditorGUILayout.Toggle("Is world Switch", _assetsRefresh.hasSwitchedWorld);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed world switch");
                _assetsRefresh.hasSwitchedWorld = hasSwitchedWorld;
                EditorUtility.SetDirty(_assetsRefresh);
            }
            
            EditorGUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();
            _assetsRefresh.currentWorldType =
                (WorldType) EditorGUILayout.EnumPopup("World type", _assetsRefresh.currentWorldType);
            if (!EditorGUI.EndChangeCheck()) return;
            _assetsRefresh.Awake();
            ChangeWorld(_assetsRefresh.currentWorldType);
            EditorUtility.SetDirty(target);
            
            
            //Undo.RecordObject(target, "Changed world type");


        }

    }
}
