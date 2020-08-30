using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidingDoubleDoor : MonoBehaviour
{
    public void Open()
    {
        foreach (Transform child in transform)
        {
            GameObject childObject = child.gameObject;
            SlidingDoor childScript = childObject.GetComponent<SlidingDoor>();
            childScript.Open();
        }
    }

    public void Close()
    {
        foreach (Transform child in transform)
        {
            GameObject childObject = child.gameObject;
            SlidingDoor childScript = childObject.GetComponent<SlidingDoor>();
            childScript.Close();
        }
    }

    public void Oscillate()
    {
        foreach (Transform child in transform)
        {
            GameObject childObject = child.gameObject;
            SlidingDoor childScript = childObject.GetComponent<SlidingDoor>();
            childScript.Oscillate();
        }
    }
}
