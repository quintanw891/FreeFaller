using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wall : MonoBehaviour
{
    public Player player;
    public float scrollSpeed;
    private Renderer rend;
    private bool scroll;

    void Awake()
    {
        rend = GetComponent<Renderer>();
    }

    void Start()
    {
        scroll = true;
    }

    public void SetScroll(bool value)
    {
        scroll = value;
    }
    
    void Update()
    {
        if (scroll)
        {
            float scrollSpeed = player.GetComponent<Player>().baseFallSpeed + player.GetComponent<Player>().tiltAddedVerticalSpeed;
            rend.material.mainTextureOffset = rend.material.mainTextureOffset + (Vector2.up * scrollSpeed * Time.deltaTime);
        }

    }
}
