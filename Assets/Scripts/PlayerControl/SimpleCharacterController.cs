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

    [Header("Movement Settings")]
    [SerializeField] 
    private float _moveSpeed = 5f;
    [SerializeField] 
    private float _jumpForce = 10f;
    [SerializeField] 
    private float _gravityMultiplier = 2f;
    [SerializeField] 
    private float _rotationSpeed = 10f;

    [Header("Ground Check")]
    [SerializeField] 
    private LayerMask _groundLayerMask;
    [SerializeField] 
    private float _groundedOffset = -0.14f;
    
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
    

    private void Start()
    {
        _inputReader.onJumpPerformed += OnJump;
    }

    private void Update()
    {
        GroundedCheck();
        CalculateMoveDirection();
        CheckIfStopped();
        FaceMoveDirection();
        ApplyGravity();
        Move();
        UpdateAnimator();
    }

    private void CalculateMoveDirection()
    {
        _moveDirection = new Vector3(_inputReader._moveComposite.x, 0f, 0f);
        _movementInputHeld = _moveDirection.magnitude > 0.01f;

        _velocity.x = _moveDirection.x * _moveSpeed;
        _velocity.z = 0f;

        _speed2D = new Vector3(_velocity.x, 0f, _velocity.z).magnitude;
        _speed2D = Mathf.Round(_speed2D * 1000f) / 1000f;

        CalculateGait();
    }

    private void CalculateGait()
    {
        if (_speed2D < 0.01f)
        {
            _currentGait = 0; // Idle
        }
        else if (_speed2D < _moveSpeed * 0.5f)
        {
            _currentGait = 1; // Walk
        }
        else
        {
            _currentGait = 2; // Run
        }
    }

    private void CheckIfStopped()
    {
        _isStopped = _moveDirection.magnitude == 0 && _speed2D < 0.5f;
        _isWalking = !_isStopped && _isGrounded;
    }

    private void FaceMoveDirection()
    {
        if (_modelTransform == null)
            return;

        if (_moveDirection.magnitude > 0.01f)
        {
            Vector3 faceDirection = new Vector3(_velocity.x, 0f, 0f);
            if (faceDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(faceDirection);
                _modelTransform.rotation = Quaternion.Slerp(_modelTransform.rotation, targetRotation, _rotationSpeed * Time.deltaTime);
            }
        }
    }

    private void OnJump()
    {
        if (_isGrounded)
        {
            _velocity.y = _jumpForce;
            _animator.SetBool(_isJumpingAnimHash, true);
        }
    }

    private void ApplyGravity()
    {
        if (_velocity.y > Physics.gravity.y)
        {
            _velocity.y += Physics.gravity.y * _gravityMultiplier * Time.deltaTime;
        }

        if (_velocity.y <= 0f)
        {
            _animator.SetBool(_isJumpingAnimHash, false);
        }
    }

    private void Move()
    {
        _controller.Move(_velocity * Time.deltaTime);
    }

    private void GroundedCheck()
    {
        Vector3 spherePosition = new Vector3(
            _controller.transform.position.x,
            _controller.transform.position.y - _groundedOffset,
            _controller.transform.position.z
        );
        _isGrounded = Physics.CheckSphere(spherePosition, _controller.radius, _groundLayerMask, QueryTriggerInteraction.Ignore);
    }

    private void UpdateAnimator()
    {
        _animator.SetFloat(_moveSpeedHash, _speed2D);
        _animator.SetInteger(_currentGaitHash, _currentGait);
        _animator.SetBool(_isGroundedHash, _isGrounded);
        _animator.SetFloat(_strafeDirectionXHash, _strafeDirectionX);
        _animator.SetFloat(_strafeDirectionZHash, _strafeDirectionZ);
        _animator.SetFloat(_isStrafingHash, 0f);
        _animator.SetBool(_isWalkingHash, _isWalking);
        _animator.SetBool(_isStoppedHash, _isStopped);
        _animator.SetBool(_movementInputHeldHash, _movementInputHeld);
    }

    private void OnDestroy()
    {
        _inputReader.onJumpPerformed -= OnJump;
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
}
