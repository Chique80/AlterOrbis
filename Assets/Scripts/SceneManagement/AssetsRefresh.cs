using Objects;
using UnityEngine;

namespace SceneManagement
{
    public class AssetsRefresh : MonoBehaviour
    {
        public enum WorldType
        {
            Organic,
            Mechanic
        }
        
        private ChangingSprite[] _changingSprites;
        private ChangingTilemap _tilemap;
        
        public WorldType currentWorldType = WorldType.Organic;

        public bool hasSwitchedWorld;
        
        private static AssetsRefresh _instance;

        public static AssetsRefresh Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<AssetsRefresh> ();

                return _instance != null ? _instance : null;
            }
        }

        public void Awake()
        {
            if (Instance != this)
            {
                Destroy (this);
                return;
            }
            
            Instance._changingSprites = FindObjectsOfType<ChangingSprite>();
            Instance._tilemap = FindObjectOfType<ChangingTilemap>();
        }

        public static void ChangeWorld(WorldType newWorld)
        {
            /*if (Instance._changingSprites == null)
            {
                Instance._changingSprites = FindObjectsOfType<ChangingSprite>();
            }

            if (Instance._tilemap == null)
            {
                Instance._tilemap = FindObjectOfType<ChangingTilemap>();  
            }*/
            
            
            Instance.currentWorldType = newWorld;
            
            foreach (var changingSprite in Instance._changingSprites)
            {
                changingSprite.Refresh();
            }
            
            Instance._tilemap.RefreshMap();


        }
        
        public void SwitchWorld()
        {
            ChangeWorld(Instance.currentWorldType == WorldType.Organic ? WorldType.Mechanic : WorldType.Organic);
        }
    }
}
