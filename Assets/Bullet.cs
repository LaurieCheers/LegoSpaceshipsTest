using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 velocity;
    public float DieAtTimestamp;

	// Update is called once per frame
	void FixedUpdate ()
    {
        transform.position += velocity;

        if(DieAtTimestamp < Time.time)
        {
            GameObject.Destroy(gameObject);
        }
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        GameObject.Destroy(gameObject);
    }
}
