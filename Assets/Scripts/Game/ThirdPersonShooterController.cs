using UnityEngine;
using Unity.Cinemachine;
using StarterAssets;

public class ThirdPersonShooterController : MonoBehaviour
{
    [Header("Aim Settings")]
    [SerializeField] private CinemachineCamera aimVirtualCamera;
    [SerializeField] private float normalSens = 1.0f;
    [SerializeField] private float aimSens = 0.65f;

    [Header("Time Settings")]
    [SerializeField] private float normalTimeScale = 1.0f;
    [SerializeField] private float slowMoTimeScale = 0.25f;
    [SerializeField] private float fixedDeltaTimeMultiplier = 0.02f;

    [Header("Raycast Settings")]
    [SerializeField] private LayerMask aimColliderMask = new LayerMask();
    [SerializeField] private float raycastDistance = 999f;
    [SerializeField] private Transform debugTransform;

    [Header("Fire Settings")]
    [SerializeField] private float fireCooldownDuration = 1.0f;

    private float _currentFireCooldown;
    private ThirdPersonController _thirdPersonController;
    private StarterAssetsInputs _starterAssetsInputs;
    private Camera _mainCamera;

    private void Awake()
    {
        _thirdPersonController = GetComponent<ThirdPersonController>();
        _starterAssetsInputs = GetComponent<StarterAssetsInputs>();
        _mainCamera = Camera.main;

        // Ensure StateMaster exists
        var stateMaster = StateMaster.Instance;
        if (stateMaster == null)
        {
            Debug.LogError("[ThirdPersonShooterController] StateMaster not found in scene!");
        }
    }

    private void Update()
    {
        HandlePauseInput();

        if (StateMaster.Instance == null || !StateMaster.Instance.IsPlaying)
        {
            return;
        }

        HandleAiming();
        HandleFiring();
    }

    private void HandlePauseInput()
    {
        if (_starterAssetsInputs.pause && StateMaster.Instance != null)
        {
            _starterAssetsInputs.pause = false;
            StateMaster.Instance.Pause();
        }
    }

    private void HandleAiming()
    {
        bool isAiming = _starterAssetsInputs.aim;

        aimVirtualCamera.gameObject.SetActive(isAiming);
        _thirdPersonController.SetSensitivity(isAiming ? aimSens : normalSens);

        float targetTimeScale = isAiming ? slowMoTimeScale : normalTimeScale;
        Time.timeScale = targetTimeScale;
        Time.fixedDeltaTime = fixedDeltaTimeMultiplier * Time.timeScale;
    }

    private void HandleFiring()
    {
        if (_currentFireCooldown > 0)
        {
            _starterAssetsInputs.fire = false;
            _currentFireCooldown -= Time.unscaledDeltaTime;
            return;
        }

        if (_starterAssetsInputs.fire && StateMaster.Instance.IsPlaying)
        {
            FireRay();
            _currentFireCooldown = fireCooldownDuration;
        }
    }

    private void FireRay()
    {
        if (_mainCamera == null)
        {
            Debug.LogWarning("Main camera not found, cannot fire raycast");
            return;
        }

        Vector2 screenCenterPoint = new Vector2(Screen.width / 2f, Screen.height / 2f);
        Ray ray = _mainCamera.ScreenPointToRay(screenCenterPoint);

        if (Physics.Raycast(ray, out RaycastHit raycastHit, raycastDistance, aimColliderMask))
        {
            if (debugTransform != null)
            {
                debugTransform.position = raycastHit.point;
            }

            if (raycastHit.transform.TryGetComponent<BulletTarget>(out BulletTarget target))
            {
                target.OnRaycastHit();
            }
        }
    }
}
