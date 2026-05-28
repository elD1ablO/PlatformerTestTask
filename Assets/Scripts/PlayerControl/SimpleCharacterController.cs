using PlayerControl;
using Synty.AnimationBaseLocomotion.Samples.InputSystem;
using UnityEngine;

public class SimpleCharacterController : MonoBehaviour, IPlayerObject
{
    private readonly int _moveSpeedHash = Animator.StringToHash("MoveSpeed");
    private readonly int _isJumpingAnimHash = Animator.StringToHash("IsJumping");
    private readonly int _isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int _currentGaitHash = Animator.StringToHash("CurrentGait");
    private readonly int _strafeDirectionXHash = Animator.StringToHash("StrafeDirectionX");
    private readonly int _strafeDirectionZHash = Animator.StringToHash("StrafeDirectionZ");
    private readonly int _isStrafingHash = Animator.StringToHash("IsStrafing");
    private readonly int _isWalkingHash = Animator.StringToHash("IsWalking");
    private readonly int _isStoppedHash = Animator.StringToHash("IsStopped");
    private readonly int _movementInputHeldHash = Animator.StringToHash("MovementInputHeld");

    [Header("Components")]
    [SerializeField]
    private InputReader _inputReader;

    [SerializeField]
    private Animator _animator;

    [SerializeField]
    private CharacterController _controller;

    [SerializeField]
    private Transform _modelTransform;

    [SerializeField]
    private SphereCollider _blockHitCollider;

    [Header("Movement Settings")]
    [SerializeField]
    private float _moveSpeed = 5f;
    [SerializeField]
    private float _sprintMoveSpeed = 7f;
    [SerializeField]
    private float _acceleration = 25f;
    [SerializeField]
    private float _deceleration = 35f;
    [SerializeField]
    private float _airControlMultiplier = 0.6f;
    [SerializeField]
    private float _rotationSpeed = 10f;
    [Header("Jump Settings")]
    [SerializeField]
    private float _jumpForce = 10f;
    [SerializeField]
    private int _maxJumpCount = 2;
    [SerializeField]
    private float _coyoteTime = 0.12f;
    [SerializeField]
    private float _jumpBufferTime = 0.12f;
    [SerializeField]
    private float _gravityStrength = 2.5f;
    [SerializeField]
    private float _fallGravityMultiplier = 3f;
    [SerializeField]
    private float _lowJumpGravityMultiplier = 4f;
    [SerializeField]
    private float _blockHitFallBoost = 18f;
    [SerializeField]
    private float _wallNormalThreshold = 0.6f;

    [Header("Ground Check")]
    [SerializeField]
    private LayerMask _groundLayerMask;
    [SerializeField]
    private float _groundedOffset = 0.08f;
    [SerializeField]
    private float _groundCheckRadius = 0.22f;

    [Header("Walk Audio")]
    [SerializeField]
    private float _walkSoundInterval = 0.35f;
    [SerializeField]
    private float _sprintSoundInterval = 0.25f;
    [SerializeField]
    private float _minimumWalkSpeed = 1.2f;

    [Header("Respawn data")]
    [SerializeField]
    private Vector3 _initialPosition = new Vector3(0f, 0f, 0f);

    private Vector3 _velocity;
    private bool _isGrounded = true;
    private float _speed2D;
    private Vector3 _moveDirection;
    private int _currentGait;
    private float _strafeDirectionX = 0f;
    private float _strafeDirectionZ = 1f;
    private bool _isWalking = false;
    private bool _isStopped = true;

    private bool _movementInputHeld = false;
    private int _jumpCount;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    private bool _isJumpHeld;
    private bool _isSprinting;
    private float _walkSoundTimer;

    private void OnEnable()
    {
        _inputReader.onJumpPerformed += OnJump;
        _inputReader.onJumpCanceled += OnJumpCanceled;

        _inputReader.onSprintActivated += OnSprintActivated;
        _inputReader.onSprintDeactivated += OnSprintDeactivated;
    }

    private void Update()
    {
        GroundedCheck();
        UpdateJumpState();
        HandleJump();
        CalculateMoveDirection();
        CheckIfStopped();
        FaceMoveDirection();
        ApplyGravity();
        Move();
        HandleWalkAudio();
        UpdateAnimator();
    }

    private void CalculateMoveDirection()
    {
        _moveDirection = new Vector3(_inputReader._moveComposite.x, 0f, 0f);

        _movementInputHeld = _moveDirection.magnitude > 0.01f;


        float currentMaxSpeed = _isSprinting
            ? _sprintMoveSpeed
            : _moveSpeed;

        float targetSpeedX = _moveDirection.x * currentMaxSpeed;

        float speedChangeRate = _movementInputHeld
            ? _acceleration
            : _deceleration;

        if (!_isGrounded)
        {
            speedChangeRate *= _airControlMultiplier;
        }

        _velocity.x = Mathf.MoveTowards(
            _velocity.x,
            targetSpeedX,
            speedChangeRate * Time.deltaTime
        );

        _velocity.z = 0f;

        _speed2D = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
        _speed2D = Mathf.Round(_speed2D * 1000f) / 1000f;

        CalculateGait();
    }

    private void CalculateGait()
    {
        if (_speed2D < 0.01f)
        {
            _currentGait = 0;
        }
        else if (_speed2D < _moveSpeed * 0.5f)
        {
            _currentGait = 1;
        }
        else
        {
            _currentGait = 2;
        }
    }

    private void CheckIfStopped()
    {
        _isStopped = _moveDirection.magnitude == 0f && _speed2D < 0.5f;
        _isWalking = !_isStopped && _isGrounded;
    }

    private void FaceMoveDirection()
    {
        if (_modelTransform == null)
        {
            return;
        }

        if (_moveDirection.magnitude > 0.01f)
        {
            Vector3 faceDirection = new Vector3(_velocity.x, 0f, 0f);

            if (faceDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(faceDirection);

                _modelTransform.rotation = Quaternion.Slerp(
                    _modelTransform.rotation,
                    targetRotation,
                    _rotationSpeed * Time.deltaTime
                );
            }
        }
    }

    private void HandleWalkAudio()
    {
        if (!_isGrounded || _speed2D < _minimumWalkSpeed)
        {
            _walkSoundTimer = 0f;
            return;
        }

        bool isSprinting = _speed2D > (_moveSpeed + 0.5f);

        float currentInterval = isSprinting
            ? _sprintSoundInterval
            : _walkSoundInterval;

        _walkSoundTimer -= Time.deltaTime;

        if (_walkSoundTimer > 0f)
        {
            return;
        }

        _walkSoundTimer = currentInterval;

        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWalk();
        }
    }

    private void OnJump()
    {
        _jumpBufferCounter = _jumpBufferTime;
        _isJumpHeld = true;
    }

    private void OnJumpCanceled()
    {
        _isJumpHeld = false;
    }

    private void UpdateJumpState()
    {
        if (_isGrounded)
        {
            _coyoteTimeCounter = _coyoteTime;
            _jumpCount = 0;

            if (_velocity.y < 0f)
            {
                _velocity.y = -2f;
            }
        }
        else
        {
            _coyoteTimeCounter -= Time.deltaTime;
        }

        if (_jumpBufferCounter > 0f)
        {
            _jumpBufferCounter -= Time.deltaTime;
        }
    }

    private void HandleJump()
    {
        if (_jumpBufferCounter <= 0f)
        {
            return;
        }

        bool canUseGroundJump = _isGrounded || _coyoteTimeCounter > 0f;
        bool canUseAirJump = !_isGrounded && _jumpCount < _maxJumpCount;

        if (!canUseGroundJump && !canUseAirJump)
        {
            return;
        }

        _velocity.y = _jumpForce;

        AudioManager.Instance.PlayJump();        

        _jumpBufferCounter = 0f;
        _coyoteTimeCounter = 0f;
        _jumpCount++;
    }

    private void ApplyGravity()
    {
        float gravityMultiplier = 1f;

        if (_velocity.y < 0f)
        {
            gravityMultiplier = _fallGravityMultiplier;
        }
        else if (_velocity.y > 0f && !_isJumpHeld)
        {
            gravityMultiplier = _lowJumpGravityMultiplier;
        }

        _velocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
    }

    private void Move()
    {
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(
            transform.position.x,
            transform.position.y + _controller.center.y - (_controller.height * 0.5f) + _groundedOffset,
            transform.position.z
        );

        bool groundDetected = Physics.CheckSphere(
            spherePosition,
            _groundCheckRadius,
            _groundLayerMask,
            QueryTriggerInteraction.Ignore
        );

        _isGrounded = groundDetected && _velocity.y <= 0.1f;
    }

    private void UpdateAnimator()
    {
        _animator.SetFloat(_moveSpeedHash, _speed2D);
        _animator.SetInteger(_currentGaitHash, _currentGait);
        _animator.SetBool(_isGroundedHash, _isGrounded);
        _animator.SetBool(_isJumpingAnimHash, !_isGrounded);
        _animator.SetFloat(_strafeDirectionXHash, _strafeDirectionX);
        _animator.SetFloat(_strafeDirectionZHash, _strafeDirectionZ);
        _animator.SetFloat(_isStrafingHash, 0f);
        _animator.SetBool(_isWalkingHash, _isWalking);
        _animator.SetBool(_isStoppedHash, _isStopped);
        _animator.SetBool(_movementInputHeldHash, _movementInputHeld);
    }
    private void OnSprintActivated()
    {
        _isSprinting = true;
    }

    private void OnSprintDeactivated()
    {
        _isSprinting = false;
    }

    private void OnDisable()
    {
        _inputReader.onJumpPerformed -= OnJump;
        _inputReader.onJumpCanceled -= OnJumpCanceled;

        _inputReader.onSprintActivated -= OnSprintActivated;
        _inputReader.onSprintDeactivated -= OnSprintDeactivated;
    }

    public void KillZoneEntered()
    {
        _controller.enabled = false;

        transform.position = _initialPosition;

        _controller.enabled = true;

        _velocity = Vector3.zero;
        _moveDirection = Vector3.zero;
        _speed2D = 0f;
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (Mathf.Abs(hit.normal.x) > _wallNormalThreshold)
        {
            bool movingIntoWall =
                Mathf.Sign(_velocity.x) == -Mathf.Sign(hit.normal.x);

            if (movingIntoWall)
            {
                _velocity.x = 0f;
            }

            return;
        }

        if (_velocity.y <= 0f)
        {
            return;
        }

        Vector3 hitDirection = hit.point - transform.position;

        bool hitFromBelow = hitDirection.y > 0.15f;

        if (!hitFromBelow)
        {
            return;
        }

        BonusCube bonusCube = hit.collider.GetComponentInParent<BonusCube>();

        if (bonusCube != null)
        {
            bonusCube.TryHitFromCharacterController(_blockHitCollider);
            AudioManager.Instance.PlayBonusCubeHit();
        }
        else
        {            
            AudioManager.Instance.PlayHeadHit();            
        }

        ForceFallAfterBlockHit();
    }
    private void ForceFallAfterBlockHit()
    {
        _isJumpHeld = false;
        _jumpBufferCounter = 0f;
        _coyoteTimeCounter = 0f;

        _velocity.y = -Mathf.Abs(_blockHitFallBoost);
    }
}