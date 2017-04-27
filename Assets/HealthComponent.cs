using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HealthComponent : MonoBehaviour
{
    public int health;
    public UnityEvent onDeath;

    public void TakeDamage(int amount)
    {
        if (health <= 0)
            return;

        health -= amount;

        if(health <= 0)
        {
            onDeath.Invoke();

            GameObject.Destroy(gameObject);
        }
    }
}
