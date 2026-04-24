using UnityEngine;
using System;
using System.Collections.Generic;

public class Duplicate : MonoBehaviour
{
    // This is the starting global Y size (in unity units) of the original object
    private float startSize = 0.1f;
    // This is the target global Y size (in unity units) of the resulting group of objects
    [SerializeField]
    private float targetSize = 0.1f;
    // This is the percent above which a little extra needed size constitutes adding an additional duplicate
    private readonly float BUFFER = 0.05f;
    // These are the Script Components which are problematic if included in the duplication.
    // After duplication, these components are removed
    private List<Type> dontDuplicateTypes = new List<Type>
    {
        typeof(Duplicate),
        typeof(Rotate)
    };

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Vector3 boundsSize = CalculateAggregateSize();
        startSize = boundsSize.y;

        int numDuplicates = 1;
        if (targetSize > startSize)
        {
            numDuplicates = (int)Math.Floor(targetSize / startSize);
            // add one extra duplicate to handle some remainder desired size
            if (targetSize % startSize > 0 + (startSize * BUFFER))
            {
                numDuplicates++;
            }
        }

        // Debug.Log("for object: " + gameObject.name + " start size: " + startSize + " target size: " + targetSize +
        // "\n" + "x: "+ boundsSize.x + " y: " + boundsSize.y + " z: " + boundsSize.z + "\nnumduplicates: " + numDuplicates);

        List<GameObject> duplicates = new List<GameObject>();
        for (int i = 1; i < numDuplicates; i++)
        {
            GameObject duplicate = Instantiate(gameObject);
            duplicate.name = $"{gameObject.name} (Duplicate {i})";
            duplicate.transform.Translate(new Vector3(0, startSize * -1, 0) * i);
            // Remove problematic Script Components in the duplicated object
            foreach (Type type in dontDuplicateTypes)
            {
                if (duplicate.GetComponent(type) != null)
                {
                    Destroy(duplicate.GetComponent(type));
                }
            }
            duplicates.Add(duplicate);
            // Debug.Log("for " + gameObject.name + "num dups: " + numDuplicates + " i: " + i
            // + "sizeoflist: " + duplicates.Count );
        }
        foreach (GameObject duplicate in duplicates)
        {
            duplicate.transform.SetParent(transform);
            // Debug.Log("dup name is " + duplicate.name + " and parent name is: " + duplicate.transform.parent.gameObject.name);
        }
    }

    public Vector3 CalculateAggregateSize()
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();

        if (colliders.Length == 0)
        {
            Debug.LogError("No colliders found on this GameObject or its children.");
            return new Vector3(0f, 0.1f, 0f);
        }

        // Initialize bounds with the first collider found
        Bounds bounds = colliders[0].bounds;

        // Encapsulate the bounds of all subsequent colliders
        for (int i = 1; i < colliders.Length; i++)
        {
            bounds.Encapsulate(colliders[i].bounds);
        }

        return bounds.size;
    }

}

