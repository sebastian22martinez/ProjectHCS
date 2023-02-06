using UnityEngine;
using System.Collections;

[RequireComponent(typeof(BoxCollider2D))]
public class CharacterController2D : MonoBehaviour
{
    [SerializeField, Tooltip("Max speed, in units per second, that the character moves.")]
    float speed = 9;

    [SerializeField, Tooltip("Acceleration while grounded.")]
    float walkAcceleration = 75;

    [SerializeField, Tooltip("Acceleration while in the air.")]
    float airAcceleration = 30;

    [SerializeField, Tooltip("Deceleration applied when character is grounded and not attempting to move.")]
    float groundDeceleration = 70;

    [SerializeField, Tooltip("Max height the character will jump regardless of gravity")]
    float jumpHeight = 4;

    [SerializeField, Tooltip("Flag to determine if the character is grounded")]
    private bool grounded;

    private BoxCollider2D boxCollider;
    public Vector2 velocity;

    private GameObject[] ObjectsWorldA;
    private GameObject[] ObjectsWorldB;

    private bool worldA;
    private bool canSwitch;

    private float keyDownHoldTime = 0.250f;
    private float timePressed = 0.0f;
    private KeyCode switchKey = KeyCode.LeftShift;
    private Vector3 spawn;

    private void Awake()
    {
        spawn = transform.position;
        worldA = true;
        canSwitch = true;
        boxCollider = GetComponent<BoxCollider2D>();
        ObjectsWorldA = GameObject.FindGameObjectsWithTag("A");
        ObjectsWorldB = GameObject.FindGameObjectsWithTag("B");

        foreach (GameObject obj in ObjectsWorldB)
        {
            Color tmp = obj.GetComponent<SpriteRenderer>().color;
            tmp.a = 0.5f;
            obj.GetComponent<SpriteRenderer>().color = tmp;
        }
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.R))
            transform.position = spawn;

        if (Input.GetKeyDown(switchKey) && canSwitch)
        {
            timePressed = Time.time;
            switchWorlds();
        }

        if (Input.GetKeyUp(switchKey) && Time.time - timePressed > keyDownHoldTime && canSwitch)
        {
            switchWorlds();
        }

        if (grounded)
        {
            velocity.y = 0;

            if (Input.GetButtonDown("Jump"))
            {
                velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
            }
        }

        velocity.y += Physics2D.gravity.y * Time.deltaTime;

        float acceleration = grounded ? walkAcceleration : airAcceleration;
        float deceleration = grounded ? groundDeceleration : 0;

        float moveInput = Input.GetAxisRaw("Horizontal");

        // calculate the velocity of the player
        if (moveInput != 0)
        {
            // move in the direction of input at walking speed 
            velocity.x = Mathf.MoveTowards(velocity.x, speed * moveInput, acceleration * Time.deltaTime);
        }
        else
        {
            // decelerate at groundDeceleration speed
            velocity.x = Mathf.MoveTowards(velocity.x, 0, deceleration * Time.deltaTime);
        }

        // move the object
        transform.Translate(velocity * Time.deltaTime);


        // returns all the colliders that are within an area 
        // (in this case the size of the character's collider)
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0);

        if (hits.Length == 1)
        {
            canSwitch = true;
        }

        grounded = false;
        foreach (Collider2D hit in hits)
        {
            // because the Controller's collider is in the list of hits we have to make 
            // sure we dont try to handle a collision with ourselves
            if (hit == boxCollider)
            {
                continue;
            }

            // get the distance between colliders
            ColliderDistance2D colliderDistance = hit.Distance(boxCollider);

            // if we're colliding
            if (colliderDistance.isOverlapped)
            {
                // if the object we're colliding with is "inactive" disable world switching
                if (hit.GetComponent<SpriteRenderer>().color.a < 0.7f)
                    canSwitch = false;
                else
                {
                    // move the character the exast distance away to no longer be overlapping
                    transform.Translate(colliderDistance.pointA - colliderDistance.pointB);

                    if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && velocity.y < 0)
                        grounded = true;
                }
            }
        }
    }

    private void switchWorlds()
    {
        worldA = !worldA;
        foreach (GameObject obj in ObjectsWorldA)
        {
            // obj.SetActive(!obj.activeSelf);
            Color tmp = obj.GetComponent<SpriteRenderer>().color;
            if (tmp.a < 0.7f)
                tmp.a = 1.0f;
            else
                tmp.a = 0.5f;

            obj.GetComponent<SpriteRenderer>().color = tmp;
        }

        foreach (GameObject obj in ObjectsWorldB)
        {
            Color tmp = obj.GetComponent<SpriteRenderer>().color;
            if (tmp.a < 0.7f)
                tmp.a = 1.0f;
            else
                tmp.a = 0.5f;

            obj.GetComponent<SpriteRenderer>().color = tmp;

        }
    }
}
