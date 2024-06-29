using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FleaMovementController : MonoBehaviour
{
    [Header("Main")]
    public PlayerInputActions _inputActions;
    [SerializeField] private Rigidbody2D _rb;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    private LayerMask _environment;


    [Header("Movement Values")]
    [SerializeField] private float _speed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _deceleration;
    [SerializeField] private float _jumpForce;

    [Header("Realtime Values")]
    [SerializeField] private bool _isGrounded;
    [SerializeField] private float _lookDir = 1;
    [SerializeField] private float _jumpCooldown = 0.2f;
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

        if (!_isGrounded && _jumpCooldown <= 0f) //TODO: add groundcheck
        {
            if (Physics2D.Raycast(_rb.position + new Vector2(-0.1f, 0f), Vector2.down, 1.01f, _environment)) { _isGrounded = true; } //Hit ground with left ray!
            else if (Physics2D.Raycast(_rb.position + new Vector2(-0.1f, 0f), Vector2.down, 1.01f, _environment)) { _isGrounded = true; } //Hit ground with right ray!
        }
        Debug.DrawRay(_rb.position + new Vector2(-0.1f, 0f), Vector2.down * 1.01f, Color.red);
        Debug.DrawRay(_rb.position + new Vector2(0.1f, 0f), Vector2.down * 1.01f, Color.red);
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (!_isGrounded || _jumpCooldown > 0) { return; }
        _isGrounded = false;
        _jumpCooldown = 0.2f;
        _rb.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        Debug.Log("shoot");
    }
}
