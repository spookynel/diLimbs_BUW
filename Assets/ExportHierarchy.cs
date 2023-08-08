using UnityEngine;
using System.Collections;
using System.IO;

public class ExportHierarchy : MonoBehaviour
{
    private const string SAVE_PATH = @"D:\Usuarios\kerke\Escritorio\DiLimbs\";

    void Start()
    {
        string fileName = "hierarchy" + System.DateTime.Now.ToString("yyyyMMddHHmmss") + ".txt";
        string filePath = Path.Combine(SAVE_PATH, fileName);

        using (StreamWriter writer = new StreamWriter(filePath))
        {
            WriteHierarchyToFile(transform, writer, 0);
        }

        Debug.Log("Hierarchy exported to: " + filePath);
    }

    private void WriteHierarchyToFile(Transform node, StreamWriter writer, int depth)
    {
        writer.WriteLine(new string('-', depth) + node.name);

        for (int i = 0; i < node.childCount; i++)
        {
            Transform child = node.GetChild(i);
            WriteHierarchyToFile(child, writer, depth + 1);
        }
    }
}
