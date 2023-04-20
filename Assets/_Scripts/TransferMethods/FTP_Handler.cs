namespace BetaMax.Posts
{
    using System;
    using System.IO;
    using System.Net;

    using BetaMax.Core;

    ///<summary>
    /// A class used to upload the files to a FTP server.
    /// </summary>
    /// * The UploadFile() method can be freely modified to suit your needs.
    public class FTP_Handler
    {
        ///<summary>The server URL</summary>
        string url;
        ///<summary>The string that will be replaced from the zips actual name.</summary>
        string urlReplaceStr = "{fileName}";
        ///<summary>The local file location</summary>
        string fileLocation;

        ///<summary>Server username</summary>
        string username;
        ///<summary>Server password</summary>
        string password;

        public FTP_Handler(string url, string fileLocation, string username, string password)
        {
            this.url = url.Replace(urlReplaceStr, SubmissionHandler.S.MAIN_ZIP_NAME);
            this.fileLocation = fileLocation;

            this.username = username;
            this.password = password;

            SubmissionHandler.Log("Set server info for FTP");
        }

        ///<summary>
        /// Start uploading with FTP protocol.
        /// </summary>
        public void UploadFile(out bool result)
        {
            try
            {
                SubmissionHandler.Log("Starting FTP file upload");

                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(url);
                request.Credentials = new NetworkCredential(username, password);
                request.Method = WebRequestMethods.Ftp.UploadFile;

                using (FileStream fileStream = new FileStream(fileLocation, FileMode.Open))
                {
                    using (Stream stream = request.GetRequestStream())
                    {
                        fileStream.CopyTo(stream);
                    }
                }

                FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                SubmissionHandler.Log("Upload succeeded: " + response.StatusDescription);
                response.Close();

                result = true;
            }
            catch (Exception e)
            {
                result = false;
                SubmissionHandler.Log(e.ToString());
            }
        }
    }
}