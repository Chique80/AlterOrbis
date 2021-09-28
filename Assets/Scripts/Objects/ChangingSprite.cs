using System;
using SceneManagement;
using UnityEngine;

namespace Objects
{
    public class ChangingSprite : MonoBehaviour
    {
        public Sprite organic;
        public Sprite mechanic;

        private SpriteRenderer _spriteRenderer;
        private AssetsRefresh _assetsRefresh;
        
        
        protected void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();


        }



        public void Refresh()
        {
            if (_spriteRenderer == null)
            {
                _spriteRenderer = GetComponent<SpriteRenderer>();
            }
            switch (AssetsRefresh.Instance.currentWorldType)
            {
                case AssetsRefresh.WorldType.Organic:
                    _spriteRenderer.sprite = organic;
                    break;
                case AssetsRefresh.WorldType.Mechanic:
                    _spriteRenderer.sprite = mechanic;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
