using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleProjector : MonoBehaviour
{
    private Player player;

    void Awake()
    {
        foreach (Transform projectorTransform in transform)
        {
            GameObject projectorObject = projectorTransform.gameObject;
            projectorObject.GetComponent<Projector>().material = new Material(projectorObject.GetComponent<Projector>().material);
        }
    }

    void Update()
    {
        //TODO fix below to fade in projector as it approaches player
        /*foreach (Transform projectorTransform in transform)
        {
            GameObject projectorObject = projectorTransform.gameObject;
            Color prev = projectorObject.GetComponent<Projector>().material.color;
            //Debug.Log("a is " + projectorObject.GetComponent<Projector>().material.color.a);
            prev.a = 0f;
            projectorObject.GetComponent<Projector>().material.color = prev;
            //projectorObject.GetComponent<Projector>().material.SetColor("_Color", prev);
        }*/
        if (transform.position.y >= player.transform.position.y)
        {
            Destroy(gameObject);
        }

    }

    public void SetPlayer(Player player)
    {
        this.player = player;
    }
}
