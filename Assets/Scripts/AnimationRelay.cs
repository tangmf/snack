using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationRelay : MonoBehaviour
{
    public PlayerController player;

    public void EndInteractAnimation()
    {
        if (player != null)
            player.EndInteractAnimation();
    }
}