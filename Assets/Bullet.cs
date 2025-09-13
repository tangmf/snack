using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    public int damage = 1;
    public float lifetime = 4f; // bullet auto-destroys after 4 seconds

    private void Start()
    {
        // Destroy after lifetime
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Move forward in local right direction
        transform.Translate(Vector2.right * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Example: if bullet hits a player
        if (collision.CompareTag("Player"))
        {
            PlayerManager pm = collision.GetComponent<PlayerManager>();
            if (pm != null)
            {
                pm.TakeDamage(20); // call player’s damage function
            }
            Destroy(gameObject); // destroy bullet on hit
        }

        // Example: if bullet hits an enemy
        if (collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }

        // Add walls or obstacles
        if (collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
        }
    }
}
