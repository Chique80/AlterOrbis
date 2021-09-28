using System.Collections.Generic;
using Core;
using UnityEngine;

namespace Objects
{
    public class BulletPool : ObjectPool<BulletPool, BulletObject, Vector2>
    {
        private static readonly Dictionary<GameObject, BulletPool> PoolInstances = new Dictionary<GameObject, BulletPool>();

        private void Awake()
        {
            //This allow to make Pool manually added in the scene still automatically findable & usable
            if(prefab != null && !PoolInstances.ContainsKey(prefab))
                PoolInstances.Add(prefab, this);
        }

        private void OnDestroy()
        {
            PoolInstances.Remove(prefab);
        }

        //initialPoolCount is only used when the object pool don't exist
        public static BulletPool GetObjectPool(GameObject prefab, int initialPoolCount = 10)
        {
            if (PoolInstances.TryGetValue(prefab, out var objPool)) return objPool;
            var obj = new GameObject(prefab.name + "_Pool");
            objPool = obj.AddComponent<BulletPool>();
            objPool.prefab = prefab;
            objPool.initialPoolCount = initialPoolCount;

            PoolInstances[prefab] = objPool;

            return objPool;
        }
    }

    public class BulletObject : PoolObject<BulletPool, BulletObject, Vector2>
    {
        private Transform _transform;
        public Rigidbody2D Rigidbody2D;
        public SpriteRenderer SpriteRenderer;
        public Bullet Bullet;

        protected override void SetReferences()
        {
            _transform = instance.transform;
            Rigidbody2D = instance.GetComponent<Rigidbody2D> ();
            SpriteRenderer = instance.GetComponent<SpriteRenderer> ();
            Bullet = instance.GetComponent<Bullet>();
            Bullet.BulletPoolObject = this;
            Bullet.mainCamera = Object.FindObjectOfType<Camera> ();
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
