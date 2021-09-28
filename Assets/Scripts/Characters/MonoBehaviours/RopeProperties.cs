using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeProperties : MonoBehaviour
{
    public GameObject object1;
    public GameObject object2;
    public bool isAttached;

    public void Start()
    {
        object1 = null;
        object2 = null;
        isAttached = false;
    }
}
