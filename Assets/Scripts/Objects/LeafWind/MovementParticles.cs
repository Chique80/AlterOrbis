using UnityEngine;

namespace Objects.LeafWind
{
    public class MovementParticles : MonoBehaviour
    {

        public float speed = 15;
        public float maxDistance = 20;
        public bool rotate = false;
        public bool right = false;

        public float sizeLoop = 5;
        public float timeMax = 0.18f;
        
        
        public float loopDistanceMin = 0.5f;
        public float loopDistanceMax = 10.0f;
        public bool noLoop = false;

        
        private float timer = 0.0f;
        private float startLoop;
        private float easeIn = 1.0f;
        private float easeOut = 1.0f;
        private bool _loop;
        private float _distance = 0;
    
        // Start is called before the first frame update
        void Start()
        {
            if (noLoop)
            {
                startLoop = maxDistance;
            }
            else
            {
                loopDistanceMin = maxDistance / 20;
                loopDistanceMax = maxDistance * 3 / 4;
                startLoop = Random.Range(loopDistanceMin, loopDistanceMax); 
            }
        }

        // Update is called once per frame
        void Update()
        {
            Vector2 speedComponents = new Vector2(speed,0);


            if (_distance > startLoop) _loop = true;
        
            if (_loop && timer < timeMax)
            {
                if (timer < timeMax/2.0f)
                {
                    float x = FirstHalfX(timer/timeMax*2.0f).y;
                    speedComponents.x = x;
                } else
                {
                    float x = SecondHalfX((timer-timeMax/2.0f)/timeMax*2.0f).y;
                    speedComponents.x = x;
                }


                if (timer < timeMax/4.0f)
                {
                    float y = FirstThirdY(timer/timeMax * 4.0f).y;
                    speedComponents.y = y;
                }
                else if (timer < timeMax*3.0f/4.0f)
                {
                    float y = SecondThirdY((timer-timeMax/4.0f)/timeMax*2.0f).y;
                    speedComponents.y = y;
                }
                else
                {
                    float y = LastThirdY((timer - 3.0f * timeMax / 4.0f) / timeMax * 4.0f).y;
                    speedComponents.y = y;

                }

                speedComponents = speedComponents  * speed * sizeLoop;
                if (right)
                    speedComponents.y *= -1;

                timer += Time.deltaTime;
            }

            Vector3 translation;
            if (rotate)
            {
                translation = new Vector3(-speedComponents.y, speedComponents.x, 0) * Time.deltaTime;  
                _distance += translation.y;
            }
            else
            {
                translation = new Vector3(speedComponents.x, speedComponents.y, 0) * Time.deltaTime;   
                _distance += translation.x;
            }


            transform.Translate(translation);
            if (_distance > maxDistance)
            {
                Destroy(this);
            }
        
        }

        Vector2 FirstHalfX(float t)
        {
            Vector2 a = new Vector2(0, 1);
            Vector2 b = new Vector2(easeIn, 1);
            Vector2 c = new Vector2(1 - easeOut, -1);
            Vector2 d = new Vector2(1, -1);

            return Hermite(a, b, c, d, t);
        }
    
        Vector2 SecondHalfX(float t)
        {
            Vector2 a = new Vector2(0, -1);
            Vector2 b = new Vector2(easeIn, -1);
            Vector2 c = new Vector2(1 - easeOut, 1);
            Vector2 d = new Vector2(1, 1);

            return Hermite(a, b, c, d, t);
        }
    
        Vector2 FirstThirdY(float t)
        {

            Vector2 a = new Vector2(0, 0);
            Vector2 b = new Vector2(easeIn, 0);
            Vector2 c = new Vector2(0, 1);
            Vector2 d = new Vector2(1, 1);
        
            return Hermite(a, b, c, d, t);
        }
    
        Vector2 SecondThirdY(float t)
        {

            Vector2 a = new Vector2(0, 1);
            Vector2 b = new Vector2(easeIn, 1);
            Vector2 c = new Vector2(1 - easeOut, -1);
            Vector2 d = new Vector2(1, -1);
        
            return Hermite(a, b, c, d, t);
        }
    
    
        Vector2 LastThirdY(float t)
        {
            Vector2 a = new Vector2(0, -1);
            Vector2 b = new Vector2(easeIn, -1);
            Vector2 c = new Vector2((1 - easeOut), 0);
            Vector2 d = new Vector2(1, 0);
        
            return Hermite(a, b, c, d, t);
        }

        Vector2 Hermite(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
        {
        
            Vector2 ab = (a*(1-t)) + (b*t);
            Vector2 bc = (b*(1-t)) + (c*t);
            Vector2 cd = (c*(1-t)) + (d*t);

            Vector2 ac = (ab*(1-t)) + (bc*t);
            Vector2 bd = (bc*(1-t)) + (cd*t);

            Vector2 ad = (ac*(1-t)) + (bd*t);
            return ad;


        }
    }
}

