using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleLooper : MonoBehaviour
{

    public GameObject player;
    private float loopbackHeight = -40;

    void Start()
    {
        
    }

    void LateUpdate()
    {
        foreach (Transform obstacleTransform in transform.Find("Obstacles"))
        {
            GameObject obstacle = obstacleTransform.gameObject;
            if (obstacleTransform.position.y >= player.transform.position.y + 5)
            {
                obstacleTransform.position = new Vector3(obstacle.GetComponent<Obstacle>().relativePosition.x, loopbackHeight,
                                                         obstacle.GetComponent<Obstacle>().relativePosition.z);
            }
            else
            {
                float riseSpeed = player.GetComponent<Player>().baseFallSpeed + player.GetComponent<Player>().tiltAddedVerticalSpeed;
                obstacleTransform.position = obstacleTransform.position + (Vector3.up * riseSpeed * Time.deltaTime);
            }
        }
    }
}
