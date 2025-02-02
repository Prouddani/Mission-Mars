using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : MonoBehaviour
{
    private PlayerInput pia;
    private Rigidbody2D _rb;

    [Header("Movement")]
    public float runSpeed;
    public float jumpPower;

    [Header("Layer Masks")]
    public LayerMask groundLayer;

    #region TIMER VARIABLES
    private float lastStanding;
    private float lastJump;
    #endregion

    void Start()
    {
        _rb = GetComponent<Rigidbody2D>();

        pia = new PlayerInput();
        pia.Player.Enable();
        pia.Player.Jump.performed += (InputAction.CallbackContext context) =>
        {
            if (context.performed && IsStandingWithCoyote())
            {
                //_rb.AddForce(Vector2.up * jumpPower, ForceMode2D.Impulse);
                _rb.linearVelocity = new Vector2(_rb.linearVelocity.x, jumpPower);
                lastStanding = -Mathf.Infinity;
                lastJump = Time.fixedTime;
            }
        };
    }

    void Update()
    {
        
    }

    void FixedUpdate()
    {
        #region Coyote Time
        if (IsStanding() && Time.fixedTime - lastJump > 0.3f)
        {
            lastStanding = Time.fixedTime;  // if player is standing (ground below them), it'll set lastStanding to Time.fixedTime,
                                            // Helpful for the method IsStandingWithCoyote
        }
        #endregion

        Vector2 dir = pia.Player.Movement.ReadValue<Vector2>();
        _rb.linearVelocity = new Vector2(
            (Mathf.Abs(dir.x) > 0) ? Mathf.Sign(dir.x) * runSpeed : 0,
            _rb.linearVelocity.y
        );
    }

    public RaycastHit2D IsStanding()
    {
        ContactFilter2D filter = new();
        filter.useLayerMask = true;
        filter.layerMask = groundLayer;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up, transform.localScale.y / 2 + 0.1f, groundLayer);
        return hit;
    }

    public bool IsStandingWithCoyote()
    {
        // checks if player is in mid air, but player can still jump,
        // for fluid movements and better control of the character

        Debug.Log(Time.fixedTime - lastStanding);
        return Time.fixedTime - lastStanding < 0.15f;
    }
}
