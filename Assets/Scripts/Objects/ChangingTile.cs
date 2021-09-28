using System;
using SceneManagement;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Objects
{
    [Serializable]
    [CreateAssetMenu(menuName = "Changing Tile")]
    public class ChangingTile : Tile
    {
        
        public Sprite organic;
        public Sprite mechanic;
        

        public override void GetTileData(Vector3Int location, ITilemap tileMap, ref TileData tileData)
        {
            base.GetTileData(location, tileMap, ref tileData);

            if (!GameObject.Find("AssetsRefresh")) return;
            if (!GameObject.Find("AssetsRefresh").GetComponent<AssetsRefresh>()) return;
            switch (GameObject.Find("AssetsRefresh").GetComponent<AssetsRefresh>().currentWorldType)
            {
                case AssetsRefresh.WorldType.Organic:
                    tileData.sprite = organic;
                    break;
                case AssetsRefresh.WorldType.Mechanic:
                    tileData.sprite = mechanic;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


        }
    }
}
