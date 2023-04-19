namespace BetaMax.Posts
{
    using System;
    using System.IO;

    using Renci.SshNet;

    using BetaMax.Core;

    ///<summary>
    /// A class used to upload the files to a SFTP server.
    /// </summary>
    /// * The UploadFile() method can be freely modified to suit your needs.
    public class SFTP_Handler
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

        public SFTP_Handler(string url, string fileLocation, string username, string password)
        {
            this.url = url.Replace(urlReplaceStr, SubmissionHandler.S.MAIN_ZIP_NAME);
            this.fileLocation = fileLocation;

            this.username = username;
            this.password = password;

            SubmissionHandler.Log("Set server info for SFTP");
        }

        ///<summary>Start uploading with SFTP protocol.</summary>
        public void UploadFile(out bool result)
        {
            try
            {
                SubmissionHandler.Log("Starting SFTP file upload");
                Uri uri = new Uri(url);
                string hostUrl = uri.Host;
                string remoteFilePath = uri.LocalPath;

                //Differentiate between relative and absolute paths.
                if (!remoteFilePath.StartsWith("//"))
                { remoteFilePath = remoteFilePath.TrimStart('/'); }

                ConnectionInfo connInfo = new ConnectionInfo(hostUrl, username, new PasswordAuthenticationMethod(username, password));
                SftpClient sftp = new SftpClient(connInfo);
                sftp.Connect();

                SubmissionHandler.Log("Is SFTP connected: " + sftp.IsConnected.ToString());

                SubmissionHandler.Log("Remote path: " + remoteFilePath);
                using (FileStream fileStream = new FileStream(fileLocation, FileMode.Open))
                {
                    sftp.UploadFile(fileStream, remoteFilePath);
                }

                sftp.Disconnect();

                result = true;
            }
            catch (System.Exception e)
            {
                SubmissionHandler.Log(e.ToString());

                result = false;
            }
        }
    }
}