using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    // Vertical movement fields
    public float baseFallSpeed;
    [HideInInspector]
    public float tiltAddedVerticalSpeed;
    public float maxAddedVerticalSpeed;
    [HideInInspector]
    public float distanceFallen;
    public float startDistanceFallen;
    private bool updateDistanceFallen;

    // Lateral movement fields
    private float lateralSpeed;
    Vector3 lateralMovement;
    public float maxLateralSpeed;

    // Objects in scene
    public ObstacleSpawner obstacleSpawner;
    public Transform walls;
    public GameObject tutorial;

    PlayerControls controls;
    Vector2 tiltInput;
    Vector2 move;
    Vector3 startPosition;
    Vector3 tilt;

    void Awake()
    {
        controls = new PlayerControls();

        controls.Gameplay.Tilt.performed += ctx => tiltInput = ctx.ReadValue<Vector2>();
        controls.Gameplay.Tilt.canceled += ctx => tiltInput = Vector2.zero;

        controls.Gameplay.Enable();
        startPosition = transform.position;
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
        distanceFallen = startDistanceFallen;
        lateralMovement = Vector3.zero;
        lateralSpeed = 0f;
    }
    
    void Update()
    {
        if (tiltInput.magnitude == 0) // No Input
        {
            transform.eulerAngles = new Vector3(0, transform.eulerAngles.y, 0);
        }
        else if (tiltInput.x > 0 && tiltInput.y >= 0) // Q I
        {
            transform.eulerAngles = new Vector3(tiltInput.magnitude * 90, -270 + Mathf.Atan(tiltInput.y / tiltInput.x) * Mathf.Rad2Deg * -1, 0);
        }else if (tiltInput.x <= 0 && tiltInput.y > 0) // Q II
        {
            transform.eulerAngles = new Vector3(tiltInput.magnitude * 90, Mathf.Atan(-tiltInput.x / tiltInput.y) * Mathf.Rad2Deg * -1, 0);
        }
        else if (tiltInput.x < 0 && tiltInput.y <= 0) // Q III
        {
            transform.eulerAngles = new Vector3(tiltInput.magnitude * 90, -90 + Mathf.Atan(-tiltInput.y / -tiltInput.x) * Mathf.Rad2Deg * -1, 0);
        }
        else // Q IV
        {
            transform.eulerAngles = new Vector3(tiltInput.magnitude * 90, -180 + Mathf.Atan(tiltInput.x / -tiltInput.y) * Mathf.Rad2Deg * -1, 0);
        }
        //TODO use tiltInput instead of tilt from this point on
        tilt = new Vector3(tiltInput.y * 90, 0, tiltInput.x * 90 * -1);
        tiltAddedVerticalSpeed = tilt.magnitude * maxAddedVerticalSpeed / 90;
        if (updateDistanceFallen)
            distanceFallen += (baseFallSpeed + tiltAddedVerticalSpeed) * Time.deltaTime;
        lateralSpeed = Mathf.Sin(tilt.magnitude * Mathf.PI / 90) * maxLateralSpeed;
        Vector3 tiltDirection = (new Vector3(tilt.z * -1, 0, tilt.x) / 90).normalized;
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

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Obstacle":
                //Debug.Log("Obstacle Collision");
                InitPlayer();
                obstacleSpawner.InitObstacles();
                break;
            case "Collectable":
                //Debug.Log("Collectable Collision");
                other.gameObject.SetActive(false);
                break;
            default:
                //Debug.Log("Default Collision");
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
}
