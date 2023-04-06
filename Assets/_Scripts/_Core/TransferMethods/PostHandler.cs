using BetaMax.Core;
using UnityEngine;

namespace BetaMax.Posts
{
    public class PostHandler
    {
        string url;
        string fileLocation;

        string username;
        string password;

        SFTP_Handler handler;

        public PostHandler(string url, string fileLocation, string username, string password)
        {
            this.url = url;
            this.fileLocation = fileLocation;

            this.username = username;
            this.password = password;
        }

        public void BeginFileTransfer()
        {
            if (url.StartsWith("http://") || url.StartsWith("https://"))
            {
                Debug.Log("Is HTTP(S)");

                HTTP_Handler hTTP_Handler = SubmissionHandler.S.gameObject.AddComponent<HTTP_Handler>();
                hTTP_Handler.SetServerInfo(url, fileLocation, username, password);
                hTTP_Handler.UploadFile();
            }
            else if (url.StartsWith("ftp://"))
            {
                Debug.Log("Is FTP");
                FTP_Handler fTP_Handler = new FTP_Handler(url, fileLocation, username, password);
                fTP_Handler.UploadFile();
            }
            else if (url.StartsWith("sftp://"))
            {
                Debug.Log("Is SFTP");
                handler = new SFTP_Handler(url, fileLocation, username, password);
                handler.UploadFile();
            }
        }
    }
}