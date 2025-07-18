using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Animator))]
public class NPCMovement : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float waitTimeAtPoint = 0.5f;
    public Transform[] waypoints;
    public GameObject exclamationMark;

    private Rigidbody2D rb;
    private Animator animator;
    private int currentWaypointIndex = 0;
    private bool isMoving = true;
    private bool isWaiting = false;
    private Vector2 movement;
    public Transform player;
    public float detectionDistance = 3f;
    public LayerMask obstacleMask;
    public float alertDuration = 2f;

    private bool playerSpotted = false;
    private float alertTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        if (exclamationMark != null)
            exclamationMark.SetActive(false);
    }

    void Update()
    {
        if (!isMoving || isWaiting)
        {
            animator.SetBool("IsMoving", false);
            return;
        }

        // Vision check for player using fan of raycasts
        if (player != null)
        {
            Vector2 rayOrigin = (Vector2)transform.position + Vector2.up * 0.5f;
            Vector2 directionToPlayer = ((Vector2)player.position - rayOrigin).normalized;
            float raySpread = 10f;
            float distance = detectionDistance;

            // Always draw a central debug ray in red for visibility
            Debug.DrawLine(rayOrigin, rayOrigin + directionToPlayer * distance, Color.red);

            Vector2[] rayDirs = new Vector2[]
            {
                directionToPlayer,
                Quaternion.Euler(0, 0, raySpread) * directionToPlayer,
                Quaternion.Euler(0, 0, -raySpread) * directionToPlayer
            };

            foreach (var dir in rayDirs)
            {
                Debug.DrawLine(rayOrigin, rayOrigin + dir * distance, Color.red);

                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, dir, distance, obstacleMask);
                if (hit.collider != null && hit.collider.CompareTag("Player"))
                {
                    SpotPlayer(directionToPlayer);
                    break;
                }
            }
        }

        if (playerSpotted)
        {
            alertTimer -= Time.deltaTime;

            if (alertTimer <= 0f)
            {
                Debug.Log("Resuming patrol.");
                playerSpotted = false;
                isMoving = true;

                if (exclamationMark != null)
                    exclamationMark.SetActive(false);
            }

            return;
        }

        Vector2 target = waypoints[currentWaypointIndex].position;
        movement = (target - (Vector2)transform.position).normalized;

        animator.SetBool("IsMoving", true);
        SetDirectionForAnimation(movement);

        if (Vector2.Distance(transform.position, target) < 0.1f)
        {
            StartCoroutine(WaitAtWaypoint());
        }
    }

    void FixedUpdate()
    {
        if (isMoving && !isWaiting)
        {
            rb.MovePosition(rb.position + movement * moveSpeed * Time.fixedDeltaTime);
        }
    }

    IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        rb.velocity = Vector2.zero;
        animator.SetBool("IsMoving", false);
        yield return new WaitForSeconds(waitTimeAtPoint);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        isWaiting = false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("Player collided with NPC: " + gameObject.name);
            isMoving = false;
            rb.velocity = Vector2.zero;
            animator.SetBool("IsMoving", false);

            if (exclamationMark != null)
                exclamationMark.SetActive(true);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            isMoving = true;

            if (exclamationMark != null)
                exclamationMark.SetActive(false);
        }
    }

    void SetDirectionForAnimation(Vector2 dir)
    {
        if (Mathf.Abs(dir.x) > Mathf.Abs(dir.y))
        {
            animator.SetInteger("Direction", dir.x > 0 ? 2 : 1); // Right : Left
        }
        else
        {
            animator.SetInteger("Direction", dir.y > 0 ? 3 : 0); // Up : Down
        }
    }
    void SpotPlayer(Vector2 directionToPlayer)
    {
        if (!playerSpotted)
        {
            Debug.Log("Player spotted!");
            playerSpotted = true;
            isMoving = false;
            rb.velocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
            SetDirectionForAnimation(directionToPlayer);

            if (exclamationMark != null)
                exclamationMark.SetActive(true);

            alertTimer = alertDuration;
        }
    }
}