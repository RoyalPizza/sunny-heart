using Pizza.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pizza
{
    public class PlayerControllerOld : PizzaMonoBehaviour
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
        [SerializeField]
        private float groundCheckRadius = 0.2f;
        [SerializeField]
        private Transform footTransform;

        [Header("Walk Config")]
        [SerializeField]
        private float _moveSpeed = 7f;

        [Header("Air Walk Config")]
        [SerializeField]
        private float _airMoveSpeed = 0.2f;
        [SerializeField]
        private float _maxAirMoveVelocity = 5f;

        [Header("Jump Config")]
        [SerializeField]
        private float _jumpForce = 20f;

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

        InputAction _moveAction;
        InputAction _jumpAction;

        private Vector2 movementInput;
        private bool jumpInputLatch;

        private bool isGrounded;
        private float lastJumpTime;

        private void Awake()
        {
            _moveAction = InputSystem.actions.FindAction("Move");
            _jumpAction = InputSystem.actions.FindAction("Jump");
        }

        private void Update()
        {
            movementInput = _moveAction.ReadValue<Vector2>();

            if (!jumpInputLatch)
                jumpInputLatch = _jumpAction.WasPressedThisFrame();

            if (movementInput.x > 0)
                transform.rotation = Quaternion.Euler(0, 0, 0);
            else if (movementInput.x < 0)
                transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        private void FixedUpdate()
        {
            // we use a overlap instead of a raycast so the check is as wide as the player (almost)
            isGrounded = Physics2D.OverlapCircle(footTransform.position, groundCheckRadius, groundLayer);

            if (isGrounded)
            {
                if (jumpInputLatch)
                {
                    _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocityX, _jumpForce);
                    lastJumpTime = Time.time;
                    jumpInputLatch = false;
                }
                else if (movementInput != Vector2.zero)
                {
                    _rigidbody2D.linearVelocity = new Vector2(movementInput.x * _moveSpeed, _rigidbody2D.linearVelocity.y);
                }
                else
                {
                    _rigidbody2D.linearVelocity = new Vector2(0f, _rigidbody2D.linearVelocity.y);
                }
            }
            else
            {
                Vector2 newVelocity = Vector2.zero;

                newVelocity.x = _rigidbody2D.linearVelocityX + (movementInput.x * _airMoveSpeed);
                if (newVelocity.x > _maxAirMoveVelocity)
                    newVelocity.x = _maxAirMoveVelocity;
                else if (newVelocity.x < -_maxAirMoveVelocity)
                    newVelocity.x = -_maxAirMoveVelocity;

                //newVelocity.y = _rigidbody2D.linearVelocityY - (_gravity * Time.fixedDeltaTime);
                newVelocity.y = _rigidbody2D.linearVelocityY;
                if (newVelocity.y < -_maxFallVelocity)
                    newVelocity.y = -_maxFallVelocity;


                _rigidbody2D.linearVelocity = newVelocity;
            }
        }

        private void OnGUI()
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 48
            };

            GUILayout.BeginArea(new Rect(10, 10, 500, 100));
            GUILayout.Label($"Velocity: {_rigidbody2D.linearVelocity}", labelStyle);
            //GUILayout.Label($"Grounded: {isGrounded}", labelStyle);
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