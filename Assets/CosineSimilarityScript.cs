using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Globalization;

public class CosineSimilarityScript : MonoBehaviour
{
    public float threshold = 0.8f;
    private List<List<float>> input;
    private List<(string, List<float>)> references;

    private void Update()
    {
        if (Time.frameCount % 30 == 0)
        {
            input = CollectNodeData();
            references = ReadCSVFiles("C:/Users/anabe/My project/Assets/ASL_avg");
            CompareLists();
            
        }
    }

    private List<List<float>> CollectNodeData()
    {
        List<Transform> nodeTransforms = new List<Transform>();
        List<List<float>> nodeDataList = new List<List<float>>();

        FindNodes(transform);

        void FindNodes(Transform root)
        {
            foreach (Transform child in root)
            {
                string nodeName = child.name;

                if (nodeName.StartsWith("RightFinger"))
                {
                    nodeTransforms.Add(child);
                }

                FindNodes(child);
            }
        }

        List<float> frameData = new List<float>();

        foreach (Transform nodeTransform in nodeTransforms)
        {
            frameData.Add(nodeTransform.position.x);
            frameData.Add(nodeTransform.position.y);
            frameData.Add(nodeTransform.position.z);
            frameData.Add(nodeTransform.rotation.x);
            frameData.Add(nodeTransform.rotation.y);
            frameData.Add(nodeTransform.rotation.z);
            frameData.Add(nodeTransform.rotation.w);
        }

        nodeDataList.Add(frameData);

        return nodeDataList;
    }

    private string[] ReadFingerNames(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        string[] fingerNames = new string[lines.Length - 1]; // Exclude header

        for (int i = 1; i < lines.Length; i++) // Start from index 1 to skip the header
        {
            string[] values = lines[i].Split(',');
            fingerNames[i - 1] = values[0];
        }

        return fingerNames;
    }

    private List<List<float>> ReadCSVFile(string filePath)
    {
        string[] lines = File.ReadAllLines(filePath);
        List<List<float>> dataVectors = new List<List<float>>();

        CultureInfo culture = CultureInfo.InvariantCulture;

        for (int i = 1; i < lines.Length; i++) // Start from index 1 to skip the header
        {
            string[] values = lines[i].Split(',');

            List<float> rowData = new List<float>();
            for (int j = 1; j < values.Length; j++) // Start from index 1 to skip the FingerName column
            {
                float value;
                if (float.TryParse(values[j], NumberStyles.Any, culture, out value))
                {
                    rowData.Add(value);
                }
            }

            dataVectors.Add(rowData);
        }

        return dataVectors;
    }

    private List<(string, List<float>)> ReadCSVFiles(string folderPath)
    {
        List<(string, List<float>)> dataVectors = new List<(string, List<float>)>();

        string[] filePaths = Directory.GetFiles(folderPath, "*.csv");

        string[] fingerNames = ReadFingerNames(filePaths[0]);

        foreach (string filePath in filePaths)
        {
            List<List<float>> fileDataVectors = ReadCSVFile(filePath);
            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string alphanumericPart = Regex.Match(fileName, @"[\w\d]+").Value;

            dataVectors.Add((alphanumericPart, fileDataVectors[0]));
        }

        return dataVectors;
    }

    private void CompareLists()
    {
        string mostSimilarReference = null;
        float maxSimilarity = float.MinValue;

        foreach (var reference in references)
        {
            float similarity = CalculateCosineSimilarity(input, reference.Item2);

            if (similarity > threshold && similarity > maxSimilarity)
            {
                maxSimilarity = similarity;
                mostSimilarReference = reference.Item1;
            }
        }

        if (mostSimilarReference != null)
        {
            Debug.Log("Most similar reference: " + mostSimilarReference);
        }
    }


    private float CalculateCosineSimilarity(List<List<float>> list1, List<float> list2)
    {
        Vector2 inputVector = new Vector2(list1.Sum(x => x.Sum()), list1.Count);
        Vector2 referenceVector = new Vector2(list2.Sum(), list2.Count);

        float dotProduct = Vector2.Dot(inputVector, referenceVector);
        float inputMagnitude = inputVector.magnitude;
        float referenceMagnitude = referenceVector.magnitude;

        float similarity = dotProduct / (inputMagnitude * referenceMagnitude);

        return similarity;
    }
}
