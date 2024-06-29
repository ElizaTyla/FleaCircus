using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FleaMovementController : MonoBehaviour
{
    enum AnimationState
    {
        IDLE = 0,
        WALK = 1,
        JUMP = 2,
        FALL = 3,
        SHOOT = 4,
        DEAD = 5
    }

    [Header("Main")]
    public PlayerInputActions _inputActions;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Animator _animator;
    private LayerMask _environment;


    [Header("Movement Values")]
    [SerializeField] private float _speed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _deceleration;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _jumpItemMultiplier;

    [Header("Realtime Values")]
    [SerializeField] private int _itemCount;
    [SerializeField] private bool _isGrounded;
    [SerializeField] private float _lookDir = 1;
    [SerializeField] private float _jumpCooldown = 0.2f;
    [SerializeField] private AnimationState _animState = AnimationState.IDLE;
    private InputAction _move;
    private InputAction _jump;
    private InputAction _shoot;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
        _environment = LayerMask.GetMask("Environment");
    }

    private void OnEnable()
    {
        _move = _inputActions.Flea.Horizontal;
        _jump = _inputActions.Flea.Jump;
        _shoot = _inputActions.Flea.Shoot;
        _move.Enable();
        _jump.Enable();
        _shoot.Enable();
        _jump.performed += Jump;
        _shoot.performed += Shoot;
    }

    private void OnDisable()
    {
        _move.Disable();
        _jump.Disable();
        _shoot.Disable();
    }

    private void FixedUpdate()
    {
        _jumpCooldown -= Time.deltaTime;

        float hInput = _inputActions.Flea.Horizontal.ReadValue<float>();
        if (hInput == 0) //decelerating
        {
            _rb.velocity += new Vector2((-_lookDir * _deceleration * Time.deltaTime), 0);
            if (_lookDir == 1 && _rb.velocity.x < 0) { _rb.velocity = new Vector2(0, _rb.velocity.y); } //Snap to 0 speed when stopped
            else if (_lookDir == -1 && _rb.velocity.x > 0) { _rb.velocity = new Vector2(0, _rb.velocity.y); } //Snap to 0 speed when stopped
        }
        else
        {
            _lookDir = hInput;
            if (_lookDir == 1) { _spriteRenderer.flipX = true; } else { _spriteRenderer.flipX = false; }
            _rb.velocity += new Vector2((hInput * _acceleration * Time.deltaTime), 0);
            if (_rb.velocity.x > _speed || _rb.velocity.x < -_speed) { _rb.velocity = new Vector2(_speed * hInput, _rb.velocity.y); } //Cap top speed
        }

        if (!_isGrounded && _jumpCooldown <= 0f)
        {
            if (Physics2D.Raycast(_rb.position + new Vector2(-0.1f, 0f), Vector2.down, 1.51f, _environment)) { _isGrounded = true; } //Hit ground with left ray!
            else if (Physics2D.Raycast(_rb.position + new Vector2(-0.1f, 0f), Vector2.down, 1.51f, _environment)) { _isGrounded = true; } //Hit ground with right ray!
        }
        else if (_isGrounded)
        {
            if (!Physics2D.Raycast(_rb.position + new Vector2(-0.1f, 0f), Vector2.down, 1.51f, _environment) && !Physics2D.Raycast(_rb.position + new Vector2(0.1f, 0f), Vector2.down, 1.51f, _environment)) { _isGrounded = false; }
        }
        //Debug.DrawRay(_rb.position + new Vector2(-0.1f, 0f), Vector2.down * 1.51f, Color.red);
        //Debug.DrawRay(_rb.position + new Vector2(0.1f, 0f), Vector2.down * 1.51f, Color.red);

        HandleAnimations();
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (!_isGrounded || _jumpCooldown > 0) { return; }
        _isGrounded = false;
        _jumpCooldown = 0.2f;
         _rb.AddForce(new Vector2(0, _jumpForce + (_itemCount * _jumpItemMultiplier)), ForceMode2D.Impulse);
        Debug.Log("Jump Force: " + (_jumpForce + (_itemCount * _jumpItemMultiplier)));
        if (_animState == AnimationState.SHOOT || _animState == AnimationState.DEAD) { return; }
        _animator.Play("Jump");
        _animState = AnimationState.JUMP;
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        Debug.Log("shoot");
        _animator.Play("Shoot");
        _animState = AnimationState.SHOOT;
    }

    private void HandleAnimations()
    {
        switch (_animState)
        {
            case AnimationState.IDLE:
                if (_isGrounded && Mathf.Abs(_rb.velocity.x) > 0.05f)
                {
                    _animator.Play("Walk");
                    _animState = AnimationState.WALK;
                } else if (!_isGrounded)
                {
                    _animator.Play("Fall");
                    _animState = AnimationState.FALL;
                }
                return;
            case AnimationState.WALK:
                if (_isGrounded && Mathf.Abs(_rb.velocity.x) < 0.05f)
                {
                    _animator.Play("Idle");
                    _animState = AnimationState.IDLE;
                }
                else if (!_isGrounded)
                {
                    _animator.Play("Fall");
                    _animState = AnimationState.FALL;
                }
                return;
            case AnimationState.JUMP:
                if (_isGrounded && Mathf.Abs(_rb.velocity.x) < 0.05f)
                {
                    _animator.Play("Idle");
                    _animState = AnimationState.IDLE;
                } else if (_isGrounded && Mathf.Abs(_rb.velocity.x) > 0.05f)
                {
                    _animator.Play("Walk");
                    _animState = AnimationState.WALK;
                }
                else if (!_isGrounded && _rb.velocity.y < 0.5f)
                {
                    _animator.Play("Fall");
                    _animState = AnimationState.FALL;
                }
                return;
            case AnimationState.FALL:
                if (_isGrounded && Mathf.Abs(_rb.velocity.x) < 0.05f)
                {
                    _animator.Play("Idle");
                    _animState = AnimationState.IDLE;
                }
                else if (_isGrounded && Mathf.Abs(_rb.velocity.x) > 0.05f)
                {
                    _animator.Play("Walk");
                    _animState = AnimationState.WALK;
                }
                return;
            case AnimationState.SHOOT:
                if (_animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f) { return; }
                if (_isGrounded)
                {
                    if (Mathf.Abs(_rb.velocity.x) < 0.05f)
                    {
                        _animator.Play("Idle");
                        _animState = AnimationState.IDLE;
                    }
                    else if (Mathf.Abs(_rb.velocity.x) > 0.05f)
                    {
                        _animator.Play("Walk");
                        _animState = AnimationState.WALK;
                    }
                }
                else
                {
                    if (_rb.velocity.y < 0.5f)
                    {
                        _animator.Play("Fall");
                        _animState = AnimationState.FALL;
                    }
                    else
                    {
                        _animator.Play("Jump", 0, 0.4f);
                        _animState = AnimationState.JUMP;
                    }
                }
                return;
            default: return;
        }
    }
}
