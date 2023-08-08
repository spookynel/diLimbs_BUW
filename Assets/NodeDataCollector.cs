using System.Collections.Generic;
using UnityEngine;

public class NodeDataCollector : MonoBehaviour
{
    private List<Transform> nodeTransforms;
    private List<List<float>> nodeDataList;

    private int frameCount = 0;
    private int printInterval = 30;

    private void Start()
    {
        // Initialize the lists
        nodeTransforms = new List<Transform>();
        nodeDataList = new List<List<float>>();

        // Find all the specified nodes and store their transforms
        FindNodes(transform);
    }

    private void FindNodes(Transform root)
    {
        // Traverse the hierarchy and find the specified nodes
        foreach (Transform child in root)
        {
            string nodeName = child.name;

            if (nodeName.StartsWith("RightFinger"))
            {
                nodeTransforms.Add(child);
            }

            // Recursively traverse the children
            FindNodes(child);
        }
    }

    private void Update()
    {
        frameCount++;

        // Collect position and rotation data every 30 frames
        if (frameCount % printInterval == 0)
        {
            List<float> frameData = new List<float>();

            foreach (Transform nodeTransform in nodeTransforms)
            {
                // Store the position and rotation data as floats
                frameData.Add(nodeTransform.position.x);
                frameData.Add(nodeTransform.position.y);
                frameData.Add(nodeTransform.position.z);
                frameData.Add(nodeTransform.rotation.x);
                frameData.Add(nodeTransform.rotation.y);
                frameData.Add(nodeTransform.rotation.z);
                frameData.Add(nodeTransform.rotation.w);
            }

            // Add the frame data to the node data list
            nodeDataList.Add(frameData);

            // Print the node data list every 30 frames
            if (frameCount % (printInterval * 10) == 0)
            {
                PrintNodeDataList();
            }
        }
    }

    private void PrintNodeDataList()
    {
        Debug.Log("Node Data List:");

        foreach (List<float> frameData in nodeDataList)
        {
            Debug.Log("Frame:");

            for (int i = 0; i < frameData.Count; i += 7)
            {
                Debug.Log($"Node {i / 7 + 1} Position: ({frameData[i]}, {frameData[i + 1]}, {frameData[i + 2]})");
                Debug.Log($"Node {i / 7 + 1} Rotation: ({frameData[i + 3]}, {frameData[i + 4]}, {frameData[i + 5]}, {frameData[i + 6]})");
            }
        }
    }
}
