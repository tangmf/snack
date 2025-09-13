using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shoot : MonoBehaviour
{
    [Header("Shooting Settings")]
    public GameObject projectilePrefab;  // prefab of your projectile
    public Transform shootPoint;         // where the projectile spawns
    public float projectileSpeed = 10f;  // speed of the projectile

    public void Fire()
    {
        if (projectilePrefab != null && shootPoint != null)
        {
            // Instantiate projectile
            GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, shootPoint.rotation);

            // Add velocity if it has Rigidbody2D
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.velocity = shootPoint.right * projectileSpeed; // assumes shootPoint.right is forward
            }

            // Destroy projectile after 4 seconds
            Destroy(projectile, 4f);
        }
        else
        {
            Debug.LogWarning("ProjectilePrefab or ShootPoint not assigned in inspector!");
        }
    }
}
