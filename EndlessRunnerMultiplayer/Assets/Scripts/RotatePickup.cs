using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatePickup : MonoBehaviour
{

    float rot_speed = 20.0f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Rotate();
    }

    void Rotate()
    {
        transform.Rotate(new Vector3(1*rot_speed*Time.deltaTime,0,0));
    }
}
