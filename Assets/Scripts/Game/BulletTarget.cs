using System;
using System.Runtime.Serialization;
using UnityEngine;

public class BulletTarget : MonoBehaviour
{

    [Header("Settings")]
    [SerializeField] private int scoreValue = 25;
    public static event Action<int> OnTargetHit;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
                
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnRaycastHit()
    {
        // Debug.Log("Raycast Hit");
        OnTargetHit?.Invoke(scoreValue);
        Destroy(gameObject);
    }
}
