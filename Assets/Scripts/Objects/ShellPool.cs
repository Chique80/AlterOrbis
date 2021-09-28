using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Objects
{
    public class ShellPool : ObjectPool<ShellPool, ShellObject, Vector2>
    {
        private static readonly Dictionary<GameObject, ShellPool> PoolInstances =
            new Dictionary<GameObject, ShellPool>();

        private void Awake()
        {
            //This allow to make Pool manually added in the scene still automatically findable & usable
            if (prefab != null && !PoolInstances.ContainsKey(prefab))
                PoolInstances.Add(prefab, this);
        }

        private void OnDestroy()
        {
            PoolInstances.Remove(prefab);
        }

        //initialPoolCount is only used when the object pool don't exist
        public static ShellPool GetObjectPool(GameObject prefab, int initialPoolCount = 10)
        {
            if (PoolInstances.TryGetValue(prefab, out var objPool)) return objPool;
            var obj = new GameObject(prefab.name + "_Pool");
            objPool = obj.AddComponent<ShellPool>();
            objPool.prefab = prefab;
            objPool.initialPoolCount = initialPoolCount;

            PoolInstances[prefab] = objPool;

            return objPool;
        }
    }

    public class ShellObject : PoolObject<ShellPool, ShellObject, Vector2>
    {
        private Transform _transform;
        public Shell Shell;
        public SpriteRenderer SpriteRenderer;

        protected override void SetReferences()
        {
            _transform = instance.transform;
            SpriteRenderer = instance.GetComponent<SpriteRenderer>();
            Shell = instance.GetComponent<Shell>();
            Shell.ShellPoolObject = this;
        }

        public override void WakeUp(Vector2 position)
        {
            _transform.position = position;
            instance.SetActive(true);
        }

        public override void Sleep()
        {
            instance.SetActive(false);
        }
    }
}