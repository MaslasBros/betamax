namespace BetaMax.Posts
{
    using System.IO;
    using System.Net;
    using BetaMax.Core;

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

            SubmissionHandler.Log("Set server info");
        }

        public void UploadFile()
        {
            SubmissionHandler.Log("Starting file upload");
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

            FtpWebResponse response = (FtpWebResponse)request.GetResponse();
            SubmissionHandler.Log("Upload succeeded: " + response.StatusDescription);
            response.Close();
        }
    }
}