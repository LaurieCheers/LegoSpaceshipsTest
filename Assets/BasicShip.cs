using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicShip : MonoBehaviour
{
    public GameObject bulletPrefab;
    public LineRenderer qBeam;

    Vector3 velocity;

    const float THRUST_MIN_MOUSE_DIST = 10.01f;
    const float THRUST = 0.005f;
    const float DRAG = 0.9f;
    const float BULLET_SPEED = 0.4f;
    int shotsQueued;

    public void Start()
    {
        qBeam.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
        qBeam.enabled = false;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Bullet newBullet = GameObject.Instantiate(bulletPrefab).GetComponent<Bullet>();
            newBullet.transform.position = transform.position;
            newBullet.velocity = velocity + transform.up * BULLET_SPEED;
            newBullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, newBullet.velocity);
            newBullet.DieAtTimestamp = Time.time + 1.0f;
        }
    }

    private void FixedUpdate()
    {
        Vector3 mouseTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseOffset = mouseTarget - transform.position;

        if (mouseOffset.sqrMagnitude > THRUST_MIN_MOUSE_DIST * THRUST_MIN_MOUSE_DIST)
        {
            Vector3 idealEuler = Quaternion.LookRotation(Vector3.forward, mouseOffset).eulerAngles;
            idealEuler.y = 0;
            idealEuler.x = 0;
            transform.rotation = Quaternion.Euler(idealEuler);// Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(idealEuler), 5.0f);
        }

        if (Input.GetMouseButton(0))
        {
            velocity += mouseOffset*THRUST;
        }

        if(Input.GetKey(KeyCode.Q))
        {
            Vector3 beamDirection = mouseOffset;
            beamDirection.z = 0;
            beamDirection.Normalize();
            Vector3 endPoint = transform.position + beamDirection * 100;
            RaycastHit2D hit = Physics2D.Linecast(transform.position + beamDirection*0.5f, endPoint);
            if(hit.collider != null)
            {
                HealthComponent targetHealth = hit.collider.gameObject.GetComponent<HealthComponent>();
                if(targetHealth != null)
                {
                    targetHealth.TakeDamage(1);
                }
                qBeam.SetPositions(new Vector3[] { transform.position, hit.point });
                qBeam.enabled = true;
            }
            else
            {
                qBeam.SetPositions(new Vector3[] { transform.position, endPoint });
                qBeam.enabled = true;
            }
        }
        else
        {
            qBeam.enabled = false;
        }

        velocity *= DRAG;
        velocity.z = 0;

        transform.position += velocity;
    }
}
