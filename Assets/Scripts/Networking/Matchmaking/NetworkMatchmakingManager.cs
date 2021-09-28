using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.CharacterSelection;
using UnityEngine.Events;

namespace Mirror.Matchmaking
{
    public abstract class NetworkMatchmakingManager : MonoBehaviour
    {
        public static NetworkMatchmakingManager singleton { get; private set; }

        [Header("Match Info")]      // => Use a Match class instead
        public bool randomMatchName = true;
        public string matchName = "99999";
        public int matchNameSize = 5;

        //Callback => improve the system to support more versatile callback
        [HideInInspector] public UnityEvent OnCreateMatchEvent;
        [HideInInspector] public UnityEvent OnSuccessfulCreateMatchEvent;
        [HideInInspector] public UnityEvent OnFailedCreateMatchEvent;

        [HideInInspector] public UnityEvent OnSearchMatchEvent;
        [HideInInspector] public UnityEvent OnMatchNotFoundEvent;
        [HideInInspector] public UnityEvent OnMatchFoundEvent;
        [HideInInspector] public UnityEvent OnJoinMatchEvent;

        protected void Awake()
        {
            InitializeSingleton();
        }

        protected virtual void InitializeSingleton()
        {
            if (singleton != null && singleton == this) return;

            if (singleton != null)
            {
                Debug.LogWarning("Multiple NetworkMatchmakingManager detected in the scene. Only one NetworkMatchmakingManager can exist at a time. The duplicate NetworkMatchmakingManager will be destroyed.");
                Destroy(gameObject);

                // Return false to not allow collision-destroyed second instance to continue.
                return;
            }

            Debug.Log("NetworkMatchmakingManager created singleton (DontDestroyOnLoad)");
            singleton = this;
            DontDestroyOnLoad(gameObject);
        }


        #region Abstract

        public abstract void CreateMatch();

        public abstract void SearchMatch(string matchName);

        #endregion

        #region Callback

        public virtual void OnCreateMatch(string matchName) => OnCreateMatchEvent.Invoke();
        public virtual void OnSuccessfulCreateMatch(string matchName) => OnSuccessfulCreateMatchEvent.Invoke();
        public virtual void OnFailedCreateMatch() => OnFailedCreateMatchEvent.Invoke();

        public virtual void OnSearchMatch(string matchName) => OnSearchMatchEvent.Invoke();
        public virtual void OnMatchNotFound(string matchName) => OnMatchNotFoundEvent.Invoke();
        public virtual void OnMatchFound(string matchName) => OnMatchFoundEvent.Invoke();

        public virtual void OnJoinMatch(string matchName) => OnJoinMatchEvent.Invoke();

        #endregion

        protected string GenerateRandomMatchName(int size) 
        {
            string name = "";
            for(int i = 0; i< size; i++)
            {
                name += UnityEngine.Random.Range(0,10).ToString();
            }

            return name;
        }
    }
}

