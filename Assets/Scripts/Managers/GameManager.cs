using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.CharacterSelection;
using RotaryHeart.Lib.SerializableDictionary;
//using Characters.MonoBehaviours;

[System.Serializable]
public class PrefabDictionary : SerializableDictionaryBase<PlayerType, GameObject> { }

public class GameManager : MonoBehaviour
{
    /// <summary>
    ///     List the prefab associated with each PlayerType. PlayerTypes in the dictionnary will be consired "used" while others won't
    /// </summary>
    [HideInInspector] public PrefabDictionary m_PlayerPrefabs = new PrefabDictionary();

    public static GameManager singleton;

    void Awake()
    {
       InitializeSingleton(); 
    }

    // Start is called before the first frame update
    void Start()
    {
        //Add type prefabs to the NetworkManager
        foreach(PlayerType type in m_PlayerPrefabs.Keys)
        {
            GameObject obj = GetPrefabOfType(type);

            if(obj != null)
            {
                NetworkCharacterSelectionManager.singleton.spawnPrefabs.Add(obj);
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void InitializeSingleton()
    {
        if (singleton != null && singleton == this)
        {
            return;
        }

        if (singleton != null)
        {
            Debug.Log("Multiple GameManagers detected in the scene. Only one GameManager can exist at a time. The duplicate GameManager will not be used."); 
            Destroy(gameObject);
            return;
        }

        Debug.Log("GameManager created singleton");
        singleton = this;

        if (Application.isPlaying) DontDestroyOnLoad(gameObject);
    }

    public GameObject GetPrefabOfType(PlayerType type)
    {
        if(type == PlayerType.None) return null;
        if(!m_PlayerPrefabs.ContainsKey(type)) return null;

        return m_PlayerPrefabs[type];
    }

    public void SetPrefabForType(PlayerType type, GameObject prefab)
    {
        if(m_PlayerPrefabs.ContainsKey(type))       //Update the prefab for the type in the dictionary
        {
            m_PlayerPrefabs[type] = prefab;
        }
        else                                        //Add the type and prefab to the dictionary
        {
            m_PlayerPrefabs.Add(type, prefab);
        }
    }

    #region Editor
        void OnValidate()
        {
            foreach(PlayerType t in m_PlayerPrefabs.Keys)
            {
                if(m_PlayerPrefabs[t] != null && m_PlayerPrefabs[t].GetComponent<NetworkGamePlayer>() == null)
                {
                    Debug.LogError("Player prefabs must have a NetworkGamePlayer script!");
                    m_PlayerPrefabs[t] = null;
                }
            }
        }
    #endregion
}
