using System;
using System.Runtime.Serialization;
using UnityEngine;

public class BulletTarget : MonoBehaviour
{
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
        Debug.Log("Raycast Hit");
        OnTargetHit?.Invoke(25);
        Destroy(gameObject);
    }
}
