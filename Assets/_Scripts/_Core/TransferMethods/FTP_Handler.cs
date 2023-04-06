namespace BetaMax.Posts
{
    using System.IO;
    using System.Net;
    using UnityEngine;

    public class FTP_Handler
    {
        string url;
        string fileLocation;

        string username;
        string password;

        public FTP_Handler(string url, string fileLocation, string username, string password)
        {
            this.url = url;
            this.fileLocation = fileLocation;

            this.username = username;
            this.password = password;

            Debug.Log("Set server info");
        }

        public void UploadFile()
        {
            Debug.Log("Starting file upload");
            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
            request.Credentials = new NetworkCredential(username, password);
            request.Method = WebRequestMethods.Ftp.UploadFile;

            using (FileStream fileStream = new FileStream(fileLocation, FileMode.Open))
            {
                using (Stream stream = request.GetRequestStream())
                {
                    // Upload the zip file to the FTP server
                    fileStream.CopyTo(stream);
                }
            }

            var response = (FtpWebResponse)request.GetResponse();
            Debug.Log("Upload succeeded: " + response.StatusDescription);
            response.Close();
        }
    }
}