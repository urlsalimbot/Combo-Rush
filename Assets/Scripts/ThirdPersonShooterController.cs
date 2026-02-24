using UnityEngine;
using Unity.Cinemachine;
using System;
using StarterAssets;

public class ThirdPersonShooterController : MonoBehaviour
{
    [SerializeField] private CinemachineCamera aimVirtualCamera;
    [SerializeField] private float normalSens = 1.0f;
    [SerializeField] private float aimSens = 0.65f;
    [SerializeField] private LayerMask aimColliderMask = new LayerMask();
    [SerializeField] private Transform debugTransform;


    [SerializeField] private float fireCooldownDuration = 1.0f;

    private float currFireCooldown = 0f;
    private ThirdPersonController thirdPersonController;
    private StarterAssetsInputs starterAssetsInputs;


    private void Awake()
    {
        thirdPersonController = GetComponent<ThirdPersonController>();
        starterAssetsInputs = GetComponent<StarterAssetsInputs>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (starterAssetsInputs.aim)
        {
            aimVirtualCamera.gameObject.SetActive(true);
            thirdPersonController.SetSensitivity(aimSens);
        }
        else
        {
            aimVirtualCamera.gameObject.SetActive(false);
            thirdPersonController.SetSensitivity(normalSens);
        }

        if (currFireCooldown > 0)
        {
            starterAssetsInputs.fire = false;
            currFireCooldown -= Time.deltaTime;


        }

        // Check for input and if the cooldown is ready
        if (starterAssetsInputs.fire && currFireCooldown <= 0)
        {
            FireRay();
            // Reset the cooldown timer
            currFireCooldown = fireCooldownDuration;

        }
    }

    private void FireRay()
    {
        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = Camera.main.ScreenPointToRay(screenCenterPoint);
        // Transform hitTransform = null;
        if (Physics.Raycast(ray, out RaycastHit raycastHit, 999f, aimColliderMask))
        {
            debugTransform.position = raycastHit.point;

            // 2. Use TryGetComponent for better performance and safety
            if (raycastHit.transform.TryGetComponent<BulletTarget>(out BulletTarget target))
            {
                target.OnRaycastHit(); // Matches the public method name
            }
        }
    }
}
