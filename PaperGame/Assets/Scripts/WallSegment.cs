using UnityEngine;

public class WallSegment : MonoBehaviour
{
    [HideInInspector]
    public Vector3 startPosition;

    void Awake()
    {
        startPosition = transform.position;
    }

    void Start()
    {

    }

    void Update()
    {

    }
    
}
