using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public Player player;
    public float obstacleDespawnHeight;
    public float firstObstacleDepth;
    public float obstacleSpawnDepth;
    private Queue<Obstacle> obstaclesToSpawn;
    private Queue<Obstacle> spawnedObstacles;

    void Start()
    {
        InitObstacles();
    }

    public void InitObstacles()
    {
        // Sort all obstacles by depth and set all as deactive
        List<Obstacle> obstacles = new List<Obstacle>();
        foreach (Transform obstacleTransform in transform.Find("Obstacles"))
        {
            GameObject obstacleObject = obstacleTransform.gameObject;
            obstacleObject.SetActive(false);
            Obstacle obstacle = obstacleObject.GetComponent<Obstacle>();
            if (obstacle.spawn)
            {
                obstacles.Add(obstacle);
            }
        }
        obstacles.Sort(Obstacle.CompareByDepth);

        // Queue spawned and unspawned obstacles
        spawnedObstacles = new Queue<Obstacle>();
        obstaclesToSpawn = new Queue<Obstacle>();

        foreach (Obstacle obstacle in obstacles)
        {
            float obstacleDepth = obstacle.relativePosition.y + firstObstacleDepth;
            if (obstacleDepth >= obstacleSpawnDepth - player.distanceFallen &&
                obstacleDepth < obstacleDespawnHeight - player.distanceFallen)
                spawnedObstacles.Enqueue(obstacle);
            else if(obstacleDepth < obstacleSpawnDepth - player.distanceFallen)
                obstaclesToSpawn.Enqueue(obstacle);
        }

        // Activate spawned obstacles
        foreach (Obstacle obstacle in spawnedObstacles)
        {
            obstacle.transform.position = new Vector3(  obstacle.relativePosition.x,
                                                        (firstObstacleDepth) + obstacle.relativePosition.y + player.distanceFallen,
                                                        obstacle.relativePosition.z);
            obstacle.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        // Deactivate highest obstacle when it reaches despawn point
        if (spawnedObstacles.Count > 0 &&
            player.distanceFallen > -1 * (firstObstacleDepth + spawnedObstacles.Peek().relativePosition.y - obstacleDespawnHeight))
            spawnedObstacles.Dequeue().gameObject.SetActive(false);

        // Activate next obstacle when within spawn range of player's depth
        float spawnOffset;
        if (obstaclesToSpawn.Count > 0 &&
            (spawnOffset = (player.distanceFallen - obstacleSpawnDepth) -
                            (-1 * (firstObstacleDepth + obstaclesToSpawn.Peek().relativePosition.y))) >= 0)
        {
            Obstacle obstacleToSpawn = obstaclesToSpawn.Dequeue();
            obstacleToSpawn.transform.position = new Vector3(obstacleToSpawn.relativePosition.x, obstacleSpawnDepth + spawnOffset,
                                                      obstacleToSpawn.relativePosition.z);
            spawnedObstacles.Enqueue(obstacleToSpawn);
            obstacleToSpawn.gameObject.SetActive(true);
        }

        float riseSpeed = player.GetComponent<Player>().baseFallSpeed + player.GetComponent<Player>().tiltAddedVerticalSpeed;
        foreach (Obstacle obstacle in spawnedObstacles)
        {
            obstacle.gameObject.transform.position = obstacle.gameObject.transform.position + (Vector3.up * riseSpeed * Time.deltaTime);
        }
    }
}
