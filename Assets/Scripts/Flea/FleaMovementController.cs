using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class FleaMovementController : MonoBehaviour
{
    [Header("Main")]
    public PlayerInputActions _inputActions;
    [SerializeField] private Rigidbody2D _rb;
    private InputAction _move;
    private InputAction _jump;
    private InputAction _shoot;

    [Header("Movement Values")]
    [SerializeField] private float _speed;
    [SerializeField] private float _acceleration;
    [SerializeField] private float _deceleration;
    [SerializeField] private float _jumpForce;

    [Header("Realtime Values")]
    private bool _isJumping = false;
    private float _lookDir = 1;

    private void Awake()
    {
        _inputActions = new PlayerInputActions();
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
            _rb.velocity += new Vector2((hInput * _acceleration * Time.deltaTime), 0);
            if (_rb.velocity.x > _speed || _rb.velocity.x < -_speed) { _rb.velocity = new Vector2(_speed * hInput, _rb.velocity.y); } //Cap top speed
        }

        if (_isJumping) //TODO: add groundcheck
        {
            _isJumping = false;
        }
    }

    private void Jump(InputAction.CallbackContext context)
    {
        if (_isJumping) { return; }
        _isJumping = true; ;
        _rb.AddForce(new Vector2(0, _jumpForce), ForceMode2D.Impulse);
    }

    private void Shoot(InputAction.CallbackContext context)
    {
        Debug.Log("shoot");
    }
}
