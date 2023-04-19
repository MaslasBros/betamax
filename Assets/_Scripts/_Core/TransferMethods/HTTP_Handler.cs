namespace BetaMax.Posts
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;

    using BetaMax.Core;

    using UnityEngine;
    using UnityEngine.Networking;

    ///<summary>
    /// A class used to upload the files to a FTP server.
    /// </summary>
    /// * The UploadFile() method can be freely modified to suit your needs.
    public class HTTP_Handler : MonoBehaviour
    {
        ///<summary>The server url</summary>
        string url;
        ///<summary>The local file location</summary>
        string fileLocation;

        ///<summary>The server login username</summary>
        string username;
        ///<summary>The server login password.</summary>
        string password;

        Action<bool> uploadResultCallback;

        ///<summary>Sets the server login info</summary>
        public void SetServerInfo(string url, string fileLocation, string username, string password)
        {
            this.url = url;
            this.fileLocation = fileLocation;

            this.username = username;
            this.password = password;

            SubmissionHandler.Log("Info set for HTTP(S)");
        }

        ///<summary>Start server uploading with </summary>
        public void UploadFile(Action<bool> uploadResultCallback)
        {
            this.uploadResultCallback = uploadResultCallback;

            StartCoroutine(HandleFileTransfer());
        }

        ///<summary>Call to begin the user file post to the HTTP provided server.</summary>
        public IEnumerator HandleFileTransfer()
        {
            SubmissionHandler.Log("Starting file upload");

            byte[] fileBytes = File.ReadAllBytes(fileLocation);

            UnityWebRequest www = UnityWebRequest.PostWwwForm(url, "POST");
            www.uploadHandler = new UploadHandlerRaw(fileBytes);
            www.SetRequestHeader("Content-Type", "application/zip");

            string auth = Convert.ToBase64String(Encoding.ASCII.GetBytes(username + ":" + password));
            www.SetRequestHeader("Authorization", "Basic " + auth);

            UnityWebRequestAsyncOperation operation = www.SendWebRequest();
            while (!operation.isDone)
            {
                yield return null;
            }

            if (www.result != UnityWebRequest.Result.Success)
            {
                SubmissionHandler.Log($"Failed to upload zip file: {www.error}");

                uploadResultCallback?.Invoke(false);
            }
            else
            {
                SubmissionHandler.Log("Zip file uploaded successfully.");

                uploadResultCallback?.Invoke(true);
            }

            Destroy(this);
        }

        private void OnDestroy()
        {
            StopAllCoroutines();
        }
    }
}