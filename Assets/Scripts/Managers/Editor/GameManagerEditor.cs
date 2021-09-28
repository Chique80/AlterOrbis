using System;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Mirror;
using Mirror.CharacterSelection;
//using Characters.MonoBehaviours;

#if UNITY_EDITOR

[CustomEditor(typeof(GameManager), true)]
[CanEditMultipleObjects]
public class GameManagerEditor : Editor
{
    private bool initialized = false;

    private bool showTypePrefabs = true;

    GUIContent m_TypePrefabLabel;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void Init()
    {
        if(initialized) return;

        m_TypePrefabLabel = new GUIContent("Type Prefabs", "Prefabs for the Player Types");

        initialized = true;
    }

    public override void OnInspectorGUI()
    {
        Init();

        serializedObject.Update();

        DrawDefaultInspector();

        ShowTypePrefabs();
    }

    private void ShowTypePrefabs()
    {
        GameManager manager = target as GameManager;
        if(manager == null) return;

        showTypePrefabs = EditorGUILayout.Foldout(showTypePrefabs, m_TypePrefabLabel);
        if(showTypePrefabs)
        {
            EditorGUI.indentLevel += 1;

            foreach(PlayerType type in Enum.GetValues(typeof(PlayerType)))
            {
                if(type != PlayerType.None)
                {
                    //Get the current prefab for the type
                    GameObject currentPrefab = manager.GetPrefabOfType(type);

                    String label = Enum.GetName(typeof(PlayerType), type);
                    GameObject newPrefab = (GameObject) EditorGUILayout.ObjectField(label, currentPrefab, typeof(GameObject), false);

                    if(newPrefab != currentPrefab)
                    {
                        if(newPrefab != null && newPrefab.GetComponent<NetworkGamePlayer>() == null) 
                        {
                            Debug.LogError("Prefab " + newPrefab + " must have a NetworkGamePlayer script!");
                        }
                        else
                        {
                            manager.SetPrefabForType(type, newPrefab);
                        }
                    }
                }
            }
            
            EditorUtility.SetDirty(manager);

            EditorGUI.indentLevel -= 1;
        }
    }

    protected void ShowDerivedProperties(Type baseType, Type superType)
    {
        bool first = true;

        SerializedProperty property = serializedObject.GetIterator();
        bool expanded = true;
        while (property.NextVisible(expanded))
        {
            // ignore properties from base class.
            var f = baseType.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            var p = baseType.GetProperty(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            if (f == null && superType != null)
            {
                f = superType.GetField(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (p == null && superType != null)
            {
                p = superType.GetProperty(property.name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            }

            if (f == null && p == null)
            {
                if (first)
                {
                    first = false;
                    EditorGUI.BeginChangeCheck();
                    serializedObject.Update();

                    EditorGUILayout.Separator();
                }
                EditorGUILayout.PropertyField(property, true);
                expanded = false;
            }
        }
        if (!first)
        {
            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}

#endif