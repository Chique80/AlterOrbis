using Objects.Platforms;
using UnityEditor;
using UnityEngine;

namespace Objects.Editor
{
    [CustomEditor(typeof(MovingPlatform))]
    public class MovingPlatformEditor : UnityEditor.Editor
    {
        private MovingPlatform _movingPlatform;

        private float _previewPosition;

        private void OnEnable()
        {
            _previewPosition = 0;
            _movingPlatform = target as MovingPlatform;

            if (!EditorApplication.isPlayingOrWillChangePlaymode)
                MovingPlatformPreview.CreateNewPreview(_movingPlatform);
        }

        private void OnDisable()
        {
            MovingPlatformPreview.DestroyPreview();
        }

        public override void OnInspectorGUI()
        {
            _movingPlatform.defaultMaterial = EditorGUILayout.ObjectField("Default Material",
                _movingPlatform.defaultMaterial, typeof(Material), true) as Material;
            _movingPlatform.freezeMaterial = EditorGUILayout.ObjectField("Freeze Material",
                _movingPlatform.freezeMaterial, typeof(Material), true) as Material;
            EditorGUI.BeginChangeCheck();
            _movingPlatform.platformCatcher = EditorGUILayout.ObjectField("Platform Catcher",
                _movingPlatform.platformCatcher, typeof(PlatformCatcher), true) as PlatformCatcher;
            if (EditorGUI.EndChangeCheck())
                Undo.RecordObject(target, "Changed Catcher");

            EditorGUI.BeginChangeCheck();
            _previewPosition = EditorGUILayout.Slider("Preview position", _previewPosition, 0.0f, 1.0f);
            if (EditorGUI.EndChangeCheck())
            {
                MovePreview();
            }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            EditorGUILayout.BeginVertical("box");
            EditorGUI.BeginChangeCheck();
            var isStartingMoving = EditorGUILayout.Toggle("Start moving", _movingPlatform.isMovingAtStart);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed move at start");
                _movingPlatform.isMovingAtStart = isStartingMoving;
                EditorUtility.SetDirty(_movingPlatform);
            }

            if (isStartingMoving)
            {
                EditorGUI.indentLevel += 1;
                EditorGUI.BeginChangeCheck();
                var startOnlyWhenVisible =
                    EditorGUILayout.Toggle("When becoming visible", _movingPlatform.startMovingOnlyWhenVisible);
                if (EditorGUI.EndChangeCheck())
                {
                    Undo.RecordObject(target, "Changed move when visible");
                    _movingPlatform.startMovingOnlyWhenVisible = startOnlyWhenVisible;
                }

                EditorGUI.indentLevel -= 1;
            }

            EditorGUILayout.EndVertical();

            EditorGUI.BeginChangeCheck();
            var platformType =
                (MovingPlatform.MovingPlatformType) EditorGUILayout.EnumPopup("Looping", _movingPlatform.platformType);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Moving Platform type");
                _movingPlatform.platformType = platformType;
            }

            EditorGUI.BeginChangeCheck();
            var newSpeed = EditorGUILayout.FloatField("Speed", _movingPlatform.speed);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(target, "Changed Speed");
                _movingPlatform.speed = newSpeed;
            }

            EditorGUILayout.Separator();
            EditorGUILayout.Separator();

            if (GUILayout.Button("Add Node"))
            {
                Undo.RecordObject(target, "added node");


                var position = _movingPlatform.localNodes[_movingPlatform.localNodes.Length - 1] + Vector3.right;

                ArrayUtility.Add(ref _movingPlatform.localNodes, position);
                ArrayUtility.Add(ref _movingPlatform.waitTimes, 0);
                EditorUtility.SetDirty(_movingPlatform);
            }

            EditorGUIUtility.labelWidth = 64;
            var delete = -1;
            for (var i = 0; i < _movingPlatform.localNodes.Length; ++i)
            {
                EditorGUI.BeginChangeCheck();

                EditorGUILayout.BeginHorizontal();

                const int size = 64;
                EditorGUILayout.BeginVertical(GUILayout.Width(size));
                EditorGUILayout.LabelField("Node " + i, GUILayout.Width(size));
                if (i != 0 && GUILayout.Button("Delete", GUILayout.Width(size)))
                {
                    delete = i;
                }

                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical();
                var newPosition = i == 0
                    ? _movingPlatform.localNodes[i]
                    : EditorGUILayout.Vector3Field("Position", _movingPlatform.localNodes[i]);

                var newTime = EditorGUILayout.FloatField("Wait Time", _movingPlatform.waitTimes[i]);
                EditorGUILayout.EndVertical();


                EditorGUILayout.EndHorizontal();

                if (!EditorGUI.EndChangeCheck()) continue;
                Undo.RecordObject(target, "changed time or position");
                _movingPlatform.waitTimes[i] = newTime;
                _movingPlatform.localNodes[i] = newPosition;
            }
            
           

            EditorGUIUtility.labelWidth = 0;

            if (delete == -1) return;
            Undo.RecordObject(target, "Removed point moving platform");

            ArrayUtility.RemoveAt(ref _movingPlatform.localNodes, delete);
            ArrayUtility.RemoveAt(ref _movingPlatform.waitTimes, delete);
            
        }

        private void OnSceneGUI()
        {
            MovePreview();

            for (var i = 0; i < _movingPlatform.localNodes.Length; ++i)
            {
                var worldPos = Application.isPlaying
                    ? _movingPlatform.WorldNode[i]
                    : _movingPlatform.transform.TransformPoint(_movingPlatform.localNodes[i]);


                var newWorld = worldPos;
                if (i != 0)
                    newWorld = Handles.PositionHandle(worldPos, Quaternion.identity);

                Handles.color = Color.red;

                if (i == 0)
                {
                    if (_movingPlatform.platformType != MovingPlatform.MovingPlatformType.Loop)
                        continue;

                    if (Application.isPlaying)
                    {
                        Handles.DrawDottedLine(worldPos,
                            _movingPlatform.WorldNode[_movingPlatform.WorldNode.Length - 1], 10);
                    }
                    else
                    {
                        Handles.DrawDottedLine(worldPos,
                            _movingPlatform.transform.TransformPoint(
                                _movingPlatform.localNodes[_movingPlatform.localNodes.Length - 1]), 10);
                    }
                }
                else
                {
                    Handles.DrawDottedLine(worldPos,
                        Application.isPlaying
                            ? _movingPlatform.WorldNode[i - 1]
                            : _movingPlatform.transform.TransformPoint(_movingPlatform.localNodes[i - 1]), 10);

                    if (worldPos == newWorld) continue;
                    Undo.RecordObject(target, "moved point");
                    _movingPlatform.localNodes[i] = _movingPlatform.transform.InverseTransformPoint(newWorld);
                }
            }
        }

        private void MovePreview()
        {
            //compute pos from 0-1 preview pos

            if (Application.isPlaying)
                return;

            var step = 1.0f / (_movingPlatform.localNodes.Length - 1);

            var starting = Mathf.FloorToInt(_previewPosition / step);

            if (starting > _movingPlatform.localNodes.Length - 2)
                return;

            var localRatio = (_previewPosition - (step * starting)) / step;

            var localPos = Vector3.Lerp(_movingPlatform.localNodes[starting],
                _movingPlatform.localNodes[starting + 1], localRatio);

            MovingPlatformPreview.Preview.transform.position = _movingPlatform.transform.TransformPoint(localPos);

            SceneView.RepaintAll();
        }
    }
}