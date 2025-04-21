using UnityEngine;

public class ChildCounter : MonoBehaviour
{
    [ContextMenu("Count Nested Children")]
    public int CountNestedChildren()
    {
        int totalChildCount = 0;

        foreach (Transform child in transform)
        {
            totalChildCount += child.childCount;
        }
        Debug.Log("Total child objects inside the children (grandchildren): " + totalChildCount);
        return totalChildCount;
        
    }

    [ContextMenu("Count Direct Children")]
    public int CountDirectChildren()
    {
        int directChildCount = transform.childCount;
        Debug.Log("Direct child objects: " + directChildCount);
        return directChildCount;
        
    }
}
