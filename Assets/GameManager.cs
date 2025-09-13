using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int score = 0;
    public TMP_Text scoreText;
    // Start is called before the first frame update
    void Start()
    {
        UpdateUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddScore(int amount)
    {
        score += amount;
        scoreText.text = "Score: " + score;
        UpdateUI();
    }

    public void UpdateUI()
    {
        scoreText.text = "Score: " + score;
    }
}
