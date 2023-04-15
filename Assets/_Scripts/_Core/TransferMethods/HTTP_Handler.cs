namespace BetaMax.Posts
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using BetaMax.Core;
    using UnityEngine;
    using UnityEngine.Networking;

    public class HTTP_Handler : MonoBehaviour
    {
        string url;
        string fileLocation;

        string username;
        string password;

        public void SetServerInfo(string url, string fileLocation, string username, string password)
        {
            this.url = url;
            this.fileLocation = fileLocation;

            this.username = username;
            this.password = password;

            SubmissionHandler.Log("Set server info");
        }

        public void UploadFile()
        {
            SubmissionHandler.Log("Starting file upload");
            StartCoroutine(HandleFileTransfer());
        }

        ///<summary>Call to begin the user file post to the HTTP provided server.</summary>
        public IEnumerator HandleFileTransfer()
        {
            byte[] fileBytes = File.ReadAllBytes(fileLocation);

            UnityWebRequest www = UnityWebRequest.PostWwwForm(url, "POST");
            www.uploadHandler = new UploadHandlerRaw(fileBytes);
            www.SetRequestHeader("Content-Type", "application/zip");

            string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            www.SetRequestHeader("Authorization", "Basic " + auth);

            UnityWebRequestAsyncOperation operation = www.SendWebRequest();
            while (!operation.isDone)
            {
                SubmissionHandler.Log("Upload in progress");
                yield return null;
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                throw new Exception($"Failed to upload zip file: {www.error}");
            }

            SubmissionHandler.Log("Zip file uploaded successfully.");

            Destroy(this);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}