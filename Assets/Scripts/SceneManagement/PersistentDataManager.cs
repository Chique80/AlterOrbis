using System.Collections.Generic;
using UnityEngine;

namespace SceneManagement
{
    public class PersistentDataManager : MonoBehaviour
    {
        private static PersistentDataManager Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                _instance = FindObjectOfType<PersistentDataManager>();
                if (_instance != null)
                    return _instance;

                Create ();
                return _instance;
            }
        }

        private static PersistentDataManager _instance;
        private static bool _quitting;

        private static PersistentDataManager Create ()
        {
            var dataManagerGameObject = new GameObject("PersistentDataManager");
            _instance = dataManagerGameObject.AddComponent<PersistentDataManager>();
            return _instance;
        }

        private readonly HashSet<IDataPersister> _dataPersisters = new HashSet<IDataPersister>();
        private readonly Dictionary<string, Data> _store = new Dictionary<string, Data>();
        private event System.Action Schedule;

        void Update()
        {
            if (Schedule != null)
            {
                Schedule();
                Schedule = null;
            }
        }

        void Awake()
        {
            if (Instance != this)
                Destroy(gameObject);
        }

        void OnDestroy()
        {
            if (_instance == this)
                _quitting = true;
        }

        public static void RegisterPersister(IDataPersister persister)
        {
            var ds = persister.GetDataSettings();
            if (!string.IsNullOrEmpty(ds.dataTag))
            {
                Instance.Register(persister);
            }
        }

        public static void UnregisterPersister(IDataPersister persister)
        {
            if (!_quitting)
            {
                Instance.Unregister(persister);
            }
        }

        public static void SaveAllData()
        {
            Instance.SaveAllDataInternal();
        }

        public static void LoadAllData()
        {
            Instance.LoadAllDataInternal();
        }

        public static void ClearPersisters()
        {
            Instance._dataPersisters.Clear();
        }
        public static void SetDirty(IDataPersister dp)
        {
            Instance.Save(dp);
        }

        protected void SaveAllDataInternal()
        {
            foreach (var dp in _dataPersisters)
            {
                Save(dp);
            }
        }

        protected void Register(IDataPersister persister)
        {
            Schedule += () =>
            {
                _dataPersisters.Add(persister);
            };
        }

        protected void Unregister(IDataPersister persister)
        {
            Schedule += () => _dataPersisters.Remove(persister);
        }

        protected void Save(IDataPersister dp)
        {
            var dataSettings = dp.GetDataSettings();
            if (dataSettings.persistenceType == DataSettings.PersistenceType.ReadOnly || dataSettings.persistenceType == DataSettings.PersistenceType.DoNotPersist)
                return;
            if (!string.IsNullOrEmpty(dataSettings.dataTag))
            {
                _store[dataSettings.dataTag] = dp.SaveData();
            }
        }

        protected void LoadAllDataInternal()
        {
            Schedule += () =>
            {
                foreach (var dp in _dataPersisters)
                {
                    var dataSettings = dp.GetDataSettings();
                    if (dataSettings.persistenceType == DataSettings.PersistenceType.WriteOnly || dataSettings.persistenceType == DataSettings.PersistenceType.DoNotPersist)
                        continue;
                    if (!string.IsNullOrEmpty(dataSettings.dataTag))
                    {
                        if (_store.ContainsKey(dataSettings.dataTag))
                        {
                            dp.LoadData(_store[dataSettings.dataTag]);
                        }
                    }
                }
            };
        }
    }
}
