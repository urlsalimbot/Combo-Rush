using UnityEngine;
using StarterAssets;

/// <summary>
/// Extension component that adds dash and wall bounce mechanics to the ThirdPersonController.
/// Attach this to the same GameObject as ThirdPersonController.
/// </summary>
[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(ThirdPersonController))]
public class PlayerMovementExtension : MonoBehaviour
{
    [Header("Dash Settings")]
    [Tooltip("Duration of the dash in seconds")]
    [SerializeField] private float dashDuration = 0.2f;

    [Tooltip("Cooldown time between dashes in seconds")]
    [SerializeField] private float dashCooldown = 1.0f;

    [Tooltip("Speed multiplier during dash")]
    [SerializeField] private float dashSpeedMultiplier = 3.0f;

    [Tooltip("Should dash work in air?")]
    [SerializeField] private bool allowAirDash = true;

    [Header("Wall Bounce Settings")]
    [Tooltip("Enable wall bounce mechanic")]
    [SerializeField] private bool enableWallBounce = true;

    [Tooltip("Distance to check for walls")]
    [SerializeField] private float wallCheckDistance = 0.5f;

    [Tooltip("Layers considered as walls")]
    [SerializeField] private LayerMask wallLayers = -1;

    [Tooltip("Force applied when bouncing off wall")]
    [SerializeField] private float wallBounceForce = 12f;

    [Tooltip("Upward force added when wall bouncing")]
    [SerializeField] private float wallBounceUpwardForce = 18f;

    [Tooltip("Height to check for wall (relative to player position)")]
    [SerializeField] private float wallCheckHeight = 1f;

    [Header("References")]
    [SerializeField] private Transform debugTransform;

    // Component references
    private CharacterController _controller;
    private StarterAssetsInputs _input;
    private Camera _mainCamera;

    // Dash state
    private bool _isDashing;
    private float _dashTimeRemaining;
    private float _dashCooldownRemaining;
    private Vector3 _dashDirection;
    private Vector3 _dashVelocity;
    private float _dashInputCooldown;

    // Wall detection state
    private bool _isTouchingWall;
    private Vector3 _wallNormal;

    // Wall bounce state
    private bool _isWallBouncing;
    private Vector3 _wallBounceVelocity;
    private float _wallBounceTime;

    private bool _hasWallBouncedThisFrame;

    private void Awake()
    {
        _controller = GetComponent<CharacterController>();
        _input = GetComponent<StarterAssetsInputs>();
        _mainCamera = Camera.main;
    }

    private void Update()
    {
        if (StateMaster.Instance == null || !StateMaster.Instance.IsPlaying) return;

        UpdateCooldowns();

        if (enableWallBounce)
        {
            CheckWall();
        }

        // Dash with input cooldown to prevent misfires
        if (_dashInputCooldown <= 0 && _input.dash && CanDash())
        {
            StartDash();
            _input.dash = false;
            _dashInputCooldown = 0.2f; // Prevent double-trigger
        }

        // Wall bounce: jump while touching a wall (in air)
        if (enableWallBounce && _isTouchingWall && _input.jump && !_controller.isGrounded)
        {
            PerformWallBounce();
            _input.jump = false;
        }

        if (_isDashing)
        {
            HandleDash();
        }

        if (_isWallBouncing)
        {
            HandleWallBounce();
        }

        if (enableWallBounce && _hasWallBouncedThisFrame)
        {
            _hasWallBouncedThisFrame = false;
        }
    }

    private void UpdateCooldowns()
    {
        if (_dashCooldownRemaining > 0)
        {
            _dashCooldownRemaining -= Time.deltaTime;
        }

        if (_dashInputCooldown > 0)
        {
            _dashInputCooldown -= Time.deltaTime;
        }

        if (_isDashing)
        {
            _dashTimeRemaining -= Time.deltaTime;
            if (_dashTimeRemaining <= 0)
            {
                EndDash();
            }
        }

        if (_isWallBouncing)
        {
            _wallBounceTime -= Time.deltaTime;
            if (_wallBounceTime <= 0)
            {
                _isWallBouncing = false;
            }
        }
    }

    private void HandleWallBounce()
    {
        // Apply wall bounce velocity
        _controller.Move(_wallBounceVelocity * Time.deltaTime);
    }

    private bool CanDash()
    {
        if (_isDashing) return false;
        if (_dashCooldownRemaining > 0) return false;
        if (!allowAirDash && !_controller.isGrounded) return false;

        return true;
    }

    private void StartDash()
    {
        _isDashing = true;
        _dashTimeRemaining = dashDuration;
        _dashCooldownRemaining = dashCooldown;

        // Get dash direction from input (camera-relative)
        Vector3 inputDirection = new Vector3(_input.move.x, 0f, _input.move.y).normalized;

        if (inputDirection != Vector3.zero)
        {
            // Make direction camera-relative
            _dashDirection = Quaternion.Euler(0f, _mainCamera.transform.eulerAngles.y, 0f) * inputDirection;
        }
        else
        {
            _dashDirection = transform.forward;
        }

        // Calculate dash velocity (preserve current vertical velocity for gravity)
        float dashSpeed = 15f * dashSpeedMultiplier;
        _dashVelocity = _dashDirection * dashSpeed;
        _dashVelocity.y = _controller.velocity.y;

        Debug.Log($"[PlayerMovementExtension] Dash started! Direction: {_dashDirection}, Velocity: {_dashVelocity}");
    }

    private void HandleDash()
    {
        // Apply dash velocity directly to controller
        _controller.Move(_dashVelocity * Time.deltaTime);
    }

    private void EndDash()
    {
        _isDashing = false;
        _dashTimeRemaining = 0f;
        Debug.Log("[PlayerMovementExtension] Dash ended");
    }

    private void CheckWall()
    {
        _isTouchingWall = false;
        _wallNormal = Vector3.zero;

        // Check in all horizontal directions around the player at wall check height
        Vector3[] checkDirections = new Vector3[]
        {
            transform.forward,
            -transform.forward,
            transform.right,
            -transform.right,
            (transform.forward + transform.right).normalized,
            (transform.forward - transform.right).normalized,
            (-transform.forward + transform.right).normalized,
            (-transform.forward - transform.right).normalized
        };

        Vector3 sphereCastOrigin = transform.position + Vector3.up * wallCheckHeight;

        foreach (Vector3 dir in checkDirections)
        {
            if (Physics.SphereCast(sphereCastOrigin, 0.3f, dir, out RaycastHit hit, wallCheckDistance + 0.1f, wallLayers))
            {
                _isTouchingWall = true;
                _wallNormal = hit.normal;

                if (debugTransform != null)
                {
                    debugTransform.position = hit.point;
                }

                break;
            }
        }
    }

    private void PerformWallBounce()
    {
        if (_hasWallBouncedThisFrame || _isWallBouncing) return;

        // Launch in the wall normal direction with slight upward trajectory
        _wallBounceVelocity = _wallNormal * wallBounceForce;
        _wallBounceVelocity.y = wallBounceUpwardForce;
        _wallBounceTime = 0.5f; // Apply bounce over short duration
        _isWallBouncing = true;

        _hasWallBouncedThisFrame = true;

        Debug.Log($"[PlayerMovementExtension] Wall bounce! Normal: {_wallNormal}, Velocity: {_wallBounceVelocity}");
    }

    /// <summary>
    /// Returns the velocity to use during dash. Call this from ThirdPersonController.Move
    /// </summary>
    public Vector3 GetDashVelocity()
    {
        if (_isDashing)
        {
            return _dashVelocity;
        }
        return Vector3.zero;
    }

    /// <summary>
    /// Returns true if player is currently dashing
    /// </summary>
    public bool IsDashing => _isDashing;

    /// <summary>
    /// Returns true if player is currently wall bouncing
    /// </summary>
    public bool IsWallBouncing => _isWallBouncing;

    /// <summary>
    /// Returns true if dash is available (not on cooldown)
    /// </summary>
    public bool CanDashNow => CanDash();

    /// <summary>
    /// Returns the remaining cooldown time for dash
    /// </summary>
    public float DashCooldownRemaining => _dashCooldownRemaining;

    /// <summary>
    /// Returns true if player is touching a wall
    /// </summary>
    public bool IsTouchingWall => _isTouchingWall;

    /// <summary>
    /// Returns the normal of the wall being touched
    /// </summary>
    public Vector3 WallNormal => _wallNormal;

    private void OnDrawGizmosSelected()
    {
        // Draw wall check rays
        if (enableWallBounce)
        {
            Gizmos.color = _isTouchingWall ? Color.red : Color.yellow;

            Vector3[] checkDirections = new Vector3[]
            {
                transform.forward,
                -transform.forward,
                transform.right,
                -transform.right
            };

            foreach (Vector3 dir in checkDirections)
            {
                Gizmos.DrawRay(transform.position, dir * wallCheckDistance);
            }
        }

        // Draw dash direction
        if (_isDashing)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(transform.position, _dashDirection * 2f);
        }
    }
}
