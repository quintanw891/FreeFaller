﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

enum MovementMode {Physics, Simple};

public class Player : MonoBehaviour
{
    // Movement fields
    [SerializeField]
    private MovementMode movementMode = MovementMode.Simple;
    // Vertical
    public float baseFallSpeed;
    [HideInInspector]
    public float tiltAddedVerticalSpeed;
    public float maxAddedVerticalSpeed;
    [HideInInspector]
    public float distanceFallen;
    [SerializeField]
    private float startDistanceFallen = 0;   // Distance fallen at start of scene
    private float spawnDistanceFallen;  // Distance fallen at respawn
    private bool updateDistanceFallen;
    // Lateral
    private float lateralSpeed;
    Vector3 lateralMovement;
    [SerializeField]
    private float maxLateralSpeed = 0.013f;

    // Objects in scene
    public ObstacleSpawner obstacleSpawner;
    public Transform walls;
    public GameObject tutorial;
    public GameObject scraps;

    // Other
    private Animator animator;
    [SerializeField]
    private int maxHealth = 1;
    private int health;
    private bool invincible;
    private bool dead;
    [SerializeField]
    private float invincibleDurationSec = 1;
    private IEnumerator invincibleRoutine;
    private bool invincibleRoutineRunning;
    private IEnumerator ripApartRoutine;
    PlayerControls controls;
    Vector2 tiltInput;
    private bool diving = false;
    Vector2 move;
    Vector3 startPosition;  // Position of Player at start of scene
    Vector3 tilt;

    void Awake()
    {
        controls = new PlayerControls();
        controls.Gameplay.Tilt.performed += ctx => tiltInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Tilt.canceled += ctx => tiltInput = Vector2.zero;
        if(movementMode == MovementMode.Simple)
        {
            controls.Gameplay.Dive.performed += ctx => diving = true;
            controls.Gameplay.Dive.canceled += ctx => diving = false;
        }
        controls.Gameplay.Enable();

        startPosition = transform.position;
        spawnDistanceFallen = startDistanceFallen;
        invincibleRoutineRunning = false;
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    void Start()
    {
        InitPlayer();
        if (tutorial && tutorial.activeSelf)
        {
            updateDistanceFallen = false;
            obstacleSpawner.gameObject.SetActive(false);
        }
        else
        {
            updateDistanceFallen = true;
        }
    }

    void InitPlayer()
    {
        transform.position = startPosition;
        tilt = Vector3.zero;
        tiltAddedVerticalSpeed = 0f;
        distanceFallen = spawnDistanceFallen;
        lateralMovement = Vector3.zero;
        lateralSpeed = 0f;
        health = maxHealth;
        invincible = false;
        animator.SetBool("invincible", false);
        dead = false;
        animator.SetBool("dead", false);
        if (invincibleRoutineRunning)
        {
            StopCoroutine(invincibleRoutine);
        }
    }
    
    void Update()
    {
        if (!dead)
        {
            float tiltMagnitude;
            float maxTilt;
            switch (movementMode)
            {
                case MovementMode.Physics:
                    // Transform magnitude of player directional input to favor 50% intensity
                    tiltMagnitude = (Mathf.Pow(2 * (tiltInput.magnitude) - 1, 3) + 1) / 2;
                    maxTilt = 1f;
                    tiltAddedVerticalSpeed = tiltMagnitude * maxAddedVerticalSpeed;
                    lateralSpeed = Mathf.Sin(tiltMagnitude * Mathf.PI) * maxLateralSpeed;
                    break;
                case MovementMode.Simple:
                default:
                    tiltMagnitude = tiltInput.magnitude;
                    maxTilt = 0.5f;
                    tiltAddedVerticalSpeed = 0f;
                    lateralSpeed = tiltMagnitude * maxLateralSpeed;
                    if (diving)
                    {
                        tiltMagnitude = 1f;
                        maxTilt = 0.8f;
                        tiltAddedVerticalSpeed = maxAddedVerticalSpeed;
                    }
                    break;
            }

            // Update the orientation of the player
            if (tiltInput.magnitude == 0) // No Input
            {
                transform.eulerAngles = new Vector3(tiltMagnitude * 90, transform.eulerAngles.y, 0);
            }
            else if (tiltInput.x > 0 && tiltInput.y >= 0) // Q I
            {
                transform.eulerAngles = new Vector3(tiltMagnitude * 90 * maxTilt, -270 + Mathf.Atan(tiltInput.y / tiltInput.x) * Mathf.Rad2Deg * -1, 0);
            }
            else if (tiltInput.x <= 0 && tiltInput.y > 0) // Q II
            {
                transform.eulerAngles = new Vector3(tiltMagnitude * 90 * maxTilt, Mathf.Atan(-tiltInput.x / tiltInput.y) * Mathf.Rad2Deg * -1, 0);
            }
            else if (tiltInput.x < 0 && tiltInput.y <= 0) // Q III
            {
                transform.eulerAngles = new Vector3(tiltMagnitude * 90 * maxTilt, -90 + Mathf.Atan(-tiltInput.y / -tiltInput.x) * Mathf.Rad2Deg * -1, 0);
            }
            else // Q IV
            {
                transform.eulerAngles = new Vector3(tiltMagnitude * 90 * maxTilt, -180 + Mathf.Atan(tiltInput.x / -tiltInput.y) * Mathf.Rad2Deg * -1, 0);
            }

            // Update the vertical and lateral speed of the player
            if (updateDistanceFallen)
                distanceFallen += (baseFallSpeed + tiltAddedVerticalSpeed) * Time.deltaTime;
            Vector3 tiltDirection = (new Vector3(tiltInput.x, 0, tiltInput.y)).normalized;
            lateralMovement = tiltDirection * lateralSpeed;
            List<Transform> blockingWalls = GetBlockingWalls(lateralMovement);
            if (blockingWalls.Count == 0)
                transform.Translate(lateralMovement, Space.World);
            else if (blockingWalls.Count == 1)
            {
                // This is the wall normal rotated into the world up plane
                Vector3 adjustedWallNormal = Vector3.Normalize(Vector3.ProjectOnPlane(blockingWalls[0].up * 100, Vector3.up));
                float normalToMovement = Vector3.SignedAngle(adjustedWallNormal, lateralMovement, Vector3.up);
                float lateralSpeed = Mathf.Abs(lateralMovement.magnitude * Mathf.Cos(Mathf.Abs(normalToMovement) - 90));
                lateralMovement = adjustedWallNormal * lateralSpeed;
                //Debug.Log("Normal: " + lateralMovement);
                //Debug.Log("Lat Speed: " + lateralSpeed);
                if (normalToMovement > 0) // slide to the wall's right
                    lateralMovement = Quaternion.Euler(0, 90, 0) * lateralMovement;
                else // slide to the wall's left
                    lateralMovement = Quaternion.Euler(0, -90, 0) * lateralMovement;
                transform.Translate(lateralMovement, Space.World);

                //Debug.Log("Angle" + normalToMovement);
            }

            //if(canMove(lateralMovement, walls))
            //    transform.Translate(lateralMovement, Space.World);
        }
    }

    /*
     * Return the list of walls that block the given movement.
     */
    List<Transform> GetBlockingWalls(Vector3 movement)
    {
        List<Transform> blockingWalls = new List<Transform>();
        foreach (Transform wall in walls)
        {
            //Offset each wall by the player's diagonal radius to ensure
            //no part of the player clips through walls.
            Plane plane = new Plane(wall.gameObject.transform.up,
                                    wall.gameObject.transform.position);
            float bufferMultiple = 1.1f; // Add additional buffer space to avoid clipping
            plane = Plane.Translate(plane, plane.normal * (transform.lossyScale.x * Mathf.Sqrt(0.5f)) * -1 * bufferMultiple);
            if (!plane.GetSide(transform.position + movement))
            {
                blockingWalls.Add(wall);
            }
        }
        return blockingWalls;
    }

    /*
       Return true if the player can translate to movement without
       passing through any child of walls
     
    bool canMove(Vector3 movement, Transform walls)
    {
        foreach (Transform wall in walls )
        {
            Plane plane = new Plane(wall.gameObject.transform.up,
                                    wall.gameObject.transform.position);
            plane = Plane.Translate(plane, plane.normal * transform.lossyScale.x/2 * -1);
            if (!plane.GetSide(transform.position + movement))
            {
                return false;
            }
        }
        return true;
    }
    */

    private IEnumerator BecomeInvincible()
    {
        invincibleRoutineRunning = true;
        invincible = true;
        animator.SetBool("invincible", true);
        yield return new WaitForSeconds(invincibleDurationSec);
        invincible = false;
        animator.SetBool("invincible", false);
        invincibleRoutineRunning = false;
    }
    
    public IEnumerator RipApart()
    {
        scraps.transform.position = transform.position;
        ParticleSystem pSystem = scraps.GetComponent<ParticleSystem>();
        pSystem.Play();
        yield return new WaitForSeconds(pSystem.main.duration);
        InitPlayer();
        obstacleSpawner.InitObstacles();
        gameObject.GetComponent<BoxCollider>().enabled = true;
        updateDistanceFallen = true;
        foreach (Transform wall in walls)
        {
            wall.gameObject.GetComponent<Wall>().SetScroll(true);
        }
        obstacleSpawner.SetRise(true);
    }

    private void die()
    {
        dead = true;
        animator.SetBool("dead", true);
        tiltAddedVerticalSpeed = 0f;
        gameObject.GetComponent<BoxCollider>().enabled = false;
        updateDistanceFallen = false;
        foreach (Transform wall in walls)
        {
            wall.gameObject.GetComponent<Wall>().SetScroll(false);
        }
        obstacleSpawner.SetRise(false);
        ripApartRoutine = RipApart();
        StartCoroutine(ripApartRoutine);
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Obstacle":
                //Debug.Log("Enter Obstacle " + other.gameObject.name);
                if (!invincible)
                {
                    health -= 1;
                    if (health <= 0)
                    {
                        die();
                    }
                    else
                    {
                        invincibleRoutine = BecomeInvincible();
                        StartCoroutine(invincibleRoutine);
                    }
                }
                break;
            case "Death Zone":
                //Debug.Log("Death Zone Collision");
                die();
                break;
            case "Collectable":
                //Debug.Log("Collectable Collision");
                other.gameObject.SetActive(false);
                break;
            case "Checkpoint":
                //Debug.Log("Checkpoint Collision");
                other.gameObject.GetComponent<Obstacle>().spawn = false;
                spawnDistanceFallen = distanceFallen;
                break;
            default:
                //Debug.Log("Default Collision");
                break;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        switch (other.tag)
        {
            case "Obstacle":
                //Debug.Log("Stay Obstacle " + other.gameObject.name);
                if (!invincible)
                {
                    health -= 1;
                    if (health <= 0)
                    {
                        die();
                    }
                    else
                    {
                        invincibleRoutine = BecomeInvincible();
                        StartCoroutine(invincibleRoutine);
                    }
                }
                break;
            default:
                break;
        }
    }

    public void OnTutorialComplete()
    {
        tutorial.SetActive(false);
        updateDistanceFallen = true;
        obstacleSpawner.gameObject.SetActive(true);
    }

    void onEnable()
    {
        controls.Gameplay.Enable();
    }

    void onDisable()
    {
        controls.Gameplay.Disable();
    }

    public int getHealth()
    {
        return health;
    }
}
