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
    [SerializeField] private GameObject _firePoint;
    [SerializeField] private GameObject _projectile;
    private LayerMask _environment;


    [Header("Movement Values")]
    [SerializeField] private float _speed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _deceleration;
    [SerializeField] private float _jumpForce;
    [SerializeField] private float _jumpItemMultiplier;
    [SerializeField] private float _bouncePadMultiplier;

    [Header("Realtime Values")]
    [SerializeField] private int _itemCount = 0;
    [SerializeField] private bool _isGrounded;
    [SerializeField] private float _lookDir = 1;
    [SerializeField] private float _jumpCooldown = 0.2f;
    [SerializeField] private float _bounceCooldown = 0.2f;
    [SerializeField] private float _shootCooldown = 0.2f;
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
        if (_animState == AnimationState.DEAD) { return; }
        _jumpCooldown -= Time.deltaTime;
        _bounceCooldown -= Time.deltaTime;
        _shootCooldown -= Time.deltaTime;

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
            if (_lookDir == 1)
            {
                _spriteRenderer.flipX = true;
                _firePoint.transform.localPosition = new Vector2(0.8f, -0.2f);                
            }
            else
            {
                _spriteRenderer.flipX = false;
                _firePoint.transform.localPosition = new Vector2(-0.8f, -0.2f);
            }
            _rb.velocity += new Vector2((hInput * _acceleration * Time.deltaTime), 0);
            if (_rb.velocity.x > _speed || _rb.velocity.x < -_speed) { _rb.velocity = new Vector2(_speed * hInput, _rb.velocity.y); } //Cap top speed
        }

        if (!_isGrounded && _jumpCooldown <= 0f)
        {
            if (Physics2D.Raycast(_rb.position + new Vector2(-0.1f, 0f), Vector2.down, 2.01f, _environment)) { _isGrounded = true; } //Hit ground with left ray!
            else if (Physics2D.Raycast(_rb.position + new Vector2(-0.1f, 0f), Vector2.down, 2.01f, _environment)) { _isGrounded = true; } //Hit ground with right ray!
        }
        else if (_isGrounded)
        {
            if (!Physics2D.Raycast(_rb.position + new Vector2(-0.1f, 0f), Vector2.down, 2.01f, _environment) && !Physics2D.Raycast(_rb.position + new Vector2(0.1f, 0f), Vector2.down, 2.01f, _environment)) { _isGrounded = false; }

            Collider2D lCol = Physics2D.Raycast(_rb.position + new Vector2(-0.1f, 0f), Vector2.down, 2.01f, _environment).collider;
            Collider2D rCol = Physics2D.Raycast(_rb.position + new Vector2(0.1f, 0f), Vector2.down, 2.01f, _environment).collider;
            if (lCol != null)
            {
                if (lCol.gameObject.CompareTag("Bounce"))
                {
                    Bounce();
                }
                else
                {
                    if (rCol != null)
                    {
                        if (rCol.gameObject.CompareTag("Bounce"))
                        {
                            Bounce();
                        }
                    }
                }
            }

            //Debug.DrawRay(_rb.position + new Vector2(-0.1f, 0f), Vector2.down * 2.01f, Color.red);
            //Debug.DrawRay(_rb.position + new Vector2(0.1f, 0f), Vector2.down * 2.01f, Color.red);

            HandleAnimations();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Blood"))
        {
            _itemCount += 1;
            GameObject.Destroy(collision.gameObject);
        }
    }

    public void Death()
    {
        _animator.Play("Death");
        _animState = AnimationState.DEAD;
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (!_isGrounded || _jumpCooldown > 0 || _animState == AnimationState.DEAD) { return; }
        _isGrounded = false;
        _jumpCooldown = 0.2f;
        _bounceCooldown = 0.2f;
         _rb.AddForce(new Vector2(0, _jumpForce + (_itemCount * _jumpItemMultiplier)), ForceMode2D.Impulse);
        Debug.Log("Jump Force: " + (_jumpForce + (_itemCount * _jumpItemMultiplier)));
        if (_animState == AnimationState.SHOOT || _animState == AnimationState.DEAD) { return; }
        _animator.Play("Jump");
        _animState = AnimationState.JUMP;
    }

    private void Bounce()
    {
        if (_bounceCooldown > 0f || _animState == AnimationState.DEAD) { return; }
        _jumpCooldown = 0.2f;
        _bounceCooldown = 0.2f;
        _rb.AddForce(new Vector2(0, _bouncePadMultiplier * _jumpForce + (_itemCount * _jumpItemMultiplier)), ForceMode2D.Impulse);
        Debug.Log("Bounce Jump Force: " + (_bouncePadMultiplier * _jumpForce + (_itemCount * _jumpItemMultiplier)));
        if (_animState == AnimationState.SHOOT || _animState == AnimationState.DEAD) { return; }
        _animator.Play("Jump");
        _animState = AnimationState.JUMP;
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        if (_shootCooldown > 0f || _animState == AnimationState.DEAD) { return; }
        _shootCooldown = 0.2f;
        Debug.Log("shoot");
        _animator.Play("Shoot");
        _animState = AnimationState.SHOOT;
    }

    private void SpawnProjectile() //Called during shoot animation
    {
        Vector2 fireDir = Vector2.right;
        if (_lookDir == -1) { fireDir = Vector2.left; }
        GameObject projectile = GameObject.Instantiate(_projectile, _firePoint.transform.position, _firePoint.transform.rotation, _firePoint.transform);
        if (fireDir == Vector2.right) { projectile.transform.Rotate(0, 0, 180); }
        //TODO: projectile spawning and logic
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
