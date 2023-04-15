namespace BetaMax.Posts
{
    using System.IO;
    using BetaMax.Core;
    using Renci.SshNet;

    public class SFTP_Handler
    {
        string url;
        string fileLocation;

        string username;
        string password;

        public SFTP_Handler(string url, string fileLocation, string username, string password)
        {
            this.url = url;
            this.fileLocation = fileLocation;

            this.username = username;
            this.password = password;
        }

        public void UploadFile()
        {
            SubmissionHandler.Log("Starting file upload");

            try
            {
                ConnectionInfo connInfo = new ConnectionInfo(url, username, new PasswordAuthenticationMethod(username, password));
                SftpClient sftp = new SftpClient(connInfo);
                sftp.Connect();

                SubmissionHandler.Log(sftp.IsConnected.ToString());

                string remoteFilePath = "/home/michael/tests/file.zip";
                using (FileStream fileStream = new FileStream(fileLocation, FileMode.Open))
                {
                    sftp.UploadFile(fileStream, remoteFilePath);
                }
            }
            catch (System.Exception e)
            {
                SubmissionHandler.Log(e.ToString());
            }
        }
    }
}