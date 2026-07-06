using System;
using UnityEngine;

public class ChangeSinCos : MonoBehaviour
{
    private DrawGraph drawGraph;

    private void Awake()
    {
        drawGraph = GetComponent<DrawGraph>();
    }
    
    public void ChangeType(int index)
    {
        if (index == 0)
            drawGraph.ChangeType(true);
        else if (index == 1)
            drawGraph.ChangeType(false);
    }
}
