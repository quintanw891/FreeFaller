using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public Player player;
    public float scrollSpeed;
    private Renderer rend;
    private bool scroll;

    // variables for segment looping
    public bool loopSegments; //when enabled loop child segments for scroll effect, otherwise scroll texture
    private readonly float segmentDespawnHeight = 0.6f;
    private Transform lowestSegment;
    private float segmentsOffset = -1;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (loopSegments)
        {
            foreach (Transform segmentTransform in transform.Find("Segments"))
            {
                if (lowestSegment == null || lowestSegment.position.y > segmentTransform.position.y)
                {
                    lowestSegment = segmentTransform;
                }
                if (segmentsOffset == -1)
                {
                    MeshRenderer renderer = segmentTransform.gameObject.GetComponent<MeshRenderer>();
                    segmentsOffset = 2 * renderer.bounds.extents.y;
                }
            }
        }
    }

    void Start()
    {
        scroll = true;
    }

    public void SetScroll(bool value)
    {
        scroll = value;
    }
    
    void LateUpdate()
    {
        if (scroll)
        {
            float scrollSpeed = player.GetComponent<Player>().baseFallSpeed + player.GetComponent<Player>().tiltAddedVerticalSpeed;

            if (loopSegments)
            {
                // loop the child objects to simulate a scrolling effect 
                List<Transform> segmentsToWrapAround = new List<Transform>();
                foreach (Transform segmentTransform in transform.Find("Segments"))
                {
                    float riseSpeed = player.GetComponent<Player>().baseFallSpeed + player.GetComponent<Player>().tiltAddedVerticalSpeed;
                    segmentTransform.position = segmentTransform.position + (Vector3.up * riseSpeed * Time.deltaTime);
                    if (segmentTransform.position.y >= player.transform.position.y + segmentDespawnHeight)
                    {
                        segmentsToWrapAround.Add(segmentTransform);
                    }
                }
                segmentsToWrapAround.Sort( (Transform a, Transform b) => (a.position.y > b.position.y) ? -1 : 1 );
                foreach (Transform segmentTransform in segmentsToWrapAround)
                {
                    segmentTransform.position = new Vector3(segmentTransform.position.x,
                        lowestSegment.position.y - segmentsOffset,
                        segmentTransform.position.z);
                    lowestSegment = segmentTransform;
                }
            }
            else
            {
                // the below will scroll the texture on the wall
                rend.material.mainTextureOffset = rend.material.mainTextureOffset + (Vector2.up * scrollSpeed * Time.deltaTime);
            }
        }
    }
}
