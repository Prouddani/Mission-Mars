using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerBehaviour : MonoBehaviour
{
    private PlayerInput pia;
    private Rigidbody2D _rb;

    [Header("Game Objects & Transforms")]
    #region GAME OBJECTS AND TRANSFORMS
    public GameObject interactionDisplay;
    
    private Transform grabPosition;
    #endregion

    [Header("Movement")]
    public float runSpeed;
    public float jumpPower;

    [Header("Behaviour")]
    public float interactionDistance = 7f;

    [Header("Layer Masks")]
    public LayerMask groundLayer;

    #region CONTROL
    private float lookDirection;
    #endregion

    #region TIMER VARIABLES
    private float lastStanding;
    private float lastJump;
    #endregion

    void Start()
    {
        grabPosition = transform.Find("Grab");
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
        pia.Player.Interact.performed += (InputAction.CallbackContext context) =>
        {
            if (context.performed)
            {
                Interact();
            }
        };
    }

    void Update()
    {
        grabPosition.localPosition = new Vector2(0.6f * lookDirection, 0);

        if (grabPosition.childCount > 0)
        {

        }
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
        if (Mathf.Abs(dir.x) > 0.2f) lookDirection = Mathf.Sign(dir.x) * 1;
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

        // Player can only jump in mid air, if the time between the last standed and now is lower than 150 miliseconds
        return Time.fixedTime - lastStanding < 0.15f;
    }

    private void SetPhysics(GameObject target, bool value)
    {
        Rigidbody2D rigid = target.GetComponent<Rigidbody2D>();
        rigid.bodyType = (value) ? RigidbodyType2D.Dynamic : RigidbodyType2D.Kinematic;

        Collider2D collider = target.GetComponent<Collider2D>();
        collider.enabled = value;
    }

    public void Interact()
    {
        List<GameObject> interactives = GameObject.FindGameObjectsWithTag("Interactive").ToList();

        Transform nearest = null;
        foreach (GameObject interact in interactives)
        {
            if (Vector2.Distance(interact.transform.position, transform.position) <= interactionDistance)
            {
                if (nearest == null || Vector2.Distance(interact.transform.position, transform.position) < Vector2.Distance(transform.position, nearest.position))
                    nearest = interact.transform;
            }
        }

        if (grabPosition.childCount == 0)
        {
            if (nearest != null)
            {
                IInteract behaviour = nearest.GetComponent<IInteract>();
                if (behaviour != null)
                {
                    behaviour.OnInteracted.Invoke();

                    if (behaviour.type == InteractionType.Grabbable)
                    {
                        nearest.parent = grabPosition;
                        nearest.localPosition = Vector2.zero;

                        SetPhysics(nearest.gameObject, false); // Disables Rigidbody2D and all Colliders2D
                    }
                }
            }
        }
        else
        {
            Transform interact = grabPosition.GetChild(0);
            interact.parent = null;
            interact.position = (Vector2)transform.position + (Vector2.right * lookDirection * 1);

            SetPhysics(interact.gameObject, true);
        }
    }
}
