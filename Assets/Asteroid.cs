using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Asteroid : MonoBehaviour
{
    public GameObject asteroidPrefab;
    Rigidbody2D rb2d;

    const float SPAWN_SPEED = 1.0f;
    const float CHILD_SCALE_FACTOR = 0.7f;

    private void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
    }

    public void OnDeath()
    {
        float childScale = transform.localScale.x * CHILD_SCALE_FACTOR;

        if (childScale > 0.3f && asteroidPrefab != null)
        {
            Vector2 separator = new Vector2(Random.Range(-1, 1), Random.Range(-1, 1)) * SPAWN_SPEED;
            SpawnChild(childScale, separator);
            SpawnChild(childScale, -separator);
        }
    }

    void SpawnChild(float childScale, Vector2 addVelocity)
    {
        Asteroid child = GameObject.Instantiate(asteroidPrefab).GetComponent<Asteroid>();
        child.transform.position = transform.position;
        //child.transform.localScale = new Vector3(childScale, childScale, childScale);
        //child.asteroidPrefab = asteroidPrefab;

        Rigidbody2D childRB = child.GetComponent<Rigidbody2D>();
        childRB.velocity = rb2d.velocity + addVelocity;
    }
}
