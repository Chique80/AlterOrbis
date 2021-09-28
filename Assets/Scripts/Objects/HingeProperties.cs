using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HingeProperties: MonoBehaviour
{
    public bool isAttachable;
    public Vector2 hingeLocation;
    public int objectNo;

    // Start is called before the first frame update
    void Start()
    {
        isAttachable = true;
        hingeLocation = new Vector2 ( 0.0F, 0.0F );
        objectNo = -1;
    }
}
