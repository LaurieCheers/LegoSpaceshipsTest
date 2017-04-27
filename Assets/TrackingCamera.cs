using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackingCamera : MonoBehaviour
{
    public GameObject tracking;
	
	void FixedUpdate ()
    {
        Vector3 tempPos = Vector3.Lerp(transform.position, tracking.transform.position, 0.1f);

        tempPos.z = -10;
        transform.position = tempPos;
    }
}
