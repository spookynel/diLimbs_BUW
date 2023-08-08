using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SaveHierarchyOnExit : MonoBehaviour
{
    private const string OutputFilePath = @"D:\Usuarios\kerke\Escritorio\DiLimbs\hierarchy.json";

    private void OnApplicationQuit()
    {
        SaveHierarchyToJson();
    }

    private void SaveHierarchyToJson()
    {
        // Create a data structure to hold the hierarchy information
        HierarchyData hierarchyData = new HierarchyData();
        PopulateHierarchyData(hierarchyData, transform);

        // Convert the hierarchy data to JSON
        string jsonData = JsonUtility.ToJson(hierarchyData, true);

        // Save the JSON data to a file
        File.WriteAllText(OutputFilePath, jsonData);

        // Refresh the Unity editor to show the exported file
        AssetDatabase.Refresh();
    }

    private void PopulateHierarchyData(HierarchyData parentData, Transform transform)
    {
        // Create a new HierarchyNode for the current transform
        HierarchyNode node = new HierarchyNode();
        node.name = transform.name;

        // Add child nodes recursively
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            HierarchyData childData = new HierarchyData(); 
            PopulateHierarchyData(childData, child);
            node.children.Add(childData);
        }

        // Add the current node to the parent data
        parentData.nodes.Add(node);
    }
}

[System.Serializable]
public class HierarchyData
{
    public List<HierarchyNode> nodes = new List<HierarchyNode>();
}

[System.Serializable]
public class HierarchyNode
{
    public string name;
    public List<HierarchyData> children = new List<HierarchyData>();
}
