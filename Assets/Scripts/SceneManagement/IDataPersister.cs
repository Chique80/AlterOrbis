using System;

namespace SceneManagement
{
    public interface IDataPersister
    {
        DataSettings GetDataSettings();

        void SetDataSettings(string dataTag, DataSettings.PersistenceType persistenceType);

        Data SaveData();

        void LoadData(Data data);
    }
    
    
    [Serializable]
    public class DataSettings
    {
        public enum PersistenceType
        {
            DoNotPersist,
            ReadOnly,
            WriteOnly,
            ReadWrite,
        }

        public string dataTag = Guid.NewGuid().ToString();
        public PersistenceType persistenceType = PersistenceType.ReadWrite;

        public override string ToString()
        {
            return dataTag + " " + persistenceType.ToString();
        }
    }
    
    public class Data
    {

    }
    
    public class Data<T> : Data
    {
        public T Value;

        public Data(T value)
        {
            this.Value = value;
        }
    }
    
    public class Data<T0, T1> : Data
    {
        public T0 value0;
        public T1 value1;

        public Data(T0 value0, T1 value1)
        {
            this.value0 = value0;
            this.value1 = value1;
        }
    }
    
    
}
