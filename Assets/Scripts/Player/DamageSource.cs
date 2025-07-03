using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSource : MonoBehaviour
{
    [SerializeField] private int damageAmount;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Goblin goblin = other.GetComponent<Goblin>();
            if (goblin != null)
            {
                goblin.TakeDamage(damageAmount, transform.position);
            }
        }
    }
}
