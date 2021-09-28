using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Core
{
    public abstract class ObjectPool<TPool, TObject, TInfo> : ObjectPool<TPool, TObject>
        where TPool : ObjectPool<TPool, TObject, TInfo>
        where TObject : PoolObject<TPool, TObject, TInfo>, new()
    {
        private void Start()
        {
            for (var i = 0; i < initialPoolCount; i++)
            {
                var newPoolObject = CreateNewPoolObject();
                pool.Add(newPoolObject);
            }
        }

        public virtual TObject Pop(TInfo info)
        {
            foreach (var poolObject in pool.Where(poolObject => poolObject.inPool))
            {
                poolObject.inPool = false;
                poolObject.WakeUp(info);
                return poolObject;
            }

            var newPoolObject = CreateNewPoolObject();
            pool.Add(newPoolObject);
            newPoolObject.inPool = false;
            newPoolObject.WakeUp(info);
            return newPoolObject;
        }
    }

    public abstract class ObjectPool<TPool, TObject> : MonoBehaviour
        where TPool : ObjectPool<TPool, TObject>
        where TObject : PoolObject<TPool, TObject>, new()
    {
        public GameObject prefab;
        public int initialPoolCount = 10;
        [HideInInspector]
        public List<TObject> pool = new List<TObject>();

        private void Start()
        {
            for (var i = 0; i < initialPoolCount; i++)
            {
                var newPoolObject = CreateNewPoolObject();
                pool.Add(newPoolObject);
            }
        }

        protected TObject CreateNewPoolObject()
        {
            var newPoolObject = new TObject {instance = Instantiate(prefab), inPool = true};
            //newPoolObject.instance.transform.SetParent(transform);
            newPoolObject.SetReferences(this as TPool);
            newPoolObject.Sleep();
            return newPoolObject;
        }

        public virtual TObject Pop()
        {
            foreach (var poolObject in pool.Where(poolObject => poolObject.inPool))
            {
                poolObject.inPool = false;
                poolObject.WakeUp();
                return poolObject;
            }

            var newPoolObject = CreateNewPoolObject();
            pool.Add(newPoolObject);
            newPoolObject.inPool = false;
            newPoolObject.WakeUp();
            return newPoolObject;
        }

        public virtual void Push(TObject poolObject)
        {
            poolObject.inPool = true;
            poolObject.Sleep();
        }
    }

    [Serializable]
    public abstract class PoolObject<TPool, TObject, TInfo> : PoolObject<TPool, TObject>
        where TPool : ObjectPool<TPool, TObject, TInfo>
        where TObject : PoolObject<TPool, TObject, TInfo>, new()
    {
        public virtual void WakeUp(TInfo info)
        { }
    }

    [Serializable]
    public abstract class PoolObject<TPool, TObject>
        where TPool : ObjectPool<TPool, TObject>
        where TObject : PoolObject<TPool, TObject>, new()
    {
        public bool inPool;
        public GameObject instance;
        public TPool objectPool;

        public void SetReferences(TPool pool)
        {
            objectPool = pool;
            SetReferences();
        }

        protected virtual void SetReferences()
        { }

        public virtual void WakeUp()
        { }

        public virtual void Sleep()
        { }

        public virtual void ReturnToPool()
        {
            var thisObject = this as TObject;
            objectPool.Push(thisObject);
        }
    }
}