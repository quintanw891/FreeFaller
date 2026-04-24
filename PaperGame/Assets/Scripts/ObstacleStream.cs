using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

enum Position {Top, Bottom, Left, Right}
enum MovementDirection {Clockwise, CounterClockwise};

[ExecuteAlways]
public class ObstacleStream : MonoBehaviour
{
    [SerializeField]
    private GameObject tilePrefab = null;
    [SerializeField]
    [Range(0.1f, 1f)]
    private float tileToTunnelRatio = 1;
    [SerializeField]
    [Range(0f, 5f)]
    private float speed = 1f;
    [SerializeField]
    [Tooltip("The wall that the tiles are attached to")]
    private Position position  = Position.Top;
    [SerializeField]
    [Tooltip("For example position top and direction clockwise means that the tiles will be attached to the top wall and move towards the right wall")]
    private MovementDirection movementDirection  = MovementDirection.Clockwise;

    private Queue<GameObject> tileQueue = new Queue<GameObject>();
    const float TUNNEL_2_SCALE_RATIO = 11;

    private Transform obstaclesFolder;

    void OnEnable()
    {
        // This script runs in both editor and play mode in order to preview
        // the duplicated tiles in editor mode.
        // OnEnable will create and enqueue the duplicated tiles.
        // OnDisable and OnDestroy will dequeue and destroy all tiles.
        // When transitioning to play mode from the editor the duplicated tiles for some
        // reason are not removed from the scene when DestroyImmediate is called but they
        // are dequeued.
        // As a workaround OnEnable will do this initial cleanup of any lingering child objects.
        Queue<GameObject> cleanupQueue = new Queue<GameObject>();
        obstaclesFolder = transform.Find("ObstaclesFolder");
        foreach (Transform child in obstaclesFolder)
        {
            // Debug.Log("ObstacleStream is cleaning up: " + child.gameObject.name);
            cleanupQueue.Enqueue(child.gameObject);
        }
        // Debug.Log("OnEnable cleanup starting. cleaning up " + cleanupQueue.Count);
        while (cleanupQueue.Count > 0)
        {
            SafeDestroyGameObject(cleanupQueue.Dequeue());
        }
        // Debug.Log("OnEnable cleanup complete cleanup queue size is " + cleanupQueue.Count);
        int numTiles = (int) Math.Ceiling(1f / tileToTunnelRatio) + 1;

        //Transform firstTilePosition WILO //Sep 2025 note: This may be where I was going to start using the 'position' and 'movement direction fields'
        // Debug.Log("OnEnable called and numTiles is "+ numTiles);
        for (int i=0; i<numTiles; i++)
        {
            GameObject tile = Instantiate(tilePrefab, obstaclesFolder, false);

            // Position tile at proper offset
            tile.transform.position =   tile.transform.position +
                                        (   transform.right * tile.transform.localScale.x *
                                            TUNNEL_2_SCALE_RATIO * tileToTunnelRatio * i);
            // scale tiles according to ratio.
            // Keep original Y scale to ensure collider is big enough to handle collisions
            Vector3 currentScale = tile.transform.localScale;
            currentScale.x *= tileToTunnelRatio;
            currentScale.z *= tileToTunnelRatio;
            tile.transform.localScale = currentScale;
            tileQueue.Enqueue(tile);
        }
    }

    public static GameObject SafeDestroyGameObject(GameObject gameobject)
    {
        if (gameobject != null){
            if (Application.isPlaying)
            {
                // Debug.Log("calling destroy on: " + gameobject);
                Destroy(gameobject);
                // Debug.Log("tried to destroy this: "+ gameobject);
            }
            else {
                // Debug.Log("calling destroyImmediate on: " + gameobject);
                DestroyImmediate(gameobject);
                // Debug.Log("tried to destroyimmediate this: "+ gameobject);
            }
        }
        return null;
    }

    void OnDisable()
    {
        // Debug.Log("OnDisable called and queue size is " + tileQueue.Count);
        while (tileQueue.Count > 0)
        {
            SafeDestroyGameObject(tileQueue.Dequeue());
        }
        // Debug.Log("After OnDisable queue size is " + tileQueue.Count);
    }

    void OnDestroy()
    {
        // Debug.Log("OnDestroy called and queue size is " + tileQueue.Count);
        while (tileQueue.Count > 0)
        {
            SafeDestroyGameObject(tileQueue.Dequeue());
        }
        // Debug.Log("After OnDestroy queue size is " + tileQueue.Count);
    }

    void Update()
    {
        if (Application.isPlaying)
        {
            foreach (GameObject tile in tileQueue)
            {
                // Perform the sliding motion
                tile.transform.position = tile.transform.position +
                                            (transform.right * -1 * tilePrefab.transform.localScale.x *
                                                TUNNEL_2_SCALE_RATIO * speed *
                                                Time.deltaTime);
            }
        }

        // Check if leading tile is no longer in the tunnel, if so rotate it to the back of the queue
        if (tileQueue.Count != 0)
        {
            float tileOffset = (tileQueue.Peek().transform.position.x +
                                    (tilePrefab.transform.localScale.x * TUNNEL_2_SCALE_RATIO * tileToTunnelRatio)) -
                                transform.position.x;

            if (tileOffset <= 0)
            {
                int numTiles = tileQueue.Count;
                GameObject tileToRotate = tileQueue.Dequeue();
                tileToRotate.transform.position = tileToRotate.transform.position +
                                                    (transform.right * tilePrefab.transform.localScale.x *
                                                        TUNNEL_2_SCALE_RATIO * tileToTunnelRatio * numTiles);
                tileQueue.Enqueue(tileToRotate);
            }
        }
    }
}
