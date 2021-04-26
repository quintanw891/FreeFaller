using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleSpawner : MonoBehaviour
{
    public Player player;
    public float firstObstacleDepth;
    //TODO add constraints to the floats below.
    //obstacleSpawnDepth < obstacleProjectDepth < obstacleDespawnHeight
    public float obstacleSpawnDepth;
    public float obstacleProjectDepth;
    public float obstacleDespawnHeight;
    private Queue<Obstacle> obstaclesToSpawn;
    private Queue<Obstacle> obstaclesToProject;
    private Queue<Obstacle> obstaclesToDespawn;
    [SerializeField]
    private GameObject obstacleProjectorPrefab = null;
    private bool rise;

    void Start()
    {
        rise = true;
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

        // Queue obstacles that are yet to spawn, project, and despawn
        obstaclesToSpawn = new Queue<Obstacle>();
        obstaclesToProject = new Queue<Obstacle>();
        obstaclesToDespawn = new Queue<Obstacle>();

        foreach (Obstacle obstacle in obstacles)
        {
            float obstacleDepth = obstacle.relativePosition.y + firstObstacleDepth;
            if(obstacleDepth < obstacleSpawnDepth - player.distanceFallen)
            {
                obstaclesToSpawn.Enqueue(obstacle);
            } else if (obstacleDepth < obstacleProjectDepth -  player.distanceFallen)
            {
                obstaclesToProject.Enqueue(obstacle);
                obstacle.transform.position = new Vector3(obstacle.relativePosition.x,
                            (firstObstacleDepth) + obstacle.relativePosition.y + player.distanceFallen,
                            obstacle.relativePosition.z);
                obstacle.gameObject.SetActive(true);
            } else if (obstacleDepth < obstacleDespawnHeight - player.distanceFallen)
            {
                obstaclesToDespawn.Enqueue(obstacle);
                obstacle.CreateProjector(obstacleProjectorPrefab, player);
                obstacle.transform.position = new Vector3(obstacle.relativePosition.x,
                                            (firstObstacleDepth) + obstacle.relativePosition.y + player.distanceFallen,
                                            obstacle.relativePosition.z);
                obstacle.gameObject.SetActive(true);
            }
        }
    }

    public void SetRise(bool value)
    {
        rise = value;
    }

    void Update()
    {
        // Despawn next obstacle when it reaches despawn point
        if (obstaclesToDespawn.Count > 0 &&
            player.distanceFallen > -1 * (firstObstacleDepth + obstaclesToDespawn.Peek().relativePosition.y - obstacleDespawnHeight))
            obstaclesToDespawn.Dequeue().gameObject.SetActive(false);

        // Activate projector for next obstacle when it reaches project point
        if (obstaclesToProject.Count > 0 &&
            player.distanceFallen > -1 * (firstObstacleDepth + obstaclesToProject.Peek().relativePosition.y - obstacleProjectDepth))
        {
            Obstacle obstacleToProject = obstaclesToProject.Dequeue();
            obstacleToProject.CreateProjector(obstacleProjectorPrefab, player);
            obstaclesToDespawn.Enqueue(obstacleToProject);
        }

        // Spawn next obstacle when it reaches spawn point
        float spawnOffset;
        if (obstaclesToSpawn.Count > 0 &&
            (spawnOffset = (player.distanceFallen - obstacleSpawnDepth) -
                            (-1 * (firstObstacleDepth + obstaclesToSpawn.Peek().relativePosition.y))) >= 0)
        {
            Obstacle obstacleToSpawn = obstaclesToSpawn.Dequeue();
            obstacleToSpawn.transform.position = new Vector3(obstacleToSpawn.relativePosition.x, obstacleSpawnDepth + spawnOffset,
                                                      obstacleToSpawn.relativePosition.z);
            obstacleToSpawn.gameObject.SetActive(true);
            obstaclesToProject.Enqueue(obstacleToSpawn);
        }

        // Update position of spawned obstacles
        if (rise)
        {
            float riseSpeed = player.GetComponent<Player>().baseFallSpeed + player.GetComponent<Player>().tiltAddedVerticalSpeed;
            foreach (Obstacle obstacle in obstaclesToDespawn)
            {
                obstacle.gameObject.transform.position = obstacle.gameObject.transform.position + (Vector3.up * riseSpeed * Time.deltaTime);
            }
            foreach (Obstacle obstacle in obstaclesToProject)
            {
                obstacle.gameObject.transform.position = obstacle.gameObject.transform.position + (Vector3.up * riseSpeed * Time.deltaTime);
            }
        }
    }
}
