using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{

    public Slider healthBar;
    public float healthPoints;
    public bool dead = false;
    // Start is called before the first frame update
    void Start()
    {
        MaxHealth();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Damage(10);
        }
    }

    public void Damage(float dmg)
    {
        if (healthPoints > 0)
        {
            healthPoints -= dmg;


            if (healthPoints <= 0)
            {
                healthPoints = 0;
                if (!dead)
                {
                    Die();
                    dead = true;
                }

            }
            else
            {
                // damaged
            }
        }


        healthBar.value = healthPoints;
    }

    public void Heal(float amt)
    {
        healthPoints += amt;
        if (healthPoints >= healthBar.maxValue)
        {
            healthPoints = healthBar.maxValue;
        }

        healthBar.value = healthPoints;
    }

    public void MaxHealth()
    {
        healthBar.maxValue = healthPoints;
        healthBar.value = healthPoints;
    }

    public void AddHealth(float health)
    {
        healthBar.maxValue += health;
    }

    public void Die()
    {

            Destroy(gameObject);

        


    }
}
