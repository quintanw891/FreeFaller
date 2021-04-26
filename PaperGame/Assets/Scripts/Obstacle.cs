using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    [HideInInspector]
    public Vector3 relativePosition; //position relative to other obstacles
    [HideInInspector]
    public bool spawn; //Should this obstacle spawn in the scene

    void Awake()
    {
        relativePosition = transform.position;
        spawn = true;
    }

    // Returns -1 if current obstacle exists above the input obstacle,
    // 0 if both obstacles exist at the same height, and 1 otherwise
    public static int CompareByDepth(Obstacle o1, Obstacle o2)
    {
        return o1.relativePosition.y.CompareTo(o2.relativePosition.y) * -1;
    }

    public void CreateProjector(GameObject projectorPrefab, Player player)
    {
        ObstacleProjector projector = Instantiate(projectorPrefab, transform).GetComponent<ObstacleProjector>();
        projector.SetPlayer(player);
    }

    public string PrintMe()
    {
        return relativePosition.y.ToString();
    }
}
