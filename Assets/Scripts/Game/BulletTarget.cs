using System;
using UnityEngine;

public class BulletTarget : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int scoreValue = 25;

    public static event Action<int> OnTargetHit;

    public void OnRaycastHit()
    {
        OnTargetHit?.Invoke(scoreValue);
        Destroy(gameObject);
    }
}
