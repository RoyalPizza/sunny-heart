using Pizza.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pizza
{
    public class PlayerController : PizzaMonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private Rigidbody2D _rigidbody2D;

        [Header("Gravity Config")]
        [SerializeField]
        private float _gravity = 60f;

        [Header("Ground Check")]
        [SerializeField, Tooltip("Set this to any layer that objects reside on to be considered grounded.")]
        private LayerMask groundLayer;
        [SerializeField, Min(0f)]
        private float groundCheckRadius = 0.2f;
        [SerializeField]
        private Transform footTransform;

        [Header("Walk Config")]
        [SerializeField, Min(0f)]
        private float _walkSpeed = 7f;
        [SerializeField, Min(0f)]
        private float _walkAccelerationSpeed = 60f;
        [SerializeField, Min(0f)]
        private float _walkDecelerationSpeed = 50f;

        [Header("Air Walk Config")]
        [SerializeField]
        private float _airMoveSpeed = 0.2f;
        [SerializeField]
        private float _maxAirMoveVelocity = 5f;

        [Header("Jump Config")]
        [SerializeField]
        private float _jumpForce = 20f;
        [SerializeField, Tooltip("The time in seconds for the jump to hang at its apex.")]
        private float _jumpHangTime = 1.0f;

        [Header("Fall Config")]
        [SerializeField]
        private float _maxFallVelocity = 10f;

        [Header("Dash Config")]
        [SerializeField]
        private float _dashForce = 15f;
        [SerializeField, Tooltip("The time in seconds the dash lasts")]
        private float _dashDuration = 0.2f;
        [SerializeField, Tooltip("The time in seconds before the player can dash again.")]
        private float _dashCooldown = 3f;

        //
        InputAction _moveAction;
        InputAction _jumpAction;
        private Vector2 _movementInput;
        private bool _jumpInput;

        private float _defaultGravityScale;

        // states
        private bool _jumpRequested;
        private bool _jumpAllowed;
        private bool _jumpTriggered; // states if we are airborne because we jumped or not
        private bool _jumpHang; // when true we are in hang time after jumping
        private float _jumpHangTimeLeft; // the time we have left to hang

        private bool _isGrounded;
        private bool _isClimb;
        private int _jumpsLeft;
        private float lastJumpTime;

        private void Awake()
        {
            _moveAction = InputSystem.actions.FindAction("Move");
            _jumpAction = InputSystem.actions.FindAction("Jump");
            _defaultGravityScale = _rigidbody2D.gravityScale;
        }

        private void Update()
        {
            // cache raw input
            _movementInput = _moveAction.ReadValue<Vector2>();
            _jumpInput = _jumpAction.IsPressed();
            if (!_jumpRequested && _jumpAction.WasPressedThisFrame() && _jumpAllowed)
                _jumpRequested = true;

            // always allow rotation
            if (_movementInput.x > 0)
                transform.rotation = Quaternion.Euler(0, 0, 0);
            else if (_movementInput.x < 0)
                transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        private void FixedUpdate()
        {
            // we use a overlap instead of a raycast so the check is as wide as the player (almost)
            _isGrounded = Physics2D.OverlapCircle(footTransform.position, groundCheckRadius, groundLayer);

            if (_isGrounded)
            {
                if (_jumpRequested)
                {
                    Vector2 newVelocity = Vector2.zero;
                    newVelocity.x = _rigidbody2D.linearVelocityX;
                    newVelocity.y = _rigidbody2D.linearVelocityY + _jumpForce;
                    _rigidbody2D.linearVelocity = newVelocity;

                    _jumpTriggered = true;
                    _jumpRequested = false;
                    _jumpHang = false;
                    _rigidbody2D.gravityScale = _defaultGravityScale;
                }
                else if (_movementInput != Vector2.zero)
                {
                    Vector2 newVelocity = Vector2.zero;
                    // basically lerp from our current velocity to the target velocity, at the rate of acceleration
                    newVelocity.x = Mathf.MoveTowards(_rigidbody2D.linearVelocityX, _movementInput.x * _walkSpeed, _walkAccelerationSpeed * Time.fixedDeltaTime);
                    newVelocity.y = _rigidbody2D.linearVelocityY;
                    _rigidbody2D.linearVelocity = newVelocity;
                }
                else if (_movementInput.x == 0f)
                {
                    Vector2 newVelocity = Vector2.zero;
                    // basically lerp from our current velocity to 0, at the rate of Deceleration
                    newVelocity.x = Mathf.MoveTowards(_rigidbody2D.linearVelocityX, 0f, _walkDecelerationSpeed * Time.fixedDeltaTime);
                    newVelocity.y = _rigidbody2D.linearVelocityY;
                    _rigidbody2D.linearVelocity = new Vector2(0f, _rigidbody2D.linearVelocity.y);
                    _rigidbody2D.linearVelocity = newVelocity;
                }
            }
            else
            {

                if (_jumpTriggered && Mathf.Abs(_rigidbody2D.linearVelocityY) < 0.1f && !_jumpHang)
                {
                    // we previously jumped, and now are at our "apex"
                    _jumpHang = true;
                    _rigidbody2D.gravityScale = 0f;
                }
                else if (_jumpHang && _jumpHangTimeLeft > 0f)
                {
                    // decrement our hang timer
                    _jumpHangTimeLeft -= Time.fixedDeltaTime;
                }
                else if (_jumpHang && _jumpHangTimeLeft <= 0f)
                {
                    // our hang is done, turn back on gravity and end the "jump"
                    _jumpTriggered = false;
                    _jumpHang = false;
                    _rigidbody2D.gravityScale = _defaultGravityScale;
                }
            }
        }

        private void OnGUI()
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 48
            };

            GUILayout.BeginArea(new Rect(10, 10, 500, 100));
            //GUILayout.Label($"Velocity: {_rigidbody2D.linearVelocity}", labelStyle);
            //GUILayout.Label($"Grounded: {_isGrounded}", labelStyle);
            GUILayout.Label($"JumpRequested: {_jumpRequested}", labelStyle);
            GUILayout.EndArea();
        }

        private void OnDrawGizmos()
        {
            if (footTransform != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(footTransform.position, groundCheckRadius);
            }
        }
    }
}