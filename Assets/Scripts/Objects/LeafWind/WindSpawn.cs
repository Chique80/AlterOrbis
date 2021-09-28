using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Objects.LeafWind
{
    public class WindSpawn : MonoBehaviour
    {
        public GameObject particles;
        public float spawnDelay;
        public float spawnInterval = 0.15f;
        public float speed = 15f;
        public bool noLoop;
        public float timeLoop = 0.2f;
        public float sizeLoop = 1;
        public float sizeParticle = 0.06f;
        public bool bothSides;

        private Vector3 _positionSystem = new Vector3(0,0,0);
        private float _heightSystem;
        private float _lengthSystem;
        private float _angle;
        private bool _rotate;
        private const float Tolerance = 0.01f;

        // Start is called before the first frame update
        private void Start()
        {


            var area = GetComponent<AreaEffector2D>();
            _angle = area.forceAngle;
            var boxCollider2D = GetComponent<BoxCollider2D>();

            //rotate==true si la force du vent est vers le haut
            _rotate = Math.Abs(_angle - 90.0f) < Tolerance;

            var size = boxCollider2D.size;
            _heightSystem = size.y;
            _lengthSystem = size.x;
            
            _positionSystem = gameObject.transform.position;
            InvokeRepeating(nameof(SpawnObjects), spawnDelay, spawnInterval);
        }

        private void SpawnObjects()
        {
            var spawnLocation = _positionSystem;


            if (_rotate)
            {
                var randomNumber = Random.Range(-_lengthSystem / 2.0f, _lengthSystem / 2.0f);
                if (bothSides && randomNumber > 0)
                {
                    particles.GetComponent<MovementParticles>().right = true;
                }
                else
                {
                    particles.GetComponent<MovementParticles>().right = false;   
                }

                spawnLocation.x += randomNumber;
                particles.GetComponent<MovementParticles>().maxDistance = _heightSystem;

            }else
            {
                var randomNumber = Random.Range(-_heightSystem / 2.0f, _heightSystem / 2.0f);
                if (bothSides && randomNumber > 0)
                {
                    particles.GetComponent<MovementParticles>().right = true;
                }
                else
                {
                    particles.GetComponent<MovementParticles>().right = false;   
                }

                spawnLocation.y += randomNumber;
                particles.GetComponent<MovementParticles>().maxDistance = _lengthSystem;

            }

            particles.GetComponent<MovementParticles>().rotate = _rotate;
            particles.GetComponent<MovementParticles>().speed = speed;
            particles.GetComponent<MovementParticles>().timeMax = timeLoop;
            particles.GetComponent<MovementParticles>().sizeLoop = sizeLoop;
            particles.GetComponent<TimedTrailRenderer>().size = sizeParticle;
            if (noLoop)
            {
                particles.GetComponent<MovementParticles>().noLoop = true;
            }
            Instantiate(particles, spawnLocation, particles.transform.rotation);

        }

        // Update is called once per frame
        private void Update()
        {
            _positionSystem = gameObject.transform.position;
        }
    }
}
