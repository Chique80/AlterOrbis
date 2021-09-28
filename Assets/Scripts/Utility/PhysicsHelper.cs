using System.Collections.Generic;
using Audio;
using Objects.Platforms;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Utility
{
    public class PhysicsHelper : MonoBehaviour
    {
        private static PhysicsHelper _instance;

        private static PhysicsHelper Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<PhysicsHelper> ();

                if (_instance != null)
                    return _instance;
            
                Create ();
            
                return _instance;
            }
        }

        private static void Create ()
        {
            var physicsHelperGameObject = new GameObject("PhysicsHelper");
            _instance = physicsHelperGameObject.AddComponent<PhysicsHelper> ();
        }

        private readonly Dictionary<Collider2D, MovingPlatform> _mMovingPlatformCache = new Dictionary<Collider2D, MovingPlatform> ();
        private readonly Dictionary<Collider2D, PlatformEffector2D> _mPlatformEffectorCache = new Dictionary<Collider2D, PlatformEffector2D> ();

        private readonly Dictionary<Collider2D, Tilemap> _mTilemapCache = new Dictionary<Collider2D, Tilemap> ();
        private readonly Dictionary<Collider2D, AudioSurface> _mAudioSurfaceCache = new Dictionary<Collider2D, AudioSurface> ();

        private void Awake ()
        {
            if (Instance != this)
            {
                Destroy (gameObject);
                return;
            }
        
            PopulateColliderDictionary (_mMovingPlatformCache);
            PopulateColliderDictionary (_mPlatformEffectorCache);
            PopulateColliderDictionary (_mTilemapCache);
            PopulateColliderDictionary (_mAudioSurfaceCache);
        }

        private static void PopulateColliderDictionary<TComponent> (IDictionary<Collider2D, TComponent> dict)
            where TComponent : Component
        {
            var components = FindObjectsOfType<TComponent> ();

            foreach (var component in components)
            {
                var componentColliders = component.GetComponents<Collider2D> ();

                foreach (var componentCollider in componentColliders)
                {
                    dict.Add (componentCollider, component);
                }
            }
        }

        public static bool ColliderHasMovingPlatform (Collider2D collider)
        {
            return Instance._mMovingPlatformCache.ContainsKey (collider);
        }

        public static bool ColliderHasPlatformEffector (Collider2D collider)
        {
            return Instance._mPlatformEffectorCache.ContainsKey (collider);
        }

        public static bool ColliderHasTilemap (Collider2D collider)
        {
            return Instance._mTilemapCache.ContainsKey (collider);
        }

        public static bool ColliderHasAudioSurface (Collider2D collider)
        {
            return Instance._mAudioSurfaceCache.ContainsKey (collider);
        }

        public static bool TryGetMovingPlatform (Collider2D collider, out MovingPlatform movingPlatform)
        {
            return Instance._mMovingPlatformCache.TryGetValue (collider, out movingPlatform);
        }

        public static bool TryGetPlatformEffector (Collider2D collider, out PlatformEffector2D platformEffector)
        {
            return Instance._mPlatformEffectorCache.TryGetValue (collider, out platformEffector);
        }

        private static bool TryGetTilemap (Collider2D collider, out Tilemap tilemap)
        {
            return Instance._mTilemapCache.TryGetValue (collider, out tilemap);
        }

        private static bool TryGetAudioSurface (Collider2D collider, out AudioSurface audioSurface)
        {
            return Instance._mAudioSurfaceCache.TryGetValue (collider, out audioSurface);
        }

        public static TileBase FindTileForOverride(Collider2D collider, Vector2 position, Vector2 direction)
        {
            if (TryGetTilemap (collider, out var tilemap))
            {
                return tilemap.GetTile(tilemap.WorldToCell(position + direction * 0.4f));
            }

            return TryGetAudioSurface (collider, out var audioSurface) ? audioSurface.tile : null;
        }
    }
}
