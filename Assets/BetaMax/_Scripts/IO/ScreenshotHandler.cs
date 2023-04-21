namespace BetaMax.Core.IO
{
    using System.IO;
    using UnityEngine;

    public class ScreenshotHandler : MonoBehaviour
    {
        string screenshotName = "scrIssue.jpg";
        string tempFolderFinalPath = string.Empty;

        public string ScreenshotPath => Path.Combine(tempFolderFinalPath, screenshotName);

        private void Start()
        {
            tempFolderFinalPath = Path.Combine(SubmissionHandler.S.TEMP_FOLDER_PATH, SubmissionHandler.S.TEMP_FOLDER_NAME);
        }

        ///<summary>Saves a jpg image from the passed camera at the users device</summary>
        public void CaptureScreenshot(Camera screenshotCamera)
        {
            RenderTexture cameraTarget = new RenderTexture(screenshotCamera.pixelWidth, screenshotCamera.pixelHeight, ((int)screenshotCamera.depth), RenderTextureFormat.ARGB32);
            screenshotCamera.targetTexture = cameraTarget;

            screenshotCamera.Render();

            byte[] imgBytes = ActiveTextureToBytes(screenshotCamera.targetTexture, screenshotCamera);

            CreateImageFile(imgBytes, tempFolderFinalPath, screenshotName);

            screenshotCamera.targetTexture = null;
        }

        ///<summary>Converts the passed render texture to a bytes array.</summary>
        byte[] ActiveTextureToBytes(RenderTexture cameraTarget, Camera screenshotCamera)
        {
            RenderTexture.active = cameraTarget;
            Texture2D texture = new Texture2D(screenshotCamera.pixelWidth, screenshotCamera.pixelHeight, TextureFormat.ARGB32, false, false);
            texture.ReadPixels(new Rect(0, 0, screenshotCamera.pixelWidth, screenshotCamera.pixelHeight), 0, 0);
            texture.Apply();

            return texture.EncodeToJPG(100);
        }

        ///<summary>Creates a jpg image file to the specified path.</summary>
        void CreateImageFile(byte[] imgBytes, string path, string filename)
        {
            string finalPath = Path.Combine(path, filename);

            File.WriteAllBytes(finalPath, imgBytes);
        }
    }
}