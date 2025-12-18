using System;
using System.Collections.Generic;
using UnityEngine;

public class KeyManager : MonoBehaviour
{
    public static KeyManager Instance { get; private set; }

    public event Action<string> OnKeyCollected;
    HashSet<string> collected = new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool IsCollected(string id) => collected.Contains(id);

    public void CollectKey(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (collected.Add(id))
        {
            Debug.Log($"Key collected: {id}");
            OnKeyCollected?.Invoke(id);
        }
    }
}