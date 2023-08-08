using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

public class CSVReader : MonoBehaviour
{
    public string folderPath = "C:/Users/anabe/My project/Assets/ASL_avg";

    void Start()
    {
        string[] filePaths = Directory.GetFiles(folderPath, "*.csv");

        // Task 1: Read the FingerName values from the first file
        string[] fingerNames = ReadFingerNames(filePaths[0]);

        foreach (string filePath in filePaths)
        {
            // Task 2: Create vector of vectors for each file
            List<List<float>> dataVectors = ReadCSVFile(filePath);

            // Task 3: Print the finger names and file information
            Debug.Log("Finger Names: " + string.Join(", ", fingerNames));
            Debug.Log("File: " + Path.GetFileName(filePath));
            foreach (List<float> rowVector in dataVectors)
            {
                Debug.Log(string.Join(", ", rowVector));
            }
        }
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
}
