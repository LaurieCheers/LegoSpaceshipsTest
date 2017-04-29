using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public Vector3 velocity;
    public float DieAtTimestamp;
    public float damage;
    public GameObject source;

	// Update is called once per frame
	void FixedUpdate ()
    {
        transform.position += velocity;

        if(DieAtTimestamp < Time.time)
        {
            GameObject.Destroy(gameObject);
        }
	}

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (source == collider.gameObject)
            return;

        HealthComponent health = collider.GetComponent<HealthComponent>();
        if (health != null)
        {
            health.TakeDamage(damage);
        }

        GameObject.Destroy(gameObject);
    }
}
