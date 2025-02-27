using Pizza.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pizza
{
    // TODO
    // - wall jump?
    // - fix air jump height
    // - work on bunny hop (if you land on the ground and jump too late, you lose all momentum. The window is too tight)

    public sealed class PlayerController : PizzaMonoBehaviour
    {
        [Header("Components")]
        [SerializeField]
        private Rigidbody2D _rigidbody2D;
        [SerializeField]
        private Animator _animator;

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
        [SerializeField, Min(0f), Tooltip("A boost force applied if we hold the movement key down prior to landing.")]
        private float _walkAfterLandingBoost = 0.5f;

        [Header("Air Walk Config")]
        [SerializeField, Min(0f), Tooltip("A small force boost when holding mvoement key mid air.")]
        private float _airWalkForce = 4f;
        [SerializeField, Min(0f), Tooltip("A force added when the player tries to go the opposite direction while mid air.")]
        private float _airBrakeForce = 12f;

        [Header("Jump Config")]
        [SerializeField, Min(0f)]
        private float _jumpForce = 15f;
        [SerializeField, Min(0f), Tooltip("This is added to the normal jump force.")]
        private float _airJumpExtraForce = 5f;
        [SerializeField, Min(0f), Tooltip("This is added to the normal air walk force after jumping.")]
        private float _airBrakeExtraForceAfterJump = 12f;
        [SerializeField, Min(0)]
        private int _airJumpsAllowedCount = 2;
        [SerializeField, Min(0f), Tooltip("The time you must be airbone before you can double jump again.")]
        private float _airJumpCooldownTime = 0.2f;

        [Header("Dash Config")]
        [SerializeField, Min(0f)]
        private float _dashForce = 20;
        [SerializeField, Min(0f)]
        private float _airDashForce = 10;
        [SerializeField, Min(0f), Tooltip("The time in seconds before the player can dash again.")]
        private float _dashCooldownTime = 3f;

        //
        InputAction _moveAction;
        InputAction _jumpAction;
        InputAction _sprintAction;
        InputAction _dashAction;

        private Vector2 _movementInput;
        private bool _jumpInput;
        private bool _sprintInput;
        private bool _dashInput;

        private bool _isGrounded;
        private bool _prevIsGrounded;

        private bool _jumpRequested;
        private bool _jumpAllowed;
        private bool _jumpTriggered; // states if we are airborne because we jumped or not
        private int _airJumpsLeft;
        private float _airJumpCooldown;

        private bool _dashRequested;
        private bool _dashAllowed;
        private bool _dashTriggered;
        private float _dashCooldown;

        private string _lastAnimStateName;
        private PlayerStateEnum _currentState;

        private const string IDLE_ANIM = "Idle";
        private const string WALK_ANIM = "Walk";
        private const string JUMP_ANIM = "Jump";
        private const string FALL_ANIM = "Fall";
        private const string LAND_ANIM = "Land";
        private const string ROLL_ANIM = "Roll";

        private void Awake()
        {
            _moveAction = InputSystem.actions.FindAction("Move");
            _jumpAction = InputSystem.actions.FindAction("Jump");
            _sprintAction = InputSystem.actions.FindAction("Sprint");
            _dashAction = InputSystem.actions.FindAction("Dash");
        }

        private void Update()
        {
            // cache raw input
            _movementInput = _moveAction.ReadValue<Vector2>();
            _jumpInput = _jumpAction.IsPressed();
            _sprintInput = _sprintAction.IsPressed();
            _dashInput = _dashAction.IsPressed();

            // handle jump requests
            if (!_jumpRequested && _jumpAction.WasPressedThisFrame() && _jumpAllowed)
                _jumpRequested = true;
            if (!_dashRequested && _dashAction.WasPressedThisFrame() && _dashAllowed)
                _dashRequested = true;

            // handle jump cooldowns
            if (_airJumpCooldown > 0f)
                _airJumpCooldown -= Time.deltaTime;
            else if (_airJumpCooldown <= 0f && _airJumpsLeft > 0)
                _jumpAllowed = true;

            // handle dash cooldowns
            if (_dashCooldown > 0f)
                _dashCooldown -= Time.deltaTime;
            else if (_dashCooldown <= 0f)
                _dashAllowed = true;

            // handle dash triggered
            // TODO: this is not the best solution. We basically use the cooldown to check if a half second has passed
            if (_dashTriggered && _dashCooldown < (_dashCooldownTime - 0.35f))
                _dashTriggered = false;

            // always allow rotation
            if (_movementInput.x > 0)
                transform.rotation = Quaternion.Euler(0, 0, 0);
            else if (_movementInput.x < 0)
                transform.rotation = Quaternion.Euler(0, 180, 0);

            UpdateState();
            UpdateAnimator();
        }

        private void FixedUpdate()
        {
            // we use a overlap instead of a raycast so the check is as wide as the player (almost)
            _prevIsGrounded = _isGrounded;
            _isGrounded = Physics2D.OverlapCircle(footTransform.position, groundCheckRadius, groundLayer);

            if (!_prevIsGrounded && _isGrounded)
            {
                // we just became grounded, reset some values

                _jumpTriggered = false;
                _jumpAllowed = true;
                _airJumpsLeft = _airJumpsAllowedCount;
                _airJumpCooldown = 0f;

                //_dashTriggered = false;
                //_dashAllowed = true;
                //_dashCooldown = 0f;
            }

            if (_isGrounded)
            {
                if (_jumpRequested && _jumpAllowed)
                {
                    // clamp so that if velocity is negative, it doesnt look like we didnt jump
                    float newVelocityY = Mathf.Clamp(_rigidbody2D.linearVelocityY, 0f, _rigidbody2D.linearVelocityY) + _jumpForce;
                    _rigidbody2D.linearVelocityY = newVelocityY;

                    _jumpTriggered = true;
                    _jumpRequested = false;
                    _jumpAllowed = false;
                    _airJumpCooldown = _airJumpCooldownTime;
                }
                else if (_movementInput.x != 0f && _dashRequested && _dashAllowed)
                {
                    // TODO: add dash
                    float newVelocityX = _rigidbody2D.linearVelocityX + (_dashForce * _movementInput.x);
                    _rigidbody2D.linearVelocityX = newVelocityX;

                    _dashTriggered = true;
                    _dashRequested = false;
                    _dashAllowed = false;
                    _dashCooldown = _dashCooldownTime;
                }
                else if (_movementInput.x != 0f)
                {
                    // if we were trying to move and we just landed, dont make the landing so sticky. Add some velocity in that direction
                    float boostForce = (!_prevIsGrounded && _isGrounded) ? _walkAfterLandingBoost : 0f;

                    // basically lerp from our current velocity to the target velocity, at the rate of acceleration
                    float newVelocityX = boostForce + Mathf.MoveTowards(_rigidbody2D.linearVelocityX, (_movementInput.x * _walkSpeed) + boostForce, _walkAccelerationSpeed * Time.fixedDeltaTime);
                    _rigidbody2D.linearVelocityX = newVelocityX;
                    PizzaLogger.Log($"newVelocityX: {newVelocityX}");
                }
                else if (_movementInput.x == 0f)
                {
                    // basically lerp from our current velocity to 0, at the rate of Deceleration
                    float newVelocityX = Mathf.MoveTowards(_rigidbody2D.linearVelocityX, 0f, _walkDecelerationSpeed * Time.fixedDeltaTime);
                    //_rigidbody2D.linearVelocityX = newVelocityX;
                }
            }
            else
            {

                if (_airJumpsLeft > 0 && _jumpRequested)
                {
                    // TODO
                    // Right now air jumps go too high, but need to counter falling, or raising on a platform.
                    // Need to tweak this so that air jumps allow for better control, and less height

                    // add a boost in direction when air jumping (if we are air braking)
                    bool tryingToAirBrake = Mathf.Sign(_movementInput.x) != Mathf.Sign(_rigidbody2D.linearVelocityX);
                    float extraAirWalkForce = (tryingToAirBrake) ? (_airBrakeExtraForceAfterJump * _movementInput.x) : 0f;
                    // clamp so that if velocity is negative, it doesnt look like we didnt jump
                    float newVelocityY = Mathf.Clamp(_rigidbody2D.linearVelocityY, 0f, _rigidbody2D.linearVelocityY) + _jumpForce + _airJumpExtraForce;
                    _rigidbody2D.linearVelocity = new Vector2(_rigidbody2D.linearVelocityX + extraAirWalkForce, newVelocityY);

                    _jumpTriggered = true;
                    _jumpRequested = false;
                    _jumpAllowed = false;
                    _airJumpsLeft -= 1;
                    _airJumpCooldown = _airJumpCooldownTime;
                }

                if (_movementInput.x != 0f && _dashRequested && _dashAllowed)
                {
                    // player is dashing mid air
                    float newVelocityX = _rigidbody2D.linearVelocityX + (_airDashForce * _movementInput.x);
                    _rigidbody2D.linearVelocityX = newVelocityX;

                    _dashTriggered = true;
                    _dashRequested = false;
                    _dashAllowed = false;
                    _dashCooldown = _dashCooldownTime;
                }
                else if (_movementInput.x != 0f)
                {
                    if (Mathf.Sign(_movementInput.x) != Mathf.Sign(_rigidbody2D.linearVelocityX))
                    {
                        // player is trying to change directions mid air
                        float newVelocityX = _rigidbody2D.linearVelocityX + (_movementInput.x * _airBrakeForce * Time.deltaTime);
                        _rigidbody2D.linearVelocityX = newVelocityX;
                    }
                    else
                    {
                        // player is trying to air walk
                        float newVelocityX = _rigidbody2D.linearVelocityX + (_movementInput.x * _airWalkForce * Time.deltaTime);
                        _rigidbody2D.linearVelocityX = newVelocityX;
                    }
                }
            }
        }

        private void UpdateState()
        {
            if (_dashTriggered)
                _currentState = PlayerStateEnum.Dash;
            else if (_jumpTriggered && _rigidbody2D.linearVelocityY > 0f)
                _currentState = PlayerStateEnum.Jump;
            // TODO: think on if we care about this or not
            //else if (_isGrounded && _rigidbody2D.linearVelocityY > 1f)
            //    PizzaLogger.LogWarning($"PlayerController::UpdateAnimator found a time where we are mid air going up, but did not jump");
            else if (!_isGrounded && _rigidbody2D.linearVelocityY < 0f)
                _currentState = PlayerStateEnum.Fall;
            else if (_movementInput.x != 0f && _isGrounded)
                _currentState = PlayerStateEnum.Walk;
            else if (_movementInput.x == 0f && _isGrounded)
                _currentState = PlayerStateEnum.Idle;

            PlayerState.shared.State = _currentState;
        }

        private void UpdateAnimator()
        {
            switch (_currentState)
            {
                case PlayerStateEnum.Idle:
                    PlayAnimatorAnim(IDLE_ANIM);
                    break;
                case PlayerStateEnum.Walk:
                    PlayAnimatorAnim(WALK_ANIM);
                    break;
                case PlayerStateEnum.Jump:
                    PlayAnimatorAnim(JUMP_ANIM);
                    break;
                case PlayerStateEnum.Fall:
                    PlayAnimatorAnim(FALL_ANIM);
                    break;
                case PlayerStateEnum.Dash:
                    PlayAnimatorAnim(ROLL_ANIM);
                    break;
                default:
                    PlayAnimatorAnim(IDLE_ANIM);
                    break;

            }
            ;
        }

        private void PlayAnimatorAnim(string animName)
        {
            if (_lastAnimStateName == animName)
                return;

            _lastAnimStateName = animName;
            _animator.Play(animName);
        }

        private void OnGUI()
        {
            var labelStyle = new GUIStyle(GUI.skin.label)
            {
                fontSize = 48
            };

            GUILayout.BeginArea(new Rect(10, 10, 500, 100));
            GUILayout.Label($"Velocity: {_rigidbody2D.linearVelocity}", labelStyle);
            //GUILayout.Label($"Grounded: {_isGrounded}", labelStyle);
            //GUILayout.Label($"_dashTriggered: {_dashTriggered}", labelStyle);
            //GUILayout.Label($"_jumpTriggered: {_jumpTriggered}", labelStyle);
            //GUILayout.Label($"_jumpHang: {_jumpHang}", labelStyle);
            //GUILayout.Label($"_jumpHangTimeLeft: {_jumpHangTimeLeft}", labelStyle);
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