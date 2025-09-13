using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Consumable : MonoBehaviour
{
    [Header("Consumable Settings")]
    public float healAmount = 20f;
    public int scoreValue = 10;
    
    private bool hasBeenCollected = false;

    public void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the colliding object has a PlayerManager component
        PlayerManager playerManager = collision.GetComponent<PlayerManager>();
        
        if (playerManager != null && !hasBeenCollected)
        {
            hasBeenCollected = true;
            
            Debug.Log("Player collected consumable: " + gameObject.name);
            
            // Heal the player
            float healthBefore = playerManager.GetCurrentHealth();
            playerManager.Heal(healAmount);
            float healthAfter = playerManager.GetCurrentHealth();
            Debug.Log($"HEALTH INCREASED: {healthBefore} -> {healthAfter} (+{healAmount})");
            
            // Find GameManager and add score
            GameManager gameManager = FindObjectOfType<GameManager>();
            if (gameManager != null)
            {
                int scoreBefore = gameManager.score;
                gameManager.AddScore(scoreValue);
                Debug.Log($"SCORE INCREASED: {scoreBefore} -> {gameManager.score} (+{scoreValue})");
            }
            else
            {
                Debug.LogError("No GameManager found in scene!");
            }
            
            // Destroy the consumable
            Debug.Log("Destroying consumable: " + gameObject.name);
            Destroy(gameObject);
        }
    }
}