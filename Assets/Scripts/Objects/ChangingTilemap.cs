using UnityEngine;
using UnityEngine.Tilemaps;

namespace Objects
{
    public class ChangingTilemap : MonoBehaviour
    {
        private Tilemap _tilemap;
        

        private void Awake()
        {
            _tilemap = GetComponent<Tilemap>();
            RefreshMap();
        }
        
        private void Start()
        {
            _tilemap = GetComponent<Tilemap>();
            RefreshMap();
        }
 
        public void RefreshMap()
        {
            if (_tilemap == null)
            {
                _tilemap = GetComponent<Tilemap>();
            }
                
            if (_tilemap != null)
            {
                _tilemap.RefreshAllTiles();
            }
        }
    }
}
