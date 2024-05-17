using System;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteAlways]
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

    private Queue<GameObject> tileQueue;
    const float TUNNEL_2_SCALE_RATIO = 11;

    void Awake()
    {
        tileQueue = new Queue<GameObject>();
        if (Application.isPlaying)
        {
            //Debug.Log("creating in play mode");
            int numTiles = (int) Math.Ceiling(1f / tileToTunnelRatio) + 1;
            for (int i=0; i<numTiles; i++)
            {
                GameObject tile = Instantiate(tilePrefab, transform, false);

                tile.transform.position =   tile.transform.position +
                                            (   transform.right * tile.transform.localScale.x *
                                                TUNNEL_2_SCALE_RATIO * tileToTunnelRatio * i);
                tile.transform.localScale = tile.transform.localScale * tileToTunnelRatio;
                tileQueue.Enqueue(tile);
            }
        }
        else if (Application.isEditor)
        {
            //Debug.Log("creating in editor");
            //visualAideList.Add(Instantiate(streamObstaclePrefab, transform, false));
        }
    }

    public static GameObject SafeDestroy(GameObject obj)
    {
        if (Application.isPlaying)
        {
            //Debug.Log("destroying in play mode");
            Destroy(obj);
        }
        else if (Application.isEditor)
        {
            //TODO DestroyImmediate below does not work.
            //     Fix, then restore the instantiate in editor and ExecuteAlways attribute

            //Debug.Log("destroying in editor");
            DestroyImmediate(obj.gameObject);
        }

        return null;
    }

    public static GameObject SafeDestroyGameObject(GameObject gameobject)
    {
        if (gameobject != null)
            SafeDestroy(gameobject);
        return null;
    }

    void OnDestroy()
    {
        //if (Application.isPlaying)
        //    Debug.Log("ondestroy called from play mode");
        //else if (Application.isEditor)
        //    Debug.Log("ondestroy called from editor");

        //for (int index = 0; index < tileQueue.Count; index++)
        //{
        //    tileQueue[index] = SafeDestroyGameObject(tileQueue[index]);
        //}
        while (tileQueue.Count > 0)
        {
            SafeDestroyGameObject(tileQueue.Dequeue());
        }

    }

    void Update()
    {
        if (Application.isPlaying)
        {
            foreach (GameObject tile in tileQueue)
            {
                tile.transform.position =   tile.transform.position +
                                            (   transform.right * -1 * tilePrefab.transform.localScale.x *
                                                TUNNEL_2_SCALE_RATIO * speed *
                                                Time.deltaTime);
            }
        }

        float tileOffset =  (   tileQueue.Peek().transform.position.x +
                                (tilePrefab.transform.localScale.x * TUNNEL_2_SCALE_RATIO * tileToTunnelRatio)) -
                            transform.position.x;

        if ( tileOffset <= 0 )
        {
            int numTiles = tileQueue.Count;
            GameObject tileToRotate = tileQueue.Dequeue();
            tileToRotate.transform.position =   tileToRotate.transform.position +
                                                (   transform.right * tilePrefab.transform.localScale.x *
                                                    TUNNEL_2_SCALE_RATIO * tileToTunnelRatio * numTiles);
            tileQueue.Enqueue(tileToRotate);
        } 

    }
}
