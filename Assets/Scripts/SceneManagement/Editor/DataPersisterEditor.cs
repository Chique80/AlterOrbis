using UnityEditor;

namespace SceneManagement.Editor
{
    public abstract class DataPersisterEditor : UnityEditor.Editor
    {
        private IDataPersister _mDataPersister;

        protected virtual void OnEnable()
        {
            _mDataPersister = (IDataPersister)target;
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            DataPersisterGUI (_mDataPersister);
        }

        private static void DataPersisterGUI (IDataPersister dataPersister)
        {
            var dataSettings = dataPersister.GetDataSettings ();

            var persistenceType = (DataSettings.PersistenceType)EditorGUILayout.EnumPopup ("Persistence Type", dataSettings.persistenceType);
            var dataTag = EditorGUILayout.TextField ("Data Tag", dataSettings.dataTag);

            dataPersister.SetDataSettings (dataTag, persistenceType);
        }
    }
}