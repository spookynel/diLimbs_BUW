using UnityEngine;
using System.IO;
using System.Text;
using System.Globalization;

public class AnimationDataRecorder : MonoBehaviour
{
    public int startFrame = 0;
    public int endFrame = 0;

    private string outputFilePath;

    private StringBuilder csvBuilder;
    private bool recordingData;

    private Transform rootTransform;

    private void Start()
    {
        csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("Frame,FingerName,PositionX,PositionY,PositionZ,RotationX,RotationY,RotationZ,RotationW");

        recordingData = (startFrame == 0 && endFrame == 0);
        rootTransform = transform;
    }

    private void Update()
    {
        if (!recordingData)
            return;

        int currentFrame = Time.frameCount;

        if (startFrame != 0 && currentFrame < startFrame)
            return;

        if (endFrame != 0 && currentFrame > endFrame)
        {
            SaveDataToFile();
            return;
        }

        TraverseChildren(rootTransform, currentFrame);

        Debug.Log($"Processed frame: {currentFrame}");
    }

    private void TraverseChildren(Transform currentTransform, int frameIndex)
    {
        if (currentTransform != rootTransform)
        {
            Vector3 localPosition = currentTransform.localPosition - rootTransform.localPosition;
            Quaternion localRotation = Quaternion.Inverse(rootTransform.localRotation) * currentTransform.localRotation;

            string fingerName = currentTransform.name;
            string positionX = localPosition.x.ToString("0.0######", CultureInfo.InvariantCulture);
            string positionY = localPosition.y.ToString("0.0######", CultureInfo.InvariantCulture);
            string positionZ = localPosition.z.ToString("0.0######", CultureInfo.InvariantCulture);
            string rotationX = localRotation.x.ToString("0.0######", CultureInfo.InvariantCulture);
            string rotationY = localRotation.y.ToString("0.0######", CultureInfo.InvariantCulture);
            string rotationZ = localRotation.z.ToString("0.0######", CultureInfo.InvariantCulture);
            string rotationW = localRotation.w.ToString("0.0######", CultureInfo.InvariantCulture);

            csvBuilder.AppendLine($"{frameIndex},{fingerName},{positionX},{positionY},{positionZ},{rotationX},{rotationY},{rotationZ},{rotationW}");
        }

        foreach (Transform child in currentTransform)
        {
            TraverseChildren(child, frameIndex);
        }
    }

    private void SaveDataToFile()
    {
        string dateTimeStamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = "NewRightHand_" + dateTimeStamp + ".csv";
        string filePath = Path.Combine("D:\\Usuarios\\kerke\\Escritorio\\DiLimbs", fileName);

        File.WriteAllText(filePath, csvBuilder.ToString());

        Debug.Log($"Animation data saved to: {filePath}");
        recordingData = false;
    }

    private void OnApplicationQuit()
    {
        if (recordingData)
        {
            SaveDataToFile();
        }
    }
}
