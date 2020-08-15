using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tutorial : MonoBehaviour
{
    public Player player;
    private Transform collectables;

    // Start is called before the first frame update
    void Start()
    {
        collectables = transform.Find("Collectables");
    }

    // Update is called once per frame
    void Update()
    {
        bool allCollected = true;
        foreach (Transform collectable in collectables)
        {
            if (collectable.gameObject.activeSelf)
                allCollected = false;
        }
        if (allCollected)
        {
            player.OnTutorialComplete();
        }
    }
}
