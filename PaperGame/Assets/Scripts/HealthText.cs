using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthText : MonoBehaviour
{
    public Player player;

    void Update()
    {
        GetComponent<Text>().text = string.Format("HP:  {0}", player.getHealth());
    }
}
