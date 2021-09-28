using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;
using UnityEngine.Tilemaps;
using Utility;

namespace Characters.MonoBehaviours
{
    [System.Serializable]
    public class VFX
    {
        [System.Serializable]
        public class VFXOverride
        {
            public TileBase tile;
            public GameObject prefab;
        }

        public GameObject prefab;
        public float lifetime = 1;
        public VFXOverride[] vfxOverride;

        [System.NonSerialized] public VFXInstancePool Pool;
        [System.NonSerialized] public Dictionary<TileBase, VFXInstancePool> VFXOverrideDictionary;
    }

    public class VFXInstance : PoolObject<VFXInstancePool, VFXInstance>, System.IComparable<VFXInstance>
    {
        public float Expires;
        private Animation _animation;
        private AudioSource _audioSource;
        private ParticleSystem[] _particleSystems;
        public Transform Transform;
        public Transform Parent;

        protected override void SetReferences()
        {
            Transform = instance.transform;
            _animation = instance.GetComponentInChildren<Animation>();
            _audioSource = instance.GetComponentInChildren<AudioSource>();
            _particleSystems = instance.GetComponentsInChildren<ParticleSystem>();
        }

        public override void WakeUp()
        {
            if (instance != null)
            {
                instance.SetActive(true);
            }

            if (_particleSystems != null)
            {
                foreach (var system in _particleSystems)
                    if (system != null)
                    {
                        system.Play();
                    }
            }

            if (_animation != null)
            {
                _animation.Rewind();
                _animation.Play();
            }

            if (_audioSource != null)
                _audioSource.Play();
        }

        public override void Sleep()
        {
            if (_particleSystems != null)
            {
                foreach (var system in _particleSystems)
                {
                    if (system != null)
                    {
                        system.Stop();
                    }
                }
            }


            if (_animation != null)
                _animation.Stop();
            if (_audioSource != null)
                _audioSource.Stop();
            if (instance != null)
            {
                instance.SetActive(false);
            }
        }

        public void SetPosition(Vector3 position)
        {
            if (Transform != null)
            {
                Transform.localPosition = position;
            }
            
        }

        public int CompareTo(VFXInstance other)
        {
            return Expires.CompareTo(other.Expires);
        }
    }

    public class VFXInstancePool : ObjectPool<VFXInstancePool, VFXInstance>
    {
    }

    public class VFXController : MonoBehaviour
    {
        public static VFXController Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<VFXController>();

                return _instance != null ? _instance : CreateDefault();
            }
        }

        private static VFXController _instance;

        private static VFXController CreateDefault()
        {
            var controllerPrefab = Resources.Load<VFXController>("VFXController");
            _instance = Instantiate(controllerPrefab);
            return _instance;
        }

        private struct PendingVFX : System.IComparable<PendingVFX>
        {
            public VFX VFX;
            public Vector3 Position;
            public float StartAt;
            public bool Flip;
            public Transform Parent;
            public TileBase TileOverride;

            public int CompareTo(PendingVFX other)
            {
                return StartAt.CompareTo(other.StartAt);
            }
        }


        public VFX[] vfxConfig;

        private readonly Dictionary<int, VFX> _fxPools = new Dictionary<int, VFX>();
        private readonly PriorityQueue<VFXInstance> _runningFx = new PriorityQueue<VFXInstance>();
        private readonly PriorityQueue<PendingVFX> _pendingFx = new PriorityQueue<PendingVFX>();

        public void Awake()
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            //DontDestroyOnLoad(gameObject);

            foreach (var vfx in vfxConfig)
            {
                vfx.Pool = gameObject.AddComponent<VFXInstancePool>();
                vfx.Pool.initialPoolCount = 2;
                vfx.Pool.prefab = vfx.prefab;

                vfx.VFXOverrideDictionary = new Dictionary<TileBase, VFXInstancePool>();
                foreach (var overrideVfx in vfx.vfxOverride)
                {
                    var tb = overrideVfx.tile;

                    var obj = new GameObject("vfxOverride");
                    obj.transform.SetParent(transform);
                    vfx.VFXOverrideDictionary[tb] = obj.AddComponent<VFXInstancePool>();
                    vfx.VFXOverrideDictionary[tb].initialPoolCount = 2;
                    vfx.VFXOverrideDictionary[tb].prefab = overrideVfx.prefab;
                }

                _fxPools[StringToHash(vfx.prefab.name)] = vfx;
            }
        }

        public void Trigger(string vfxName, Vector3 position, float startDelay, bool flip, Transform parent,
            TileBase tileOverride = null)
        {
            Trigger(StringToHash(vfxName), position, startDelay, flip, parent, tileOverride);
        }

        public void Trigger(int hash, Vector3 position, float startDelay, bool flip, Transform parent,
            TileBase tileOverride = null)
        {
            if (!_fxPools.TryGetValue(hash, out var vfx))
            {
                Debug.LogError("VFX does not exist.");
            }
            else
            {
                if (startDelay > 0)
                {
                    _pendingFx.Push(new PendingVFX()
                    {
                        VFX = vfx, Position = position, StartAt = Time.time + startDelay, Flip = flip, Parent = parent,
                        TileOverride = tileOverride
                    });
                }
                else
                    CreateInstance(vfx, position, flip, parent, tileOverride);
            }
        }

        private void Update()
        {
            while (!_runningFx.Empty && _runningFx.First.Expires <= Time.time)
            {
                var vfxInstance = _runningFx.Pop();
                vfxInstance.objectPool.Push(vfxInstance);
            }

            while (!_pendingFx.Empty && _pendingFx.First.StartAt <= Time.time)
            {
                var task = _pendingFx.Pop();
                CreateInstance(task.VFX, task.Position, task.Flip, task.Parent, task.TileOverride);
            }

            var instances = _runningFx._items;
            foreach (var vfx in instances.Where(vfx => vfx.Parent != null))
            {
                if (vfx.Transform != null)
                {
                    vfx.Transform.position = vfx.Parent.position;  
                }
                
            }
        }

        private void CreateInstance(VFX vfx, Vector4 position, bool flip, Transform parent, TileBase tileOverride)
        {
            if (tileOverride == null || !vfx.VFXOverrideDictionary.TryGetValue(tileOverride, out var poolToUse))
                poolToUse = vfx.Pool;

            var vfxInstance = poolToUse.Pop();

            if (vfxInstance == null) return;
            vfxInstance.Expires = Time.time + vfx.lifetime;
            if (vfxInstance.Transform != null)
            {
                vfxInstance.Transform.localScale = flip ? new Vector3(-1, 1, 1) : new Vector3(1, 1, 1); 
            }
            vfxInstance.Parent = parent;
            vfxInstance.SetPosition(position);
            _runningFx.Push(vfxInstance);

        }

        public static int StringToHash(string name)
        {
            return name.GetHashCode();
        }
    }
}